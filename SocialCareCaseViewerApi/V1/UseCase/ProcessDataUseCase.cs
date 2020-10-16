using System;
using System.Linq;
using System.Threading.Tasks;
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
        public CareCaseDataList Execute(string mosaicId, string officerEmail)
        {
            var result = _processDataGateway.GetProcessData(mosaicId, officerEmail);

            return new CareCaseDataList
            {
                Cases = result.ToList()
            };
        }
    }
}
