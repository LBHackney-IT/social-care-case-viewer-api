using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class CaseStatusGateway : ICaseStatusGateway
    {
        private readonly DatabaseContext _databaseContext;

        public CaseStatusGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
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
            var caseStatuses = _databaseContext.CaseStatuses
                .Where(cs => cs.PersonId == personId)
                .Where(cs => cs.EndDate == null || cs.EndDate > DateTime.Today);

            return caseStatuses.Select(caseStatus => caseStatus.ToDomain()).ToList();
        }
        public CaseStatus? GetCaseStatusesByPersonIdDate(long personId, DateTime date)
        {
            var caseStatus = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.StartDate <= date)
                .Where(cs => cs.EndDate == null || cs.EndDate >= date)
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
                CreatedBy = request.CreatedBy
            };

            _databaseContext.CaseStatuses.Add(caseStatus);

            foreach (var optionValue in request.Fields)
            {

            }

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
