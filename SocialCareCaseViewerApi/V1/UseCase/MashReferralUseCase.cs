using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class MashReferralUseCase : IMashReferralUseCase
    {
        private readonly IMashReferralGateway _mashReferralGateway;

        public MashReferralUseCase(IMashReferralGateway mashReferralGateway)
        {
            _mashReferralGateway = mashReferralGateway;
        }

        public void Reset()
        {
            _mashReferralGateway.Reset();

            var referral1 = new MashReferral { };
            var referral2 = new MashReferral { };
            var referral3 = new MashReferral { };
            var referral4 = new MashReferral { };
            var referral5 = new MashReferral { };

            _mashReferralGateway.InsertDocument(referral1);
            _mashReferralGateway.InsertDocument(referral2);
            _mashReferralGateway.InsertDocument(referral3);
            _mashReferralGateway.InsertDocument(referral4);
            _mashReferralGateway.InsertDocument(referral5);
        }
    }
}
