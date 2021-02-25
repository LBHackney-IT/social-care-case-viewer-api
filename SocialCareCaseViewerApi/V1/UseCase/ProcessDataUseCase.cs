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
        private IDatabaseGateway _databaseGateway;

        public ProcessDataUseCase(IProcessDataGateway processDataGateway, IDatabaseGateway databaseGateway)
        {
            _processDataGateway = processDataGateway;
            _databaseGateway = databaseGateway;
        }
        public CareCaseDataList Execute(ListCasesRequest request)
        {
            string ncId = null;

            //grab both mosaic id and nc reference id
            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                string ncIdTmp = _databaseGateway.GetNCReferenceByPersonId(request.MosaicId);

                if (!string.IsNullOrEmpty(ncIdTmp))
                {
                    ncId = ncIdTmp;
                }

                string mosaicIdTmp = _databaseGateway.GetPersonIdByNCReference(request.MosaicId);

                if (!string.IsNullOrEmpty(mosaicIdTmp))
                {
                    ncId = request.MosaicId;
                    request.MosaicId = mosaicIdTmp;                    
                }
            }

            var result = _processDataGateway.GetProcessData(request, ncId);

            int? nextCursor = request.Cursor + request.Limit;

            //support page size 1
            if (nextCursor == result.Item2 || result.Item1.Count() < request.Limit) nextCursor = null;

            return new CareCaseDataList
            {
                Cases = result.Item1.ToList(),
                NextCursor = nextCursor
            };
        }

        public Task<string> Execute(CaseNotesDocument caseNotesDoc)
        {
            return _processDataGateway.InsertCaseNoteDocument(caseNotesDoc);
        }
    }
}
