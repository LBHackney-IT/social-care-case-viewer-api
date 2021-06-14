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
    //TODO: Rename to match the APIs endpoint
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    //TODO: rename class to match the API name
    public class SocialCareCaseViewerApiController : BaseController
    {
        private readonly IAllocationsUseCase _allocationUseCase;
        private readonly ICaseNotesUseCase _caseNotesUseCase;
        private readonly IVisitsUseCase _visitsUseCase;
        private readonly IWarningNoteUseCase _warningNoteUseCase;
        private readonly IGetVisitByVisitIdUseCase _getVisitByVisitIdUseCase;
        private readonly IRelationshipsUseCase _relationshipsUseCase;

        public SocialCareCaseViewerApiController(
            IAllocationsUseCase allocationUseCase, ICaseNotesUseCase caseNotesUseCase,
            IVisitsUseCase visitsUseCase, IWarningNoteUseCase warningNotesUseCase,
            IGetVisitByVisitIdUseCase getVisitByVisitIdUseCase, IRelationshipsUseCase relationshipsUseCase)
        {
            _allocationUseCase = allocationUseCase;
            _caseNotesUseCase = caseNotesUseCase;
            _visitsUseCase = visitsUseCase;
            _warningNoteUseCase = warningNotesUseCase;
            _getVisitByVisitIdUseCase = getVisitByVisitIdUseCase;
            _relationshipsUseCase = relationshipsUseCase;
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
        [ProducesResponseType(typeof(CreateAllocationResponse), StatusCodes.Status201Created)]
        [HttpPost]
        [Route("allocations")]
        public IActionResult CreateAllocation([FromBody] CreateAllocationRequest request)
        {
            var validator = new CreateAllocationRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var result = _allocationUseCase.ExecutePost(request);
                return CreatedAtAction("CreateAllocation", result, result);
            }
            catch (CreateAllocationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UpdateAllocationException ex)
            {
                return StatusCode(500, ex.Message);
            }
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
            var validator = new UpdateAllocationRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                return Ok(_allocationUseCase.ExecuteUpdate(request));
            }
            catch (EntityUpdateException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (UpdateAllocationException ex)
            {
                return StatusCode(500, ex.Message);
            }
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
        /// Get visit by visit id
        /// </summary>
        /// <response code="200">Success. Returns a matching visit</response>
        /// <response code="404">No visit found for visit id</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(Visit), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("visits/{visitId:long}")]
        public IActionResult GetVisitByVisitId(long visitId)
        {
            var response = _getVisitByVisitIdUseCase.Execute(visitId);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
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
            return Ok(_visitsUseCase.ExecuteGetByPersonId(request.Id));
        }

        /// <summary>
        /// Create warning note.
        /// <response code="201">Record successfully created</response>
        /// <reponse code="500">There was a problem creating the record</reponse>
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost]
        [Route("warningnotes")]
        public IActionResult PostWarningNote([FromBody] PostWarningNoteRequest request)
        {
            try
            {
                var result = _warningNoteUseCase.ExecutePost(request);
                return CreatedAtAction("CreateAllocation", result, result);
            }
            catch (PostWarningNoteException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get all warning notes created for a specific person
        /// </summary>
        /// <response code="200">Success. Returns warning notes related to the specified ID</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListWarningNotesResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId}/warningNotes")]
        public IActionResult ListWarningNotes(long personId)
        {
            return Ok(_warningNoteUseCase.ExecuteGet(personId));
        }

        /// <summary>
        /// Get a specific warning note along with any associated warning note reviews
        /// </summary>
        /// <response code="200">Success. Returns warning note and any related reviews for the specified ID</response>
        /// <response code="404">No warning note found for the specified ID</response>
        [ProducesResponseType(typeof(WarningNoteResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("warningnotes/{warningNoteId}")]
        public IActionResult GetWarningNoteById(long warningNoteId)
        {
            try
            {
                return Ok(_warningNoteUseCase.ExecuteGetWarningNoteById(warningNoteId));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Amend Warning Notes in response to a review and add a Warning Note Review to the database
        /// </summary>
        /// <param name="request"></param>
        /// <response code="204">Amended successfully</response>
        /// <response code="404">Exception encountered</response>
        /// <response code="500">There was a problem updating the record</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpPatch]
        [Route("warningnotes")]
        public IActionResult PatchWarningNote([FromBody] PatchWarningNoteRequest request)
        {
            var validator = new PatchWarningNoteRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                _warningNoteUseCase.ExecutePatch(request);
                return NoContent();
            }
            catch (PatchWarningNoteException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Get a list of relationships by person id
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Successful request. Relationships returned</response>
        /// <response code="404">Person not found</response>
        /// <response code="500">There was a problem getting the relationships</response>
        [ProducesResponseType(typeof(ListRelationshipsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId}/relationships")]
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
