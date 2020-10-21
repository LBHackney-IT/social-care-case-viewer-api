using System;
using System.Linq;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ProcessDataUseCase : IProcessDataUseCase
    {
        private IProcessDataGateway _processDataGateway;
        public ProcessDataUseCase(IProcessDataGateway processDataGateway)
        {
            _processDataGateway = processDataGateway;
        }
        public CareCaseDataList Execute(ListCasesRequest request)
        {
            var result = _processDataGateway.GetProcessData(request);

            return new CareCaseDataList
            {
                Cases = result.ToList()
            };
        }

        public CareCaseDataList Execute(long mosaicId, string officerEmail)
        {
            var result = _processDataGateway.GetProcessData(mosaicId);

            return new CareCaseDataList
            {
                Cases = result.ToList()
            };
        }
    }
}
