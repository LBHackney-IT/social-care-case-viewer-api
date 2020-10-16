using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public interface ISccvDbContext
    {
        IMongoCollection<BsonDocument> matProcessCollection { get; set; }

        IMongoCollection<BsonDocument> getCollection();

    }
}
