using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ResidentsUseCase : IResidentsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public ResidentsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public GetPersonResponse? ExecuteGet(long id)
        {
            var person = _databaseGateway.GetPersonDetailsById(id);

            return person?.ToResponse();
        }

        public AddNewResidentResponse ExecutePost(AddNewResidentRequest request)
        {
            return _databaseGateway.AddNewResident(request);
        }

        public void ExecutePatch(UpdatePersonRequest request)
        {
            _databaseGateway.UpdatePerson(request);
        }
    }
}
