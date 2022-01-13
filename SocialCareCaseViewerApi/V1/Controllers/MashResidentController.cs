using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
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

        /// <summary>
        /// Updates a MASH resident attached to a referral
        /// </summary>
        /// <response code="200">Successful. MASH resident details have been updated </response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">There was a server side error updating the MASH resident</response>
        [ProducesResponseType(typeof(MashResidentResponse), StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("{mashResidentId:long}")]
        public IActionResult UpdateMashResident([FromBody] UpdateMashResidentRequest request, long mashResidentId)
        {
            try
            {
                var updatedMashResident = _mashResidentUseCase.UpdateMashResident(request, mashResidentId);
                return Ok(updatedMashResident);
            }
            catch (Exception e) when (
                e is MashResidentNotFoundException ||
                e is PersonNotFoundException)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
