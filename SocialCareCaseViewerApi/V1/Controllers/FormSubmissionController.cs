using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/submissions")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class FormSubmissionController : Controller
    {
        private readonly IFormSubmissionsUseCase _formSubmissionsUseCase;

        public FormSubmissionController(IFormSubmissionsUseCase formSubmissionsUseCase)
        {
            _formSubmissionsUseCase = formSubmissionsUseCase;
        }

        /// <summary>
        /// Get a submission
        /// </summary>
        /// <response code="200">Case submission successfully found</response>
        /// <response code="404">Case submission not found</response>
        [ProducesResponseType(typeof(CaseSubmissionResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{submissionId}")]
        public IActionResult GetSubmissionById(string submissionId)
        {
            var submission = _formSubmissionsUseCase.ExecuteGetById(submissionId);

            if (submission == null)
            {
                return NotFound();
            }

            return Ok(submission);
        }

        /// <summary>
        /// Get case submissions via query parameters
        /// </summary>
        /// <response code="200">Case submission successfully found</response>
        /// <response code="400">Invalid query made</response>
        [ProducesResponseType(typeof(Paginated<CaseSubmissionResponse>), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetSubmissionByQueryParameters([FromQuery] QueryCaseSubmissionsRequest request)
        {
            var validator = new QueryCaseSubmissionsValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var forms = _formSubmissionsUseCase.ExecuteGetByQuery(request);

                return Ok(forms);
            }
            catch (QueryCaseSubmissionsException e)
            {
                return BadRequest(e.Message);
            }
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
                var createdSubmission = _formSubmissionsUseCase.ExecutePost(request).Item1;

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

        /// <summary>
        /// Update a submission
        /// </summary>
        /// <response code="200">Case submission successfully updated</response>
        /// <response code="400">Invalid UpdateCaseSubmissionRequest received</response>
        /// <response code="422">Could not process request</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("{submissionId}")]
        public IActionResult UpdateSubmission(string submissionId, [FromBody] UpdateCaseSubmissionRequest request)
        {
            var validator = new UpdateCaseSubmissionRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                return Ok(_formSubmissionsUseCase.ExecuteUpdateSubmission(submissionId, request));
            }
            catch (WorkerNotFoundException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (PersonNotFoundException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (GetSubmissionException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (UpdateSubmissionException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }

        /// <summary>
        /// Delete a submission
        /// </summary>
        /// <response code="204">Case submission successfully deleted</response>
        /// <response code="400">Invalid DeleteCaseSubmissionRequest received</response>
        /// <response code="422">Case submission not found or worker not found</response>
        /// <response code="410">Case submission already deleted</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete]
        [Route("{submissionId}")]
        public IActionResult DeleteSubmission(string submissionId, [FromBody] DeleteCaseSubmissionRequest request)
        {
            var validator = new DeleteCaseSubmissionRequestValidator();
            var validatioResults = validator.Validate(request);

            if (!validatioResults.IsValid)
            {
                return BadRequest(validatioResults.ToString());
            }

            try
            {
                _formSubmissionsUseCase.ExecuteDelete(submissionId, request);
            }
            catch (WorkerNotFoundException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (DeleteSubmissionException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (SubmissionAlreadyDeletedException e)
            {
                return StatusCode(StatusCodes.Status410Gone, e.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Edit answers for a submission
        /// </summary>
        /// <param name="submissionId">Get the related submission by unique ID</param>
        /// <param name="stepId">Get the correct step on the form answers to update</param>
        /// <param name="request">Contains Edited By - email for worker doing the edit, StepAnswers - new answers to be saved for Form Submission</param>
        /// <response code="201">Case submission created successfully</response>
        /// <response code="400">Invalid CreateCaseSubmissionRequest received</response>
        /// <response code="422">Could not process request</response>
        [ProducesResponseType(typeof(CaseSubmissionResponse), StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("{submissionId}/steps/{stepId}")]
        public IActionResult EditSubmissionAnswers(string submissionId, string stepId, [FromBody] UpdateFormSubmissionAnswersRequest request)
        {
            var validator = new UpdateFormSubmissionAnswersValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                return Ok(_formSubmissionsUseCase.UpdateAnswers(submissionId, stepId, request));
            }
            catch (GetSubmissionException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (WorkerNotFoundException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }
    }
}
