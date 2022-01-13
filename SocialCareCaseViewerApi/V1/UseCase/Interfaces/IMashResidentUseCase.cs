using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IMashResidentUseCase
    {
        public MashResidentResponse UpdateMashResident(UpdateMashResidentRequest request, long mashResidentId);
    }
}
