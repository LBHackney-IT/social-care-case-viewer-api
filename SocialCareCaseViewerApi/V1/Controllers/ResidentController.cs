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
        private readonly IGetAllUseCase _getAllUseCase;
        private readonly IResidentsUseCase _residentsUseCase;

        public ResidentController(IGetAllUseCase getAllUseCase, IResidentsUseCase residentsUseCase)
        {
            _getAllUseCase = getAllUseCase;
            _residentsUseCase = residentsUseCase;
        }

        /// <summary>
        /// Returns list of residents who share the query search parameter
        /// </summary>
        /// <response code="200">Success. Returns a list of matching residents information</response>
        /// <response code="400">Invalid Query Parameter.</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetResidents([FromQuery] ResidentQueryParam rqp, int? cursor = 0, int? limit = 20)
        {
            try
            {
                return Ok(_getAllUseCase.Execute(rqp, (int) cursor, (int) limit));
            }
            catch (MosaicApiException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.GetType().Name.ToString()} : {ex.Message}");
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

    }
}
