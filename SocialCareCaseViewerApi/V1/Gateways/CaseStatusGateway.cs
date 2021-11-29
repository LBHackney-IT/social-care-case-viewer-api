using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class CaseStatusGateway : ICaseStatusGateway
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ISystemTime _systemTime;

        public CaseStatusGateway(DatabaseContext databaseContext, ISystemTime systemTime)
        {
            _databaseContext = databaseContext;
            _systemTime = systemTime;
        }

        public CaseStatus? GetCasesStatusByCaseStatusId(long id)
        {
            return _databaseContext.CaseStatuses
                .Where(cs => cs.Id == id)
                .Include(cs => cs.Person)
                .Include(cs => cs.Answers)
                .FirstOrDefault()
                ?.ToDomain();
        }

        public List<CaseStatus> GetActiveCaseStatusesByPersonId(long personId)
        {
            var caseStatuses = _databaseContext
                .CaseStatuses
                .Where(cs => cs.PersonId == personId)
                .Where(cs => cs.EndDate == null || cs.EndDate > DateTime.Today)
                .Include(cs => cs.Answers)
                .Include(cs => cs.Person).ToList();

            foreach (var caseStatus in caseStatuses)
            {
                if (caseStatus.Answers != null)
                {
                    var caseAnswers = new List<CaseStatusAnswer>();

                    foreach (var answer in caseStatus.Answers)
                    {
                        if (answer.DiscardedAt == null)
                        {
                            caseAnswers.Add(answer);
                        }
                    }
                    caseStatus.Answers = caseAnswers;
                }
            }

            return caseStatuses.Select(caseStatus => caseStatus.ToDomain()).ToList();
        }

        public List<CaseStatus> GetCaseStatusesByPersonId(long personId)
        {
            var caseStatuses = _databaseContext
                .CaseStatuses
                .Where(cs => cs.PersonId == personId)
                .Include(cs => cs.Answers)
                .Include(cs => cs.Person).ToList();

            return caseStatuses.Select(caseStatus => caseStatus.ToDomain()).ToList();
        }

        public List<CaseStatus> GetClosedCaseStatusesByPersonIdAndDate(long personId, DateTime date)
        {
            var caseStatuses = _databaseContext.CaseStatuses
                .Where(cs => cs.PersonId == personId && cs.EndDate > date)
                .Include(cs => cs.Person)
                .Include(cs => cs.Answers).ToList();

            foreach (var caseStatus in caseStatuses)
            {
                if (caseStatus.Answers != null)
                {
                    var caseAnswers = new List<CaseStatusAnswer>();

                    foreach (var answer in caseStatus.Answers)
                    {
                        if (answer.DiscardedAt == null && answer.EndDate == null)
                        {
                            caseAnswers.Add(answer);
                        }
                    }
                    caseStatus.Answers = caseAnswers;
                }
            }

            return caseStatuses.Select(caseStatus => caseStatus.ToDomain()).ToList();
        }

        public CaseStatus CreateCaseStatus(CreateCaseStatusRequest request)
        {
            var caseStatus = new Infrastructure.CaseStatus
            {
                PersonId = request.PersonId,
                Type = request.Type,
                StartDate = request.StartDate,
                Notes = request.Notes,
                CreatedBy = request.CreatedBy,
                Answers = new List<CaseStatusAnswer>()
            };

            Guid identifier = Guid.NewGuid();

            foreach (var answer in request.Answers)
            {
                var caseStatusAnswer = new CaseStatusAnswer
                {
                    CaseStatusId = caseStatus.Id,
                    Option = answer.Option,
                    Value = answer.Value,
                    StartDate = request.StartDate,
                    CreatedAt = _systemTime.Now,
                    CreatedBy = request.CreatedBy,
                    GroupId = identifier.ToString()
                };
                caseStatus.Answers.Add(caseStatusAnswer);
            }
            _databaseContext.CaseStatuses.Add(caseStatus);

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }

        public CaseStatus UpdateCaseStatus(UpdateCaseStatusRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.Include(cs => cs.Answers).FirstOrDefault(x => x.Id == request.CaseStatusId);

            ValidateUpdate(request, caseStatus);
            var caseStatusType = caseStatus.Type.ToLower();

            //end date provided
            if (request.EndDate != null)
            {
                caseStatus.EndDate = request.EndDate;

                switch (caseStatusType)
                {
                    case "cp":
                    case "lac":
                        var activeStatusAnswers = caseStatus
                                                     .Answers
                                                     .Where(x => x.DiscardedAt == null && (x.EndDate == null || x.EndDate > DateTime.Today));

                        //discard future ones
                        foreach (var a in activeStatusAnswers.Where(x => x.StartDate > DateTime.Today))
                        {
                            a.DiscardedAt = _systemTime.Now;
                            a.LastModifiedBy = request.EditedBy;
                        }

                        //save start date and group id from the current answer and use them in the end reason
                        var activeAnswer = activeStatusAnswers.Where(x => x.StartDate <= DateTime.Today).First();

                        //end current ones
                        foreach (var a in activeStatusAnswers.Where(x => x.StartDate <= DateTime.Today))
                        {
                            a.EndDate = request.EndDate;
                            a.LastModifiedBy = request.EditedBy;
                        }

                        AddNewAnswers(request, caseStatus, startDate: activeAnswer.StartDate, endDate: request.EndDate, groupId: activeAnswer.GroupId);
                        break;
                }
            }
            //end date not provided
            else
            {
                switch (caseStatusType)
                {
                    case "cin":
                        caseStatus.Notes = request.Notes;
                        if (request.StartDate != null)
                        {
                            caseStatus.StartDate = (DateTime) request.StartDate;
                        }
                        break;

                    case "cp":
                    case "lac":
                        var previousAnswersToCopyAndDiscardCount = caseStatusType == "cp" ? 1 : 2;

                        var existingAnswerGroups = caseStatus
                                                .Answers
                                                .Where(x => x.DiscardedAt == null)
                                                .OrderBy(x => x.StartDate)
                                                .GroupBy(x => x.GroupId);

                        //multiple answer groups, check for overlapping dates
                        if (existingAnswerGroups?.Count() > 1)
                        {
                            foreach (var g in existingAnswerGroups)
                            {
                                foreach (var a in g)
                                {
                                    if (request.StartDate <= a.StartDate && a.EndDate != null && a.EndDate < DateTime.Today)
                                    {
                                        throw new InvalidStartDateException("Start date overlaps with previous status start date.");
                                    }
                                }
                            }
                        }

                        if (existingAnswerGroups?.Count() == 1 || (existingAnswerGroups?.Count() == 2 && caseStatus.Answers.Any(x => x.StartDate > DateTime.Today && x.EndDate == null)))
                        {
                            caseStatus.StartDate = (DateTime) request.StartDate;
                        }

                        var currentActiveAnswers = caseStatus.Answers
                                                .Where(x => x.DiscardedAt == null && (x.EndDate == null || x.EndDate > DateTime.Today) && x.StartDate <= DateTime.Today)
                                                .OrderBy(x => x.StartDate).ToList();

                        ReplaceCurrentActiveAnswers(request, caseStatus, currentActiveAnswers);

                        var previousCaseStatusAnswers = caseStatus.Answers
                                                .Where(x => x.DiscardedAt == null && x.EndDate != null && x.EndDate <= DateTime.Today)
                                                .OrderByDescending(x => x.StartDate).Take(previousAnswersToCopyAndDiscardCount).ToList();

                        CopyAndDiscardPreviousAnswers(request, caseStatus, previousCaseStatusAnswers);

                        break;
                }
            }

            caseStatus.LastModifiedBy = request.EditedBy;
            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }

        private static void ValidateUpdate(UpdateCaseStatusRequest request, Infrastructure.CaseStatus caseStatus)
        {
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {request.CaseStatusId} not found");
            }

            if (caseStatus.EndDate != null && request.EndDate < DateTime.Today)
            {
                throw new InvalidEndDateException($"Invalid end date.");
            }
        }
        private void ReplaceCurrentActiveAnswers(UpdateCaseStatusRequest request, Infrastructure.CaseStatus caseStatus, List<CaseStatusAnswer> caseStatusAnswers)
        {
            Guid identifier = Guid.NewGuid();
            DateTime? currentStatusEndDate = null;

            foreach (var a in caseStatusAnswers)
            {
                a.DiscardedAt = _systemTime.Now;
                a.LastModifiedBy = request.EditedBy;
                var currentEndDate = a.EndDate;
                if (currentEndDate.HasValue)
                {
                    currentStatusEndDate = currentEndDate;
                }
            }

            foreach (var ra in request.Answers)
            {
                caseStatus.Answers.Add(new CaseStatusAnswer()
                {
                    CaseStatusId = caseStatus.Id,
                    CreatedBy = request.EditedBy,
                    StartDate = (DateTime) request.StartDate,
                    Option = ra.Option,
                    Value = ra.Value,
                    GroupId = identifier.ToString(),
                    CreatedAt = _systemTime.Now,
                    EndDate = currentStatusEndDate
                });
            }
        }

        private void CopyAndDiscardPreviousAnswers(UpdateCaseStatusRequest request, Infrastructure.CaseStatus caseStatus, List<CaseStatusAnswer> previousCaseStatusAnswers)
        {
            Guid identifier = Guid.NewGuid();

            foreach (var a in previousCaseStatusAnswers)
            {
                a.DiscardedAt = _systemTime.Now;
                a.LastModifiedBy = request.EditedBy;

                caseStatus.Answers.Add(new CaseStatusAnswer()
                {
                    CaseStatusId = caseStatus.Id,
                    CreatedBy = request.EditedBy,
                    StartDate = a.StartDate,
                    EndDate = request.StartDate,
                    Option = a.Option,
                    Value = a.Value,
                    GroupId = identifier.ToString(),
                    CreatedAt = _systemTime.Now
                });
            }
        }

        private void AddNewAnswers(UpdateCaseStatusRequest request, Infrastructure.CaseStatus caseStatus, DateTime? startDate = null, DateTime? endDate = null, string? groupId = null)
        {
            Guid identifier = Guid.NewGuid();

            foreach (var a in request?.Answers)
            {
                caseStatus.Answers.Add(new Infrastructure.CaseStatusAnswer()
                {
                    CaseStatusId = caseStatus.Id,
                    CreatedBy = request.EditedBy,
                    StartDate = startDate ?? (DateTime) request.StartDate,
                    EndDate = endDate ?? null,
                    Option = a.Option,
                    Value = a.Value,
                    GroupId = groupId ?? identifier.ToString(),
                    CreatedAt = _systemTime.Now
                });
            }
        }

        public CaseStatus CreateCaseStatusAnswer(CreateCaseStatusAnswerRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault(x => x.Id == request.CaseStatusId);

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status id {request.CaseStatusId} does not exist.");
            }

            if (caseStatus.Answers == null) caseStatus.Answers = new List<CaseStatusAnswer>();

            Guid identifier = Guid.NewGuid();

            foreach (var answer in request.Answers)
            {
                var caseStatusAnswer = new Infrastructure.CaseStatusAnswer()
                {
                    CaseStatusId = caseStatus.Id,
                    CreatedBy = request.CreatedBy,
                    StartDate = request.StartDate,
                    Option = answer.Option,
                    Value = answer.Value,
                    GroupId = identifier.ToString(),
                    CreatedAt = _systemTime.Now
                };

                caseStatus.Answers.Add(caseStatusAnswer);
            }

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }

        public CaseStatus ReplaceCaseStatusAnswers(CreateCaseStatusAnswerRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault(x => x.Id == request.CaseStatusId);

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status id {request.CaseStatusId} does not exist.");
            }

            var activeAnswers = caseStatus
                                    .Answers
                                    .Where(x => x.DiscardedAt == null && x.EndDate == null);

            //discard future ones
            if (activeAnswers.Any(x => x.StartDate > DateTime.Today.Date))
            {
                foreach (var answer in activeAnswers)
                {
                    answer.DiscardedAt = _systemTime.Now;
                    answer.LastModifiedBy = request.CreatedBy;
                }
            }
            //end the current ones and add new episode end reason (hard coded for data migration purposes)
            else
            {
                caseStatus.Answers.Add(new CaseStatusAnswer()
                {
                    CreatedBy = request.CreatedBy,
                    EndDate = request.StartDate,
                    StartDate = activeAnswers.First().StartDate,
                    Option = LACAnswerOption.EpisodeReason,
                    Value = LACAnswerValue.X1,
                    GroupId = activeAnswers.First().GroupId
                });

                foreach (var answer in activeAnswers)
                {
                    answer.EndDate = request.StartDate;
                    answer.LastModifiedBy = request.CreatedBy;
                }
            }

            //add new ones
            Guid identifier = Guid.NewGuid();

            foreach (var answer in request.Answers)
            {
                var caseStatusAnswer = new Infrastructure.CaseStatusAnswer()
                {
                    CaseStatusId = caseStatus.Id,
                    CreatedBy = request.CreatedBy,
                    StartDate = request.StartDate,
                    Option = answer.Option,
                    Value = answer.Value,
                    GroupId = identifier.ToString(),
                    CreatedAt = _systemTime.Now
                };

                caseStatus.Answers.Add(caseStatusAnswer);
            }

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }
    }

    public static class LACAnswerOption
    {
        public const string EpisodeReason = "episodeReason";
    }
    public static class LACAnswerValue
    {
        public const string X1 = "X1";
    }
}
