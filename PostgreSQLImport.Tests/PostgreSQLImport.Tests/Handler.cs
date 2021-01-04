using Amazon.Lambda.Core;
using Amazon.S3.Util;
using Moq;
using Npgsql;
using NUnit.Framework;
using PostgreSQLImport.Database;
using System.Collections.Generic;

namespace PostgreSQLImport.Tests
{

    //[TestFixture]
    //public class HandlerTest : DatabaseTests
    //{
    //    [Test]
    //    public void CanLoadACsvIntoTheDatabase()
    //    {
    //        var mockDatabaseActions = new Mock<IDatabaseActions>();
    //        var handler = new Handler(mockDatabaseActions.Object);

    //        var bucketData = new S3EventNotification.S3Entity()
    //        {
    //            Bucket = new S3EventNotification.S3BucketEntity() { Name = "testBucket" },
    //            Object = new S3EventNotification.S3ObjectEntity { Key = "test/key.csv" }
    //        };
    //        //S3 record mock
    //        var testRecord = new S3EventNotification.S3EventNotificationRecord
    //        {
    //            AwsRegion = "eu-west-2",
    //            S3 = bucketData
    //        };

    //        var s3EventMock = new S3EventNotification
    //        {
    //            Records = new List<S3EventNotification.S3EventNotificationRecord> { testRecord }
    //        };

    //        string tableName = "test";

    //        var contextMock = new Mock<ILambdaContext>();
    //        //set up Database actions
    //        mockDatabaseActions.Setup(x => x.CopyDataToDatabase(contextMock.Object, testRecord.AwsRegion, bucketData.Bucket.Name, bucketData.Object.Key, tableName));
    //        mockDatabaseActions.Setup(x => x.TruncateTable(contextMock.Object, It.IsAny<string>()));
    //        mockDatabaseActions.Setup(x => x.SetupDatabase(contextMock.Object)).Returns(() => new NpgsqlConnection());

    //        Assert.DoesNotThrow(() => handler.ImportData(s3EventMock, contextMock.Object));
    //        mockDatabaseActions.Verify(y => y.SetupDatabase(contextMock.Object), Times.Once);
    //        mockDatabaseActions.Verify(y => y.TruncateTable(contextMock.Object, It.IsAny<string>()), Times.Once);
    //        mockDatabaseActions.Verify(y => y.CopyDataToDatabase(contextMock.Object, testRecord.AwsRegion, bucketData.Bucket.Name, bucketData.Object.Key, tableName), Times.Once);
    //    }
    //}
}
