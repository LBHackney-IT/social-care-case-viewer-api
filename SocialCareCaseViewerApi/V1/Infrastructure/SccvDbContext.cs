using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class SccvDbContext : ISccvDbContext
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _mongoDatabase;
        public IMongoCollection<BsonDocument> matProcessCollection { get; set; }
        public SccvDbContext()
        {
            string cAfile = Environment.GetEnvironmentVariable("RDS_CA_2019");

            // ADD CA certificate to local trust store
            // DO this once - Maybe when your service starts
            X509Store localTrustStore = new X509Store(StoreName.Root);
            //  string caContentString = System.IO.File.ReadAllText(cAfile);

            // X509Certificate2 caCert = new X509Certificate2(Encoding.ASCII.GetBytes(cAfile));

            // try
            // {
            //     localTrustStore.Open(OpenFlags.ReadWrite);
            //     localTrustStore.Add(caCert);
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine("Root certificate import failed: " + ex.Message);
            //     throw;
            // }
            // finally
            // {
            //     localTrustStore.Close();
            // }

            // string MONGO_CONN_STRING = Environment.GetEnvironmentVariable("MONGO_CONN_STRING") ??
            //                      @"mongodb://localhost:27017/?gssapiServiceName=mongodb";
            _mongoClient = new MongoClient(new MongoUrl(@"mongodb://localhost:27017/?gssapiServiceName=mongodb"));
            //create a new blank database if database does not exist, otherwise get existing database
            _mongoDatabase = _mongoClient.GetDatabase("social_care_db");
            //create collection to hold the documents if it does not exist, otherwise retrieve existing
            matProcessCollection = _mongoDatabase.GetCollection<BsonDocument>("form_data");
        }
        public IMongoCollection<BsonDocument> getCollection()
        {
            return matProcessCollection;
        }
    }
}
