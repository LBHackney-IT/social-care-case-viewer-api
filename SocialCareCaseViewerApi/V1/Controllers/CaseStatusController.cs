using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
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
        /// Get a list of case statuses by person id
        /// </summary>
        /// <response code="200">Successful request. Case statuses returned</response>
        /// <response code="404">Case status not found</response>
        [ProducesResponseType(typeof(CaseStatusResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId:long}/case-statuses")]
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
        [Route("residents/case-statuses")]
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
                _caseStatusesUseCase.ExecutePost(request);

                return CreatedAtAction(nameof(CreateCaseStatus), "Successfully created case status.");
            }
            catch (Exception e) when (
                e is PersonNotFoundException ||
                e is CaseStatusAlreadyExistsException ||
                e is WorkerNotFoundException
            )
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Edit a Case Status
        /// </summary>
        /// <param name="request"></param>
        /// <param name="caseStatusId"></param>
        /// <response code="201">Successfully updated a case status</response>
        /// <response code="400">Invalid request received</response>
        [ProducesResponseType(typeof(CaseStatus), 200)]
        [HttpPatch]
        [Route("residents/case-statuses/{caseStatusId:long}")]
        public IActionResult UpdateCaseStatus([FromRoute] long caseStatusId, [FromBody] UpdateCaseStatusRequest request)
        {
            var validator = new UpdateCaseStatusValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var caseStatus = _caseStatusesUseCase.ExecuteUpdate(caseStatusId, request);
                return Ok(caseStatus);
            }
            catch (Exception e) when (
                e is PersonNotFoundException ||
                e is WorkerNotFoundException ||
                e is CaseStatusDoesNotMatchPersonException
            )
            {
                return BadRequest(e.Message);
            }
            catch (CaseStatusDoesNotExistException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
