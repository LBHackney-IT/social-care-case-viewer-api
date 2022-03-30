using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class SearchController : Controller
    {
        private readonly IResidentUseCase _residentUseCase;
        private readonly ISearchUseCase _searchUseCase;

        public SearchController(IResidentUseCase residentUseCase, ISearchUseCase searchUseCase)
        {
            _residentUseCase = residentUseCase;
            _searchUseCase = searchUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResidentInformationList), StatusCodes.Status200OK)]
        [Route("search/person")]
        public IActionResult GetPersonRecordsBySearchQuery([FromQuery] PersonSearchRequest request)
        {
            if (!string.IsNullOrEmpty(request.PersonId))
            {
                return Ok(_residentUseCase.GetResidentsByQuery(new ResidentQueryParam() { MosaicId = request.PersonId }, request.Cursor ?? 0, 20));
            }
            else
            {
                return Ok(_searchUseCase.GetResidentsByQuery(request));
            }
        }
    }
}
