using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System;
using System.Globalization;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseStatusesUseCase : ICaseStatusesUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public CaseStatusesUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ListCaseStatusesResponse ExecuteGet(long personId, string startDateString, string endDateString)
        {
            var person = _databaseGateway.GetPersonByMosaicId(personId);

            if (person == null)
            {
                throw new GetCaseStatusesException("Person not found");
            }

            var endDate = DateTime.Now;
            if (endDateString != null)
            {
                var cultureInfo = new CultureInfo("en-GB");
                endDate = DateTime.ParseExact(endDateString, "dd-MM-yyyy", cultureInfo);
            }
            var startDate = DateTime.Now;
            if (startDateString != null)
            {
                var cultureInfo = new CultureInfo("en-GB");
                startDate = DateTime.ParseExact(startDateString, "dd-MM-yyyy", cultureInfo);
            }

            var caseStatus = _databaseGateway.GetCaseStatusesByPersonId(personId, startDate, endDate);

            var response = new ListCaseStatusesResponse() { PersonId = personId, CaseStatuses = caseStatus.ToResponse() };

            return response;
        }
    }
}
