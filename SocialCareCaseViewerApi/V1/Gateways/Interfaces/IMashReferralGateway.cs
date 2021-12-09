using System.Collections.Generic;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IMashReferralGateway
    {
        public void Reset();
        public IEnumerable<MashReferral> GetReferralsUsingQuery(QueryMashReferrals request);

        public MashReferral? GetReferralUsingId(long requestId);
        public MashReferral UpdateReferral(UpdateMashReferral request, long referralId);
        public MashReferral CreateReferral(CreateReferralRequest request);
    }
}
