using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

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

        [HttpPost]
        [Route("reset")]
        public IActionResult ResetMashReferrals()
        {
            _mashReferralUseCase.Reset();

            return Ok();
        }
    }
}
