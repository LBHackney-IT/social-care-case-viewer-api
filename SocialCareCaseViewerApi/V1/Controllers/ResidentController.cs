using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/residents")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ResidentController : BaseController
    {
        private readonly IResidentsUseCase _residentsUseCase;
        private readonly IWarningNoteUseCase _warningNoteUseCase;
        private readonly IRelationshipsUseCase _relationshipsUseCase;

        public ResidentController(IResidentsUseCase residentsUseCase, IWarningNoteUseCase warningNoteUseCase, IRelationshipsUseCase relationshipsUseCase)
        {
            _residentsUseCase = residentsUseCase;
            _warningNoteUseCase = warningNoteUseCase;
            _relationshipsUseCase = relationshipsUseCase;
        }

        /// <summary>
        /// Get a resident by resident id
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem getting the record</response>
        ///
        [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{id:long}")]
        public IActionResult GetResident(long id)
        {
            var response = _residentsUseCase.ExecuteGet(id);

            if (response == null)
            {
                return NotFound();
            }

            return StatusCode(200, response);
        }

        /// <summary>
        /// Returns list of residents who share the query search parameter
        /// </summary>
        /// <response code="200">Success. Returns a list of matching residents information</response>
        /// <response code="400">Invalid Query Parameter.</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetResidents([FromQuery] ResidentQueryParam rqp, int cursor = 0, int limit = 20)
        {
            try
            {
                return Ok(_residentsUseCase.ExecuteGetAll(rqp, cursor, limit));
            }
            catch (MosaicApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.GetType().Name} : {ex.Message}");
            }
            catch (InvalidQueryParameterException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Creates a new resident and adds all related entities
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
                var response = _residentsUseCase.ExecutePost(residentRequest);

                return CreatedAtAction(nameof(AddNewResident), new { id = response.Id }, response);
            }
            catch (ResidentCouldNotBeinsertedException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (AddressCouldNotBeInsertedException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Update a resident's details
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem updating the records</response>
        ///
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpPatch]
        [Route("residents")]
        public IActionResult UpdateResident([FromBody] UpdatePersonRequest request)
        {
            try
            {
                _residentsUseCase.ExecutePatch(request);
            }
            catch (UpdatePersonException ex)
            {
                return NotFound(ex.Message);
            }

            return StatusCode(204);
        }

        /// <summary>
        /// Get all warning notes created for a specific resident
        /// </summary>
        /// <response code="200">Success. Returns warning notes related to the specified ID</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListWarningNotesResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{personId:long}/warningNotes")]
        public IActionResult ListWarningNotes(long personId)
        {
            return Ok(_warningNoteUseCase.ExecuteGet(personId));
        }

        /// <summary>
        /// Get a list of relationships by resident id
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Successful request. Relationships returned</response>
        /// <response code="404">Person not found</response>
        /// <response code="500">There was a problem getting the relationships</response>
        [ProducesResponseType(typeof(ListRelationshipsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{personId}/relationships")]
        public IActionResult ListRelationships([FromQuery] ListRelationshipsRequest request)
        {
            try
            {
                return Ok(_relationshipsUseCase.ExecuteGet(request));
            }
            catch (GetRelationshipsException ex)
            {
                return NotFound(ex.Message);
            }
        }

    }
}
