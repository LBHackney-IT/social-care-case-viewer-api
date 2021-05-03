using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Security.Cryptography.X509Certificates;

namespace SocialCareCaseViewerApi.Tests
{
    public class MongoDbTestContext : ISccvDbContext
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _mongoDatabase;
        public IMongoCollection<BsonDocument> matProcessCollection { get; set; }
        public MongoDbTestContext()
        {
            // ADD CA certificate to local trust store
            // DO this once - Maybe when your service starts
            X509Store localTrustStore = new X509Store(StoreName.Root);

            string certificateString = "-----BEGIN CERTIFICATE----- MIIEBjCCAu6gAwIBAgIJAMc0ZzaSUK51MA0GCSqGSIb3DQEBCwUAMIGPMQswCQYD VQQGEwJVUzEQMA4GA1UEBwwHU2VhdHRsZTETMBEGA1UECAwKV2FzaGluZ3RvbjEi MCAGA1UECgwZQW1hem9uIFdlYiBTZXJ2aWNlcywgSW5jLjETMBEGA1UECwwKQW1h em9uIFJEUzEgMB4GA1UEAwwXQW1hem9uIFJEUyBSb290IDIwMTkgQ0EwHhcNMTkw ODIyMTcwODUwWhcNMjQwODIyMTcwODUwWjCBjzELMAkGA1UEBhMCVVMxEDAOBgNV BAcMB1NlYXR0bGUxEzARBgNVBAgMCldhc2hpbmd0b24xIjAgBgNVBAoMGUFtYXpv biBXZWIgU2VydmljZXMsIEluYy4xEzARBgNVBAsMCkFtYXpvbiBSRFMxIDAeBgNV BAMMF0FtYXpvbiBSRFMgUm9vdCAyMDE5IENBMIIBIjANBgkqhkiG9w0BAQEFAAOC AQ8AMIIBCgKCAQEArXnF/E6/Qh+ku3hQTSKPMhQQlCpoWvnIthzX6MK3p5a0eXKZ oWIjYcNNG6UwJjp4fUXl6glp53Jobn+tWNX88dNH2n8DVbppSwScVE2LpuL+94vY 0EYE/XxN7svKea8YvlrqkUBKyxLxTjh+U/KrGOaHxz9v0l6ZNlDbuaZw3qIWdD/I 6aNbGeRUVtpM6P+bWIoxVl/caQylQS6CEYUk+CpVyJSkopwJlzXT07tMoDL5WgX9 O08KVgDNz9qP/IGtAcRduRcNioH3E9v981QO1zt/Gpb2f8NqAjUUCUZzOnij6mx9 McZ+9cWX88CRzR0vQODWuZscgI08NvM69Fn2SQIDAQABo2MwYTAOBgNVHQ8BAf8E BAMCAQYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUc19g2LzLA5j0Kxc0LjZa pmD/vB8wHwYDVR0jBBgwFoAUc19g2LzLA5j0Kxc0LjZapmD/vB8wDQYJKoZIhvcN AQELBQADggEBAHAG7WTmyjzPRIM85rVj+fWHsLIvqpw6DObIjMWokpliCeMINZFV ynfgBKsf1ExwbvJNzYFXW6dihnguDG9VMPpi2up/ctQTN8tm9nDKOy08uNZoofMc NUZxKCEkVKZv+IL4oHoeayt8egtv3ujJM6V14AstMQ6SwvwvA93EP/Ug2e4WAXHu cbI1NAbUgVDqp+DRdfvZkgYKryjTWd/0+1fS8X1bBZVWzl7eirNVnHbSH2ZDpNuY 0SBd8dj5F6ld3t58ydZbrTHze7JJOd8ijySAp4/kiu9UfZWuTPABzDa/DSdz9Dk/ zPW4CXXvhLmE02TA9/HeCw3KEHIwicNuEfw= -----END CERTIFICATE-----";

            string certificateWithoutHeaderAndFooter = certificateString
                                                        .Replace("\\n", "")
                                                        .Replace("-----BEGIN CERTIFICATE-----", "")
                                                        .Replace("-----END CERTIFICATE-----", "");

            var certificateBytes = Convert.FromBase64String(certificateWithoutHeaderAndFooter);
            var certificate = new X509Certificate2(certificateBytes);

            try
            {
                localTrustStore.Open(OpenFlags.ReadWrite);
                localTrustStore.Add(certificate);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Root certificate import failed: " + ex.Message);
                throw;
            }
            finally
            {
                localTrustStore.Close();
            }

            _mongoClient = new MongoClient(new MongoUrl("mongodb://mongo-db:27017/?gssapiServiceName=mongodb"));
            //create a new blank database if database does not exist, otherwise get existing database
            _mongoDatabase = _mongoClient.GetDatabase("social_care_db_test");
            //create collection to hold the documents if it does not exist, otherwise retrieve existing
            matProcessCollection = _mongoDatabase.GetCollection<BsonDocument>("form_data_test");
        }
        public IMongoCollection<BsonDocument> getCollection()
        {
            return matProcessCollection;
        }
    }
}
