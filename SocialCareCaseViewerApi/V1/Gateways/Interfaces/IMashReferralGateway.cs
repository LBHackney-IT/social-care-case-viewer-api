using System.Collections.Generic;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Domain;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IMashReferralGateway
    {
        public MashReferral? GetReferralUsingId(string requestId);
        public IEnumerable<MashReferral> GetReferralsUsingFilter(FilterDefinition<Infrastructure.MashReferral> filter);
        public void Reset();
        public void InsertDocument(Infrastructure.MashReferral referral);
    }
}
