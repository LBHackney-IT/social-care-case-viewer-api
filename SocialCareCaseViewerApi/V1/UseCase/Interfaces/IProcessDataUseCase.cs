using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public interface IProcessDataUseCase
    {
        CareCaseDataList Execute(ListCasesRequest request);
        Task<string> Execute(CreateCaseNoteRequest request);
    }
}
