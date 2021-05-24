using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseRecordsUseCase
    {
        CareCaseDataList Execute(ListCasesRequest? request);

        CareCaseData Execute(string recordId);
        Task<string> Execute(CreateCaseNoteRequest request);
    }
}
