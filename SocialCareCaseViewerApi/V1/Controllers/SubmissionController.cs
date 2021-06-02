using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/submissions")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class SubmissionController : BaseController
    {
        private readonly ISubmissionsUseCase _submissionsUseCase;

        public SubmissionController(ISubmissionsUseCase submissionsUseCase)
        {
            _submissionsUseCase = submissionsUseCase;
        }

        /// <summary>
        /// Create a submission
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Case submission created successfully</response>
        /// <response code="400">Invalid CreateCaseSubmissionRequest received</response>
        /// <response code="422">Could not process request</response>
        [ProducesResponseType(typeof(CaseSubmissionResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public IActionResult CreateSubmission([FromBody] CreateCaseSubmissionRequest request)
        {
            var validator = new CreateCaseSubmissionRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var createdSubmission = _submissionsUseCase.ExecutePost(request);
                return CreatedAtAction(nameof(CreateSubmission), createdSubmission);
            }
            catch (WorkerNotFoundException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (PersonNotFoundException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }
    }
}
