using System;
using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/residents")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ResidentController : Controller
    {
        private readonly IResidentUseCase _residentUseCase;
        private readonly ICreateRequestAuditUseCase _createRequestAuditUseCase;

        public ResidentController(IResidentUseCase residentUseCase, ICreateRequestAuditUseCase createRequestAuditUseCase)
        {
            _residentUseCase = residentUseCase;
            _createRequestAuditUseCase = createRequestAuditUseCase;
        }

        /// <summary>
        /// Returns list of contacts who share the query search parameter
        /// </summary>
        /// <response code="200">Success. Returns a list of matching residents information</response>
        [ProducesResponseType(typeof(ResidentInformationList), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult ListContacts([FromQuery] ResidentQueryParam rqp, int? cursor = 0, int? limit = 20)
        {
            return Ok(_residentUseCase.GetResidentsByQuery(rqp, (int) cursor, (int) limit));
        }

        /// <summary>
        /// Creates a new Allocation Resident/Team
        /// </summary>
        /// <response code="204">Records successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        [ProducesResponseType(typeof(CreateAllocationResponse), StatusCodes.Status201Created)]
        [HttpPost]
        [Route("{residentid}/allocate")]
        public IActionResult AllocateResidentToTheTeam([FromBody] AllocateResidentToTheTeamRequest allocateRequest, int residentid)
        {
            allocateRequest.PersonId = residentid;
            var validator = new AllocateResidentToTheTeamRequestValidator();
            var validationResults = validator.Validate(allocateRequest);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                return Ok(_residentUseCase.AllocateResidentToTheTeam(allocateRequest));
            }

            catch (Exception e) when (
                e is PersonNotFoundException ||
                e is TeamNotFoundException
            )
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Creates a new person record and adds all related entities
        /// </summary>
        /// <response code="201">Records successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddNewResidentResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public IActionResult AddNewResident([FromBody] AddNewResidentRequest residentRequest)
        {
            try
            {
                var response = _residentUseCase.AddNewResident(residentRequest);

                return CreatedAtAction(nameof(AddNewResident), new { id = response.Id }, response);
            }
            catch (Exception e) when (
                e is ResidentCouldNotBeinsertedException ||
                e is AddressCouldNotBeInsertedException
                )
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Get resident by id
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem getting the record</response>
        ///
        [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetPerson([FromQuery] GetPersonRequest request)
        {
            if (request.AuditingEnabled && !string.IsNullOrEmpty(request.UserId))
            {
                var auditRequest = new CreateRequestAuditRequest()
                {
                    ActionName = "view_resident",
                    UserName = request.UserId,
                    Metadata = new Dictionary<string, object>() { { "residentId", request.Id } }
                };

                _createRequestAuditUseCase.Execute(auditRequest);
            }

            var response = _residentUseCase.GetResident(request);

            if (response == null)
            {
                return NotFound();
            }

            return StatusCode(200, response);
        }

        /// <summary>
        /// Update resident details
        /// </summary>
        /// <response code="204">Success</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem updating the records</response>
        ///
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpPatch]
        public IActionResult UpdatePerson([FromBody] UpdatePersonRequest request)
        {
            try
            {
                _residentUseCase.UpdateResident(request);
                return StatusCode(204);
            }
            catch (UpdatePersonException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
