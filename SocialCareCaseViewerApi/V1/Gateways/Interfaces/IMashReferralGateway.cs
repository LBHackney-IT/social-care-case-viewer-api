using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IMashReferralGateway
    {
        public void Reset();
        public void InsertDocument(MashReferral referral);
    }
}
