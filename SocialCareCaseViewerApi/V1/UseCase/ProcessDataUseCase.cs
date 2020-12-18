using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using System.Linq;
using System.Threading.Tasks;

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
            //check whether provided mosaic id has a lookup value, so records with nc references can be matched
            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                string mosaicID = _databaseGateway.GetNCReferenceByPersonId(request.MosaicId);

                if (!string.IsNullOrEmpty(mosaicID))
                {
                    request.MosaicId = mosaicID;
                }
            }

            request.Limit = request.Limit > 100 ? 100 : request.Limit;
            var result = _processDataGateway.GetProcessData(request);

            var nextCursor = result.Count() == request.Limit ? result.Max(r => r.RecordId) : "";

            return new CareCaseDataList
            {
                Cases = result.ToList(),
                NextCursor = nextCursor
            };
        }

        public Task<string> Execute(CaseNotesDocument caseNotesDoc)
        {
            return _processDataGateway.InsertCaseNoteDocument(caseNotesDoc);
        }
    }
}
