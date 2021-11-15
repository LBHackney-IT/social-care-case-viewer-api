using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IMashReferralUseCase
    {
        public MashReferral? GetMashReferralUsingId(string requestId);
        public MashReferral_2? GetMashReferralUsingId_2(string requestId);
        // public MashResident? GetMashResidentsByReferral(string requestId_2);         
        public IEnumerable<MashReferral> GetMashReferrals(QueryMashReferrals request);
        public MashReferral UpdateMashReferral(UpdateMashReferral request, string referralId);
        public void Reset();
    }
}
