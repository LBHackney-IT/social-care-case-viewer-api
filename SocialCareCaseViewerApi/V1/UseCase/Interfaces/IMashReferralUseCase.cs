using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IMashReferralUseCase
    {
        public MashReferral? GetMashReferralUsingId(long requestId);

        public IEnumerable<MashReferral> GetMashReferrals(QueryMashReferrals request);
        public void CreateNewMashReferral(CreateReferralRequest request);
        public MashReferral UpdateMashReferral(UpdateMashReferral request, long referralId);

        public void Reset();

    }
}
