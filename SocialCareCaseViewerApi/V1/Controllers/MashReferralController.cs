using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Controllers
{
    [Route("api/v1/mash-referral")]
    [ApiController]
    [Produces("application/json")]
    public class MashReferralController : Controller
    {
        private readonly IMashReferralUseCase _mashReferralUseCase;

        public MashReferralController(IMashReferralUseCase mashReferralUseCase)
        {
            _mashReferralUseCase = mashReferralUseCase;
        }

        /// <summary>
        /// Get a mash referrals using id value
        /// </summary>
        /// <response code="200">Successful request. Referrals returned</response>
        /// <response code="404">Mash referral not found</response>
        /// <response code="500">There was a server side error getting the mash referrals</response>
        [ProducesResponseType(typeof(MashReferral), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("{referralId}")]
        public IActionResult GetMashReferral(string referralId)
        {
            var referral = _mashReferralUseCase.GetMashReferralUsingId(referralId);

            if (referral != null)
            {
                return Ok(referral);
            }

            return NotFound();
        }

        /// <summary>
        /// Get a list of mash referrals based on supplied query params
        /// </summary>
        /// <response code="200">Successful request. Referrals returned</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">There was a server side error getting the mash referrals</response>
        [ProducesResponseType(typeof(List<MashReferral>), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetMashReferrals([FromQuery] QueryMashReferrals request)
        {
            var referrals = _mashReferralUseCase.GetMashReferrals(request).ToList();

            return Ok(referrals);
        }

        [HttpPost]
        [Route("reset")]
        public IActionResult ResetMashReferrals()
        {
            _mashReferralUseCase.Reset();

            return Ok();
        }
    }
}
