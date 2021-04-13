using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IWarningNoteUseCase
    {
        CreateWarningNoteResponse ExecutePost(CreateWarningNoteRequest request);
        List<WarningNote> ExecuteGet(GetWarningNoteRequest request);
    }
}
