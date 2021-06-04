using System;
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
    public class FormSubmissionController : BaseController
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
        [Route("{submissionId:Guid}")]
        public IActionResult GetSubmissionById(Guid submissionId)
        {
            var submission = _formSubmissionsUseCase.ExecuteGetById(submissionId);

            if (submission == null)
            {
                return NotFound();
            }

            return Ok(submission);
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
    }
}
