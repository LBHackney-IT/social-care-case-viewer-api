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
        [Route("{referralId:long}")]
        public IActionResult GetMashReferral(long referralId)
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
        [ProducesResponseType(typeof(List<MashReferral_2>), StatusCodes.Status200OK)]
        [HttpGet]
        public IActionResult GetMashReferrals([FromQuery] QueryMashReferrals request)
        {
            var referrals = _mashReferralUseCase.GetMashReferrals(request).ToList();

            return Ok(referrals);
        }

        /// <summary>
        /// Creates new referral at the 'contact' stage
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Successfully created new referral</response>
        /// <response code="400">Invalid request</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public IActionResult CreateNewContact([FromBody] CreateReferralRequest request)
        {
            var validator = new CreateReferralRequestValidator();
            var validation = validator.Validate(request);
            if (!validation.IsValid)
            {
                return BadRequest(validation.ToString());
            }

            _mashReferralUseCase.CreateNewMashReferral(request);

            return CreatedAtAction(nameof(CreateNewContact), "Successfully created new contact referral");
        }

        /// <summary>
        /// Update a mash referral
        /// </summary>
        /// <response code="200">Successful request. Referrals returned</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">There was a server side error getting the mash referrals</response>
        [ProducesResponseType(typeof(MashReferral), StatusCodes.Status200OK)]
        [HttpPatch]
        [Route("{referralId:long}")]
        public IActionResult UpdateMashReferral([FromBody] UpdateMashReferral request, long referralId)
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
