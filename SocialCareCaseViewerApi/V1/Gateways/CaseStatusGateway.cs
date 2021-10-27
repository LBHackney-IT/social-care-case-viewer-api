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

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {request.CaseStatusId} not found");
            }

            if (caseStatus.EndDate != null)
            {
                throw new CaseStatusAlreadyClosedException($"Case status with {request.CaseStatusId} has already been closed.");
            }

            if (request.EndDate != null)
            {
                caseStatus.EndDate = request.EndDate;
            }

            if (request.Answers != null)
            {
                Guid identifier = Guid.NewGuid();

                foreach (var answer in request.Answers)
                {
                    var caseStatusAnswer = new CaseStatusAnswer
                    {
                        CaseStatusId = caseStatus.Id,
                        Option = answer.Option,
                        Value = answer.Value,
                        CreatedAt = _systemTime.Now,
                        GroupId = identifier.ToString(),
                        CreatedBy = request.EditedBy
                    };
                    caseStatus.Answers.Add(caseStatusAnswer);
                }
            }
            _databaseContext.SaveChanges();
            return caseStatus.ToDomain();
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

        public CaseStatus ReplaceCaseStatusAnswer(CreateCaseStatusAnswerRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault(x => x.Id == request.CaseStatusId);

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status id {request.CaseStatusId} does not exist.");
            }

            var activeAnswerGroups = caseStatus
                .Answers
                .Where(x => x.DiscardedAt == null && x.EndDate == null)
                .OrderBy(x => x.StartDate)
                .GroupBy(x => x.GroupId);

            //end the current active answer and add new ones
            if (activeAnswerGroups.Count() == 1)
            {
                foreach (var answer in activeAnswerGroups.First())
                {
                    answer.EndDate = request.StartDate;
                }

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
            }
            else
            {
                //discard current scheduled change
                foreach (var answer in activeAnswerGroups.Last())
                {
                    answer.DiscardedAt = _systemTime.Now;
                }

                //end current active status
                foreach (var answer in activeAnswerGroups.First())
                {
                    answer.EndDate = request.StartDate;
                }

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
            }

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }
    }
}
