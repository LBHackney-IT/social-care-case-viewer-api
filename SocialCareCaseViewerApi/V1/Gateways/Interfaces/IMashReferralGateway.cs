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
        public void Reset();
        public void InsertDocument(Infrastructure.MashReferral referral);
        public MashReferral? GetReferralUsingId(string requestId);
        public IEnumerable<MashReferral> GetReferralsUsingFilter(FilterDefinition<Infrastructure.MashReferral> filter);

        public IEnumerable<MashReferral_2> GetReferralsUsingQuery(QueryMashReferrals request);

        public void UpsertRecord(Infrastructure.MashReferral referral);
        public Infrastructure.MashReferral? GetInfrastructureUsingId(string requestId);
        public MashReferral_2? GetReferralUsingId_2(long requestId);
        public MashReferral_2 UpdateReferral(UpdateMashReferral request, long referralId);

    }
}
