using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
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
        /// Get a mash referrals using id value
        /// </summary>
        /// <response code="200">Successful request. Referrals returned</response>
        /// <response code="404">Mash referral not found</response>
        /// <response code="500">There was a server side error getting the mash referrals</response>
        [ProducesResponseType(typeof(MashReferral_2), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("_2/{referralId}")]
        public IActionResult GetMashReferral_2(string referralId)
        {
            try
            {
                var referral = _mashReferralUseCase.GetMashReferralUsingId_2(referralId);

                if (referral != null)
                {
                    return Ok(referral);
                }
            }
            catch (Exception e) when (
            e is MashReferralNotFoundException ||
            e is WorkerNotFoundException ||
            e is MashReferralStageMismatchException)
            {
                return BadRequest(e.Message);
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

        /// <summary>
        /// Update a mash referral
        /// </summary>
        /// <response code="200">Successful request. Referrals returned</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">There was a server side error getting the mash referrals</response>
        [ProducesResponseType(typeof(MashReferral), StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("{referralId}")]
        public IActionResult UpdateMashReferral([FromBody] UpdateMashReferral request, string referralId)
        {
            var validator = new UpdateMashReferralValidator();
            var validation = validator.Validate(request);
            if (!validation.IsValid)
            {
                return BadRequest(validation.ToString());
            }

            try
            {
                var updatedReferral = _mashReferralUseCase.UpdateMashReferral(request, referralId);
                return Ok(updatedReferral);
            }
            catch (Exception e) when (
                e is MashReferralNotFoundException ||
                e is WorkerNotFoundException ||
                e is MashReferralStageMismatchException)
            {
                return BadRequest(e.Message);
            }
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
