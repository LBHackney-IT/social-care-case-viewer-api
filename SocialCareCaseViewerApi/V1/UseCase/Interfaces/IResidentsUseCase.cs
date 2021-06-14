using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IResidentsUseCase
    {
        GetResidentResponse ExecuteGet(long id);

        ResidentInformationList ExecuteGetAll(ResidentQueryParam rqp, int cursor, int limit);

        public AddNewResidentResponse ExecutePost(AddNewResidentRequest request);

        void ExecutePatch(UpdateResidentRequest request);
    }
}
