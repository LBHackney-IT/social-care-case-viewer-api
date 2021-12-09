using System.Collections.Generic;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Domain;
using MashReferral_2 = SocialCareCaseViewerApi.V1.Domain.MashReferral_2;

using SocialCareCaseViewerApi.V1.Boundary.Requests;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IMashReferralGateway
    {
        public void Reset2();
        public IEnumerable<MashReferral_2> GetReferralsUsingQuery(QueryMashReferrals request);

        public MashReferral_2? GetReferralUsingId_2(long requestId);
        public MashReferral_2 UpdateReferral(UpdateMashReferral request, long referralId);
        public void CreateReferral(CreateReferralRequest request);
    }
}
