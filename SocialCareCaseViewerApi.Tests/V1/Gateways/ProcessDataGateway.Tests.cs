using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ProcessDataGatewayTests : MongoDbTests
    {
        private ProcessDataGateway _classUnderTest;
        private Mock<ISccvDbContext> _mockSccvDbContext;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockSccvDbContext = new Mock<ISccvDbContext>();
            _mockSccvDbContext.Setup(x => x.getCollection()).Returns(collection);
            _classUnderTest = new ProcessDataGateway(_mockSccvDbContext.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void GetProcessDataShouldRetrieveACollectionFromTheDatabase()
        {
            var stubbedRequest = _fixture.Build<ListCasesRequest>()
                                    .Without(p => p.MosaicId)
                                    .Without(p => p.StartDate)
                                    .Without(p => p.EndDate)
                                    .With(x => x.ExactNameMatch, false)
                                    .Create();

            var stubbedCaseData = _fixture.Build<CaseNoteBase>()
                                    .With(x => x.FirstName, stubbedRequest.FirstName)
                                    .With(x => x.LastName, stubbedRequest.LastName)
                                    .With(x => x.WorkerEmail, stubbedRequest.WorkerEmail)
                                    .With(x => x.FormName, stubbedRequest.FormName)
                                    .Create();

            Console.WriteLine(stubbedCaseData);
            var bsonCareCaseData = BsonDocument.Parse(JsonConvert.SerializeObject(stubbedCaseData));
            collection.InsertOne(bsonCareCaseData);

            var response = _classUnderTest.GetProcessData(stubbedRequest, null);
            var responseList = response.Item1.ToList();

            responseList.Should().BeOfType<List<CareCaseData>>();
            response.Item1.ToList().Should().BeEquivalentTo(stubbedCaseData);
        }
    }
}
