using System.Collections.Generic;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;


namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IFormSubmissionsUseCase
    {
        (CaseSubmissionResponse, CaseSubmission) ExecutePost(CreateCaseSubmissionRequest request);

        CaseSubmissionResponse ExecuteGetById(string submissionId);

        IEnumerable<CaseSubmissionResponse> ExecuteGetByQuery(QueryCaseSubmissionsRequest request);

        CaseSubmissionResponse ExecuteUpdateSubmission(string submissionId, UpdateCaseSubmissionRequest request);
        CaseSubmissionResponse UpdateAnswers(string submissionId, string stepId,
            UpdateFormSubmissionAnswersRequest request);
    }
}
