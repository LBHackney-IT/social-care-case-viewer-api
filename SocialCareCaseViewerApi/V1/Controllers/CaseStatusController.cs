using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]

    public class CaseStatusController : Controller
    {
        private readonly ICaseStatusesUseCase _caseStatusesUseCase;

        public CaseStatusController(ICaseStatusesUseCase caseStatusUseCase)
        {
            _caseStatusesUseCase = caseStatusUseCase;
        }

        /// <summary>
        /// Get a list of fields and their options for a case status type
        /// </summary>
        /// <response code="200">Found case status type, returns the fields for that type</response>
        /// <response code="404">Case status type not found, or no fields exist</response>
        [ProducesResponseType(typeof(GetCaseStatusFieldsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("case-status/form-options/{type?}")]
        public IActionResult GetCaseStatusTypeFields([FromRoute] GetCaseStatusFieldsRequest request)
        {
            try
            {
                return Ok(_caseStatusesUseCase.ExecuteGetFields(request));
            }
            catch (CaseStatusNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
        }


        /// <summary>
        /// Get a list of case statuses by person id
        /// </summary>
        /// <response code="200">Successful request. Case statuses returned</response>
        /// <response code="404">Case status not found</response>
        [ProducesResponseType(typeof(ListRelationshipsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId:long}/casestatuses")]
        public IActionResult ListCaseStatuses(long personId)
        {
            try
            {
                return Ok(_caseStatusesUseCase.ExecuteGet(personId));
            }
            catch (GetCaseStatusesException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Create a case status
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Successfully created case status</response>
        /// <response code="400">Invalid CreatePersonCaseStatusRequest received</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        [Route("resident/case-statuses")]
        public IActionResult CreateCaseStatus([FromBody] CreateCaseStatusRequest request)
        {
            var validator = new CreateCaseStatusRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var caseStatus = _caseStatusesUseCase.ExecutePost(request);

                return CreatedAtAction(nameof(CreateCaseStatus), "Successfully created case status.");
            }
            catch (Exception e) when (
                e is PersonNotFoundException ||
                e is CaseStatusTypeNotFoundException ||
                e is CaseStatusAlreadyExistsException ||
                e is WorkerNotFoundException
            )
            {
                return BadRequest(e.Message);
            }
        }
    }
}
