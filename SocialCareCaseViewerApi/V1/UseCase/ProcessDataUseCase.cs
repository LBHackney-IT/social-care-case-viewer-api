using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ProcessDataUseCase :IProcessDataUseCase
    {
        private IProcessDataGateway _processDataGateway;
        public ProcessDataUseCase(IProcessDataGateway processDataGateway)
        {
            _processDataGateway = processDataGateway;
        }
        public GetProcessDataResponse Execute(GetProcessDataRequest request)
        {
            var gatewayResult = _processDataGateway.GetProcessData();

            return new GetProcessDataResponse(request, gatewayResult,DateTime.Now);
        }
    }
}
