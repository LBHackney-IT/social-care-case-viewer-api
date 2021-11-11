using System.Collections.Generic;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Domain;
using MashReferral_2 = SocialCareCaseViewerApi.V1.Domain.MashReferral_2;


#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IMashReferralGateway
    {
        public void Reset();
        public void InsertDocument(Infrastructure.MashReferral referral);
        public MashReferral? GetReferralUsingId(string requestId);
        public IEnumerable<MashReferral> GetReferralsUsingFilter(FilterDefinition<Infrastructure.MashReferral> filter);
        public void UpsertRecord(Infrastructure.MashReferral referral);
        public Infrastructure.MashReferral? GetInfrastructureUsingId(string requestId);
        public MashReferral_2? GetReferralUsingId_2(string requestId);
        // public MashResident? GetMashResidentsByReferral(string requestId);

    }
}
