using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class MashResidentUseCase : IMashResidentUseCase
    {
        private readonly IMashReferralGateway _mashReferralGateway;

        public MashResidentUseCase(IMashReferralGateway mashReferralGateway)
        {
            _mashReferralGateway = mashReferralGateway;
        }

        public MashResidentResponse UpdateMashResident(UpdateMashResidentRequest request, long mashResidentId)
        {
            var response = _mashReferralGateway.UpdateMashResident(request, mashResidentId);
            return response.ToResponse();
        }
    }
}
