using System;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ProcessDataUseCase :IProcessDataUseCase
    {
        private IProcessDataGateway _processDataGateway;
        public ProcessDataUseCase(IProcessDataGateway processDataGateway)
        {
            _processDataGateway = processDataGateway;
        }
        public GetProcessDataResponse Execute(string mosaicId, string officerEmail)
        {
            var gatewayResult = _processDataGateway.GetProcessData(mosaicId, officerEmail);

            return new GetProcessDataResponse(request, gatewayResult,DateTime.Now);
        }
    }
}
