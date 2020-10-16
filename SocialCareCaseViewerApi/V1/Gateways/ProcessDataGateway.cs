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

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class ProcessDataGateway : IProcessDataGateway
    {
        private ISccvDbContext _sccvDbContext;

        public ProcessDataGateway(ISccvDbContext sccvDbContext)
        {
            _sccvDbContext = sccvDbContext;
        }
        public IEnumerable<CareCaseData> GetProcessData(string mosaicId, string officerEmail)
        {
            //retrieve data by id
            var idFilter = Builders<BsonDocument>.Filter.Eq("mosaic_id", mosaicId);
            var emailFilter = Builders<BsonDocument>.Filter.Eq("worker_email", officerEmail);
            var combinedFilter = Builders<BsonDocument>.Filter.Or(mosaicId, officerEmail);

            var result = _sccvDbContext.getCollection().Find(combinedFilter);
            //if document does not exist in the DB, then thrown a corresponsing error.
            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }

            return BsonSerializer.Deserialize<IList<CareCaseData>>((BsonDocument) result);
        }
    }
    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string message) : base(message) { }
    }
}
