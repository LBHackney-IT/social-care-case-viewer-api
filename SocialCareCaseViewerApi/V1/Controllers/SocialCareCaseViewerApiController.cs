using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
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
        private readonly IGetChildrenAllocationUseCase _childrenAllocationUseCase;
        private readonly IGetAdultsAllocationsUseCase _adultsAllocationUseCase;

        public SocialCareCaseViewerApiController(IGetAllUseCase getAllUseCase, IAddNewResidentUseCase addNewResidentUseCase,
            IProcessDataUseCase processDataUsecase, IGetChildrenAllocationUseCase childrenAllocationUseCase, IGetAdultsAllocationsUseCase adultsAllocationsUseCase)
        {
            _getAllUseCase = getAllUseCase;
            _processDataUsecase = processDataUsecase;
            _addNewResidentUseCase = addNewResidentUseCase;
            _childrenAllocationUseCase = childrenAllocationUseCase;
            _adultsAllocationUseCase = adultsAllocationsUseCase;
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
        /// <response code="404">No cases found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("cases")]
        public IActionResult ListCases([FromQuery] ListCasesRequest request)
        {
            try
            {
                return Ok(_processDataUsecase.Execute(request));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }


        /// <summary>
        /// Find cfs allocations by Mosaic ID or officer email
        /// </summary>
        /// <response code="200">Success. Returns allocations related to the specified ID or officer email</response>
        /// <response code="404">No allocations found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CfsAllocationList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("cfs_allocations")]
        public IActionResult GetChildrensAllocatedWorker([FromQuery] ListAllocationsRequest request)
        {
            return Ok(_childrenAllocationUseCase.Execute(request));
        }

        /// <summary>
        /// Find acs allocations by Mosaic ID or officer email
        /// </summary>
        /// <response code="200">Success. Returns allocations related to the specified ID or officer email</response>
        [ProducesResponseType(typeof(AscAllocationList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("asc-allocations")]
        public IActionResult GetAdultsAllocatedWorker([FromQuery] ListAllocationsRequest request)
        {
            return Ok(_adultsAllocationUseCase.Execute(request));
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
        /// Find cases by Mosaic ID or officer email
        /// </summary>
        /// <response code="200">Success. Returns cases related to the specified ID or officer email</response>
        /// <response code="404">No cases found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("cases-test")]
        public IActionResult ListCasesTest([FromQuery] ListCasesRequest request)
        {
            long mosaicId = 0;
            _ = Int64.TryParse(request.MosaicId, out mosaicId);

            try
            {
                return Ok(_processDataUsecase.Execute(mosaicId, request.WorkerEmail));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
