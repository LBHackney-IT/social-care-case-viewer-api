using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;


namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IFormSubmissionsUseCase
    {
        (CaseSubmissionResponse, CaseSubmission) ExecutePost(CreateCaseSubmissionRequest request);

        CaseSubmissionResponse ExecuteGetById(string submissionId);

        Paginated<CaseSubmissionResponse> ExecuteGetByQuery(QueryCaseSubmissionsRequest request);

        CaseSubmissionResponse ExecuteUpdateSubmission(string submissionId, UpdateCaseSubmissionRequest request);
        CaseSubmissionResponse UpdateAnswers(string submissionId, string stepId,
            UpdateFormSubmissionAnswersRequest request);
        void ExecuteDelete(string submissionsId, DeleteCaseSubmissionRequest request);
    }
}
