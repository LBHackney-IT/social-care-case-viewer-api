using AutoFixture;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ProcessDataGatewayTests : MongoDbTests
    {
        private ProcessDataGateway _classUnderTest;
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private Mock<ISccvDbContext> _mockSccvDbContext;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockSccvDbContext = new Mock<ISccvDbContext>();
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _mockSccvDbContext.Setup(x => x.getCollection()).Returns(collection);
            _classUnderTest = new ProcessDataGateway(_mockSccvDbContext.Object, _mockSocialCarePlatformAPIGateway.Object);
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
                                    .With(x => x.Cursor, 0)
                                    .With(x => x.Limit, 10)
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
            responseList.First().FirstName.Should().BeEquivalentTo(stubbedCaseData.FirstName);
            responseList.First().LastName.Should().BeEquivalentTo(stubbedCaseData.LastName);
            responseList.First().OfficerEmail.Should().BeEquivalentTo(stubbedCaseData.WorkerEmail);
            responseList.First().FormName.Should().BeEquivalentTo(stubbedCaseData.FormName);
        }
    }
}
