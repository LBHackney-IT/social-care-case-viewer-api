using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IWarningNoteUseCase
    {
        PostWarningNoteResponse ExecutePost(PostWarningNoteRequest request);
        List<WarningNote> ExecuteGet(long personId);
    }
}
