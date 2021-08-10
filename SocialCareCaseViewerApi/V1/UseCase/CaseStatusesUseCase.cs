using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public class CaseStatusesUseCase : ICaseStatusesUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public CaseStatusesUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ListCaseStatusesResponse ExecuteGet(long personId)
        {
            var caseStatus = _databaseGateway.GetCaseStatusesByPersonId(personId);

            if (caseStatus == null || caseStatus.Count == 0)
            {
                throw new GetCaseStatusesException("Case status for person not found");
            }

            var response = new ListCaseStatusesResponse() { PersonId = personId, CaseStatuses = caseStatus.ToResponse()};

            return response;
        }
    }
}
