using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetAllUseCase : IGetAllUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public GetAllUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ResidentInformationList Execute(ResidentQueryParam rqp, int cursor, int limit)
        {
            limit = limit < 10 ? 10 : limit;
            limit = limit > 100 ? 100 : limit;
            var residents = _databaseGateway.GetAllResidents(
                cursor: cursor, limit: limit, rqp.FirstName, rqp.LastName,
                rqp.DateOfBirth, rqp.PersonId, rqp.AgeGroup);

            var nextCursor = residents.Count == limit ? residents.Max(r => r.PersonId) : "";
            return new ResidentInformationList
            {
                Residents = residents.ToResponse(),
                NextCursor = nextCursor
            };
        }
    }
}
