using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Controllers
{
    [Route("api/v1/mash-resident")]
    [ApiController]
    [Produces("application/json")]
    public class MashResidentController : Controller
    {
        private readonly IMashResidentUseCase _mashResidentUseCase;

        public MashResidentController(IMashResidentUseCase mashResidentUseCase)
        {
            _mashResidentUseCase = mashResidentUseCase;
        }

        [ProducesResponseType(typeof(MashResidentResponse), StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("{mashResidentId:long}")]
        public IActionResult UpdateMashResident(UpdateMashResidentRequest request, long mashResidentId)
        {
            var updatedMashResident = _mashResidentUseCase.UpdateMashResident(request, mashResidentId);
            return Ok(updatedMashResident);
        }
    }
}
