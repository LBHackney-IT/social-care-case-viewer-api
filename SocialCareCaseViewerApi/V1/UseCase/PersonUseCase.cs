using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class PersonUseCase : IPersonUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public PersonUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public GetPersonResponse ExecuteGet(GetPersonRequest request)
        {
            Person person = _databaseGateway.GetPersonDetailsById(request.Id);

            return person != null ? ResponseFactory.ToResponse(person) : null;
        }

        public void ExecutePatch(UpdatePersonRequest request)
        {
            _databaseGateway.UpdatePerson(request);
        }
    }
}
