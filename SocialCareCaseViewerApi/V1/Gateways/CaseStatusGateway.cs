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
                .FirstOrDefault()
                ?.ToDomain();
        }

        public List<CaseStatus> GetCaseStatusesByPersonId(long personId)
        {
            var caseStatuses = _databaseContext
                .CaseStatuses
                .Where(cs => cs.PersonId == personId)
                .Where(cs => cs.EndDate == null || cs.EndDate > DateTime.Today)
                .Include(cs => cs.Person)
                .Include(cs => cs.Answers);           

            return caseStatuses.Select(caseStatus => caseStatus.ToDomain()).ToList();
        }
        public CaseStatus? GetCaseStatusesByPersonIdDate(long personId, DateTime date)
        {
            var caseStatus = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.StartDate <= date)
                .Where(cs => cs.EndDate == null || cs.EndDate >= date)
                .Include(cs => cs.Person)
                .FirstOrDefault();

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

            _databaseContext.CaseStatuses.Add(caseStatus); 

            foreach (var answer in request.Fields)
            {
                var caseStatusAnswer = new CaseStatusAnswer
                {
                    CaseStatusId = caseStatus.Id,
                    Question = answer.Name,
                    Answer = answer.Selected,
                    StartDate = request.StartDate,
                    CreatedAt = _systemTime.Now
                };
                caseStatus.Answers.Add(caseStatusAnswer);
            }
            _databaseContext.CaseStatuses.Add(caseStatus);

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }

        public CaseStatus UpdateCaseStatus(long caseStatusId, UpdateCaseStatusRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatusId);
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {caseStatusId} not found");
            }

            if (request.EndDate != null)
            {
                caseStatus.EndDate = request.EndDate;
            }

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }
    }
}
