using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    public class CaseStatusTypeFieldsController : BaseController
    {
        private readonly IGetCaseStatusFieldsUseCase _getCaseStatusFieldsUseCase;

        public CaseStatusTypeFieldsController(IGetCaseStatusFieldsUseCase getCaseStatusFieldsUseCase)
        {
            _getCaseStatusFieldsUseCase = getCaseStatusFieldsUseCase;
        }

        /// <summary>
        /// Get a list of fields and their options for a case status type
        /// </summary>
        /// <response code="200">Found case status type, returns the fields for that type</response>
        /// <response code="404">Case status type not found, or no fields exist</response>
        [ProducesResponseType(typeof(GetCaseStatusFieldsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("/case-status/form-options/{type}")]
        public IActionResult GetCaseStatusTypeFields(string type)
        {
            GetCaseStatusFieldsResponse response =
                _getCaseStatusFieldsUseCase.Execute(new GetCaseStatusFieldsRequest() { Type = type });

            if (response.Fields.Any())
            {
                return Ok(response);
            }

            return NotFound("There are no fields for this type");
        }
    }
}
