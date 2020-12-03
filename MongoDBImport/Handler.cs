using Amazon.Lambda.Core;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace MongoDBImport
{
    public class Handler
    {
        public string ImportFormData(ILambdaContext context)
        {
            return "Hello world";
        }
    }
}
