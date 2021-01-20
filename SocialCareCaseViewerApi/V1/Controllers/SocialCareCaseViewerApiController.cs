using System;
using System.Globalization;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    //TODO: Rename to match the APIs endpoint
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    //TODO: rename class to match the API name
    public class SocialCareCaseViewerApiController : BaseController
    {
        private readonly IGetAllUseCase _getAllUseCase;
        private readonly IAddNewResidentUseCase _addNewResidentUseCase;
        private readonly IProcessDataUseCase _processDataUsecase;
        private readonly IAllocationsUseCase _allocationUseCase;
        private readonly IWorkersUseCase _workersUseCase;
        private readonly ITeamsUseCase _teamsUseCase;

        public SocialCareCaseViewerApiController(IGetAllUseCase getAllUseCase, IAddNewResidentUseCase addNewResidentUseCase,
            IProcessDataUseCase processDataUsecase, IAllocationsUseCase allocationUseCase, IWorkersUseCase workersUseCase, ITeamsUseCase teamsUseCase)
        {
            _getAllUseCase = getAllUseCase;
            _processDataUsecase = processDataUsecase;
            _addNewResidentUseCase = addNewResidentUseCase;
            _allocationUseCase = allocationUseCase;
            _workersUseCase = workersUseCase;
            _teamsUseCase = teamsUseCase;
        }

        /// <summary>
        /// Returns list of contacts who share the query search parameter
        /// </summary>
        /// <response code="200">Success. Returns a list of matching residents information</response>
        /// <response code="400">Invalid Query Parameter.</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents")]
        public IActionResult ListContacts([FromQuery] ResidentQueryParam rqp, int? cursor = 0, int? limit = 20)
        {
            try
            {
                return Ok(_getAllUseCase.Execute(rqp, (int) cursor, (int) limit));
            }
            //TODO: add better Mosaic API error handling
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
        /// Inserts a new record in the DM_PERSONS table
        /// </summary>
        /// <response code="201">Record successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem generating a token.</response>
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddNewResidentResponse), StatusCodes.Status201Created)]
        [HttpPost]
        [Route("residents")]
        public IActionResult AddNewResident([FromBody] AddNewResidentRequest residentRequest)
        {
            try
            {
                var response = _addNewResidentUseCase.Execute(residentRequest);

                return CreatedAtAction("GetResident", new { id = response.PersonId }, response);
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
        /// Find cases by Mosaic ID or officer email
        /// </summary>
        /// <response code="200">Success. Returns cases related to the specified ID or officer email</response>
        /// <response code="400">One or more dates are invalid or missing</response>
        /// <response code="404">No cases found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("cases")]
        public IActionResult ListCases([FromQuery] ListCasesRequest request)
        {
            try
            {
                string dateValidationError = null;

                if (!string.IsNullOrWhiteSpace(request.StartDate) && !DateTime.TryParseExact(request.StartDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    dateValidationError += "Invalid start date";
                }
                if (!string.IsNullOrWhiteSpace(request.EndDate) && !DateTime.TryParseExact(request.EndDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
                {
                    dateValidationError += " Invalid end date";
                }
                if (!string.IsNullOrEmpty(dateValidationError))
                {
                    return StatusCode(400, dateValidationError);
                }

                return Ok(_processDataUsecase.Execute(request));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }


        /// <summary>
        /// Find allocations by Mosaic ID or officer email
        /// </summary>
        /// <response code="200">Success. Returns allocations related to the specified ID or officer email</response>
        /// <response code="404">No allocations found for the specified mosaic id or worker id</response>
        [ProducesResponseType(typeof(AllocationList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("allocations")]
        public IActionResult GetAllocations([FromQuery] ListAllocationsRequest request)
        {
            return Ok(_allocationUseCase.Execute(request));
        }

        /// <summary>
        /// create new allocations for workers
        /// </summary>
        /// <response code="201">Allocation successfully inserted</response>
        [ProducesResponseType(typeof(CreateAllocationRequest), StatusCodes.Status201Created)]
        [HttpPost]
        [Route("allocations")]
        public IActionResult CreateAllocation([FromBody] CreateAllocationRequest request)
        {
            var result = _allocationUseCase.ExecutePost(request);
            return CreatedAtAction("CreateAllocation", result, result);
        }

        /// <summary>
        /// Deallocate worker. Other allocation updates are not supported at the moment
        /// <response code="200">Record successfully updated</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <reponse code="500">There was a problem updating the record</reponse>
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("allocations")]
        public IActionResult UpdateAllocation([FromBody] UpdateAllocationRequest request)
        {
            try
            {
                return Ok(_allocationUseCase.ExecuteUpdate(request));
            }
            catch (EntityUpdateException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Create new case note record for mosaic client
        /// </summary>
        /// <response code="201">Record successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem generating a token.</response>
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [HttpPost]
        [Route("cases")]
        public async Task<IActionResult> CreateCaseNote([FromBody] CaseNotesDocument request)
        {
            var id = await _processDataUsecase.Execute(request).ConfigureAwait(false);
            return StatusCode(201, new { _id = id });
        }

        /// <summary>
        /// Get a list of workers by team id
        /// </summary>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListWorkersResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("workers")]
        public IActionResult ListWorkers([FromQuery] ListWorkersRequest request)
        {
            return Ok(_workersUseCase.ExecuteGet(request));
        }

        /// <summary>
        /// Get a list of teams by context
        /// </summary>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListTeamsResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("teams")]
        public IActionResult ListTeams([FromQuery] ListTeamsRequest request)
        {
            return Ok(_teamsUseCase.ExecuteGet(request));
        }
    }
}
