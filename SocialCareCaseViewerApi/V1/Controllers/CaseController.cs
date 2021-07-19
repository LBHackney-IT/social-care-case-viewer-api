using System;
using System.Collections.Generic;
using System.Net;
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
        private readonly ICaseRecordsUseCase _caseRecordsUseCase;
        private readonly ICreateRequestAuditUseCase _createRequestAuditUseCase;

        public CaseController(ICaseRecordsUseCase caseRecordsUseCase, ICreateRequestAuditUseCase createRequestAuditUseCase)
        {
            _caseRecordsUseCase = caseRecordsUseCase;
            _createRequestAuditUseCase = createRequestAuditUseCase;
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
                return Ok(_caseRecordsUseCase.GetResidentCases(request));
            }
            catch (DocumentNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Find a case by unique Record ID
        /// </summary>
        /// <response code="200">Success. Returns case related to the specified ID</response>
        /// <response code="404">No cases found for the specified ID or officer email</response>
        [ProducesResponseType(typeof(CareCaseData), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{Id}")]
        public IActionResult GetCaseByRecordId([FromQuery] GetCaseNotesRequest request)
        {
            if (request.AuditingEnabled && !string.IsNullOrEmpty(request.UserId) && !string.IsNullOrEmpty(request.ResidentId))
            {
                var auditRequest = new CreateRequestAuditRequest()
                {
                    ActionName = "view_case_note",
                    UserName = request.UserId,
                    Metadata = new Dictionary<string, object>() {
                        { "residentId", request.ResidentId },
                        { "casenoteId", request.Id }
                    }
                };

                _createRequestAuditUseCase.Execute(auditRequest);
            }

            var caseRecord = _caseRecordsUseCase.Execute(request.Id);

            if (caseRecord == null)
            {
                return NotFound("Document Not Found");
            }

            return Ok(caseRecord);
        }

        /// <summary>
        /// Create new case record
        /// </summary>
        /// <response code="201">Record successfully inserted</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> CreateCaseRecord([FromBody] CreateCaseNoteRequest request)
        {
            var id = await _caseRecordsUseCase.Execute(request).ConfigureAwait(false);
            return StatusCode(201, new { _id = id });
        }
    }
}
