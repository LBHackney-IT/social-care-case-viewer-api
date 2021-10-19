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

        public List<CaseStatus> GetCaseStatusesByPersonId(long personId)
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
        public CaseStatus? GetCaseStatusesByPersonIdDate(long personId, DateTime date)
        {
            var caseStatus = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.StartDate <= date)
                .Where(cs => cs.EndDate == null || cs.EndDate >= date)
                .Include(cs => cs.Person)
                .FirstOrDefault();

            if (caseStatus != null && caseStatus.Answers != null)
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

            return caseStatus?.ToDomain();
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

            if (request.StartDate != null && caseStatus.Type != "LAC"){
                caseStatus.StartDate = (DateTime) request.StartDate;
            }

            if (request.Notes != null)
            {
                caseStatus.Notes = request.Notes;
            }

            if (request.Answers != null)
            {
                if (caseStatus.Type == "LAC")
                {
                    var activeAnswers = GetActiveCaseStatusAnswers(caseStatus.Answers);
                    foreach (var caseStatusAnswer in activeAnswers)
                    {
                        caseStatusAnswer.DiscardedAt = DateTime.Now;
                        caseStatus.Answers.Remove(caseStatusAnswer);
                    }
                }

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
                        CreatedBy = request.EditedBy,
                        StartDate = (DateTime) request.StartDate
                    };
                    caseStatus.Answers.Add(caseStatusAnswer);
                }
            }

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }

        public List<CaseStatusAnswer> GetActiveCaseStatusAnswers(List<CaseStatusAnswer> answers)
        {
            var active_elm = answers
                    .Where(csa => csa.StartDate < DateTime.Now)
                    .OrderByDescending(csa => csa.StartDate)
                    .FirstOrDefault();

            var activeAnswersGroup = answers
                    .Where(csa => csa.GroupId == active_elm.GroupId)
                    .ToList();

            return activeAnswersGroup;
        }

        public CaseStatus CreateCaseStatusAnswer(CreateCaseStatusAnswerRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault(x => x.Id == request.CaseStatusId);

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status id {request.CaseStatusId} does not exist.");
            }

            if (caseStatus.Answers == null) caseStatus.Answers = new List<CaseStatusAnswer>();

            foreach (var answer in request.Answers)
            {
                Guid identifier = Guid.NewGuid();

                var caseStatusAnswer = new Infrastructure.CaseStatusAnswer()
                {
                    CaseStatusId = caseStatus.Id,
                    CreatedBy = request.CreatedBy,
                    StartDate = request.StartDate,
                    Option = answer.Option,
                    Value = answer.Value,
                    GroupId = identifier.ToString(),
                    CreatedAt = _systemTime.Now //will get overwritten by audit feature 
                };

                caseStatus.Answers.Add(caseStatusAnswer);
            }
            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }
    }
}
