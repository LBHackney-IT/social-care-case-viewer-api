using System;
using System.Globalization;
using System.Threading.Tasks;
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
    [Route("api/v1/cases")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class CaseController : BaseController
    {
        private readonly IProcessDataUseCase _processDataUseCase;

        public CaseController(IProcessDataUseCase processDataUseCase)
        {
            _processDataUseCase = processDataUseCase;
        }

        /// <summary>
        /// Find cases by Mosaic ID or officer email
        /// </summary>
        /// <response code="200">Success. Returns cases related to the specified ID or officer email</response>
        /// <response code="400">One or more dates are invalid or missing</response>
        /// <response code="404">No cases found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CareCaseDataList), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetCases([FromQuery] ListCasesRequest request)
        {
            try
            {
                string? dateValidationError = null;

                if (!string.IsNullOrWhiteSpace(request?.StartDate) && !DateTime.TryParseExact(request.StartDate,
                    "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    dateValidationError += "Invalid start date";
                }

                if (!string.IsNullOrWhiteSpace(request?.EndDate) && !DateTime.TryParseExact(request.EndDate,
                    "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
                {
                    dateValidationError += " Invalid end date";
                }

                return !string.IsNullOrEmpty(dateValidationError) ? StatusCode(400, dateValidationError) : Ok(_processDataUseCase.Execute(request));
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
        [Route("id")]
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
        /// Create new case record
        /// </summary>
        /// <response code="201">Record successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">There was a problem generating a token.</response>
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> CreateCaseNote([FromBody] CreateCaseNoteRequest request)
        {
            var id = await _processDataUseCase.Execute(request).ConfigureAwait(false);
            return StatusCode(201, new { _id = id });
        }
    }
}
