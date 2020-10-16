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

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class ProcessDataGateway : IProcessDataGateway
    {
        private ISccvDbContext _sccvDbContext;

        public ProcessDataGateway(ISccvDbContext sccvDbContext)
        {
            _sccvDbContext = sccvDbContext;
        }
        public CareCaseData GetProcessData(string processRef)
        {
            //retrieve data by id
            var filter = Builders<BsonDocument>.Filter.Eq("_id", processRef);
            //we will never expect more than one JSON documents matching an ID so we always choose the first/default result
            var result = _sccvDbContext.getCollection().FindAsync(filter).Result.FirstOrDefault();
            //if document does not exist in the DB, then thrown a corresponsing error.
            if (result == null)
            {
                throw new DocumentNotFound();
            }

            return ProcessDataFactory.CreateProcessDataObject(result);
        }
    }

    public class DocumentNotFound : Exception { }
}
