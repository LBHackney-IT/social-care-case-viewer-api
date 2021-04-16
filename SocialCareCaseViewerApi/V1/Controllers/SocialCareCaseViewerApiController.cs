using System;
using System.Collections.Generic;
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
        private readonly IProcessDataUseCase _processDataUseCase;
        private readonly IAllocationsUseCase _allocationUseCase;
        private readonly IGetWorkersUseCase _getWorkersUseCase;
        private readonly ITeamsUseCase _teamsUseCase;
        private readonly ICaseNotesUseCase _caseNotesUseCase;
        private readonly IVisitsUseCase _visitsUseCase;
        private readonly IWarningNoteUseCase _warningNoteUseCase;
        private readonly IPersonUseCase _personUseCase;

        public SocialCareCaseViewerApiController(IGetAllUseCase getAllUseCase, IAddNewResidentUseCase addNewResidentUseCase,
            IProcessDataUseCase processDataUseCase, IAllocationsUseCase allocationUseCase, IGetWorkersUseCase getWorkersUseCase,
            ITeamsUseCase teamsUseCase, ICaseNotesUseCase caseNotesUseCase, IVisitsUseCase visitsUseCase,
            IWarningNoteUseCase warningNotesUseCase, IPersonUseCase personUseCase)
        {
            _getAllUseCase = getAllUseCase;
            _processDataUseCase = processDataUseCase;
            _addNewResidentUseCase = addNewResidentUseCase;
            _allocationUseCase = allocationUseCase;
            _getWorkersUseCase = getWorkersUseCase;
            _teamsUseCase = teamsUseCase;
            _caseNotesUseCase = caseNotesUseCase;
            _visitsUseCase = visitsUseCase;
            _warningNoteUseCase = warningNotesUseCase;
            _personUseCase = personUseCase;
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
        /// Creates a new person record and adds all related entities
        /// </summary>
        /// <response code="201">Records successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(AddNewResidentResponse), StatusCodes.Status201Created)]
        [HttpPost]
        [Route("residents")]
        public IActionResult AddNewResident([FromBody] AddNewResidentRequest residentRequest)
        {
            try
            {
                var response = _addNewResidentUseCase.Execute(residentRequest);

                return CreatedAtAction("GetResident", new { id = response.PersonId }, response); //TODO: return object with IDs for all related entities
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
        /// Get resident by id
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem getting the record</response>
        ///
        [ProducesResponseType(typeof(GetPersonResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("person/{id}")]
        public IActionResult GetPerson([FromQuery] GetPersonRequest request)
        {
            var response = _personUseCase.ExecuteGet(request);

            if(response == null)
            {
                return NotFound();
            }

            return StatusCode(200, response);
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

                return Ok(_processDataUseCase.Execute(request));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Find specific case by unique Record ID produced by MongoDB
        /// </summary>
        /// <response code="200">Success. Returns case related to the specified ID</response>
        /// <response code="404">No cases found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CareCaseData), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("cases/{id}")]
        public IActionResult GetCaseByRecordId([FromQuery] GetCaseByIdRequest request)
        {
            try
            {
                return Ok(_processDataUseCase.Execute(request.Id));
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
        public async Task<IActionResult> CreateCaseNote([FromBody] CreateCaseNoteRequest request)
        {
            var id = await _processDataUseCase.Execute(request).ConfigureAwait(false);
            return StatusCode(201, new { _id = id });
        }

        /// <summary>
        /// Get a list of workers by worker id, worker email or team id
        /// </summary>
        /// <response code="404">No workers found for inserted worker id, worker email or team id</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(List<WorkerResponse>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("workers")]
        public IActionResult GetWorkers([FromQuery] GetWorkersRequest request)
        {
            var workers = _getWorkersUseCase.Execute(request);

            if (workers.Count == 0)
            {
                return NotFound();
            }
            return Ok(workers);
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

        /// <summary>
        /// Get case notes by person id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="400">Id parameter is invalid or missing</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListCaseNotesResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("casenotes/person/{id}")]
        public IActionResult ListCaseNotes([FromQuery] ListCaseNotesRequest request)
        {
            return Ok(_caseNotesUseCase.ExecuteGetByPersonId(request.Id));
        }

        /// <summary>
        /// Get case note by id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="400">Id parameter is invalid or missing</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(CaseNoteResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("casenotes/{id}")]
        public IActionResult GetCaseNoteById([FromQuery] GetCaseNotesRequest request)
        {
            try
            {
                return Ok(_caseNotesUseCase.ExecuteGetById(request.Id));
            }
            catch (SocialCarePlatformApiException ex)
            {
                return StatusCode(ex.Message == "404" ? 404 : 500);
            }
        }

        /// <summary>
        /// Get visits by person id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="400">Id parameter is invalid or missing</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListVisitsResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("visits/person/{id}")]
        public IActionResult ListVisits([FromQuery] ListVisitsRequest request)
        {
            var showHistoricData = Environment.GetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA");

            return showHistoricData != null && showHistoricData.Equals("true")
                ? Ok(_visitsUseCase.ExecuteGetByPersonId(request.Id))
                : StatusCode(200, null);
        }

        /// <summary>
        /// Create warning note.
        /// <response code="201">Record successfully created</response>
        /// <reponse code="500">There was a problem creating the record</reponse>
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("warningnotes")]
        public IActionResult CreateWarningNote([FromQuery] CreateWarningNoteRequest request)
        {
            try
            {
                var result = _warningNoteUseCase.ExecutePost(request);
                return CreatedAtAction("CreateAllocation", result, result);
            }
            catch (CreateWarningNoteException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get warning notes by person id
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Success. Returns warning notes related to the specified ID</response>
        /// <response code="404">No warning notes found for the specified ID</response>
        [ProducesResponseType(typeof(List<WarningNote>), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("warningnotes/{id}")]
        public IActionResult GetWarningNote([FromQuery] GetWarningNoteRequest request)
        {
            try
            {
                return Ok(_warningNoteUseCase.ExecuteGet(request));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
