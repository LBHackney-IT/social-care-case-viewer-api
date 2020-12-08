using System;
using Amazon.Lambda.Core;

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
