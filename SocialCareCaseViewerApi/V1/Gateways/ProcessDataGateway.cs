using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using MongoDB.Bson.Serialization;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Factories;
using System.Text.RegularExpressions;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class ProcessDataGateway : IProcessDataGateway
    {
        private ISccvDbContext _sccvDbContext;
        private ISccvDbContextTemp _sccvDbContextTemp;

        public ProcessDataGateway(ISccvDbContext sccvDbContext, ISccvDbContextTemp sccvDbContextTemp)
        {
            _sccvDbContext = sccvDbContext;
            _sccvDbContextTemp = sccvDbContextTemp;
        }
        public IEnumerable<CareCaseData> GetProcessData(ListCasesRequest request)
        {
            long mosaicId = 0;
            if (!string.IsNullOrEmpty(request.MosaicId))
            {

                if (!Int64.TryParse(request.MosaicId, out mosaicId))
                {
                    throw new Exception();
                }
            }
            var filter = !string.IsNullOrEmpty(request.WorkerEmail) ?
                Builders<BsonDocument>.Filter.Eq("worker_email", request.WorkerEmail)
                : Builders<BsonDocument>.Filter.Eq("mosaic_id", mosaicId);

            var result = _sccvDbContext.getCollection().Find(filter).ToList();
            //if document does not exist in the DB, then thrown a corresponsing error.
            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }
            return ResponseFactory.ToResponse(result);
        }

        public IEnumerable<CareCaseData> GetProcessData(long mosaicId, string officerEmail)
        {
            var filter = !string.IsNullOrEmpty(officerEmail) ?
               Builders<BsonDocument>.Filter.Eq("worker_email", officerEmail)
               : "{$or: [{ mosaic_id: " + mosaicId.ToString() + "}, { mosaic_id: /" + mosaicId.ToString() + "/}]}";

            /*var sFilter = "{ $expr: " +
                                "{ $regexMatch: " +
                                    "{ input: { $toString: $mosaic_id }, $regex: /" + mosaicId.ToString() + "/ } " +
                            "} }";*/

            //var sFilter = "{$or: [{ mosaic_id: " + mosaicId.ToString() + "}, { mosaic_id: /" + mosaicId.ToString() +"/}]}";

            /*var result = (filter != null) ? _sccvDbContextTemp.getCollection().Find(filter).ToList()
                : _sccvDbContextTemp.getCollection().Find(sFilter).ToList();*/

            //if document does not exist in the DB, then thrown a corresponsing error.

            var result = _sccvDbContextTemp.getCollection().Find(filter).ToList();

            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }
            return ResponseFactory.ToResponse(result);
        }
    }
}
