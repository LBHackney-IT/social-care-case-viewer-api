using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CaseRecordsUseCaseTests
    {
        private Mock<IProcessDataGateway> _mockProcessDataGateway = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateWay = null!;
        private Mock<IMongoGateway> _mockMongoGateway = null!;
        private CaseRecordsUseCase _caseRecordsUseCase = null!;

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockDatabaseGateWay = new Mock<IDatabaseGateway>();
            _mockMongoGateway = new Mock<IMongoGateway>();

            _caseRecordsUseCase = new CaseRecordsUseCase(_mockProcessDataGateway.Object, _mockDatabaseGateWay.Object, _mockMongoGateway.Object);
        }

        [Test]
        public void GetResidentCasesReturnsResidentsWithMosaicIdProvided()
        {
            var request = TestHelpers.CreateListCasesRequest(mosaicId: "1");
            var residents = new List<Person>
            {
                new Person { Id = 2, FirstName = "foo2", LastName = "bar2", DateOfBirth = DateTime.Now }
                 new Person { Id = 1, FirstName = "foo1", LastName = "bar1", DateOfBirth = DateTime.Now },
            };

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residents: residents),
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns("1");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns("1");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(
                () => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));
            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 1));

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.Cases.Count.Should().Be(1);
            response.Cases.First().FirstName.Should().Be("foo1");
            response.Cases.First().LastName.Should().Be("bar1");
        }

        [Test]
        public void GetResidentCasesReturnsFirstResidentsWhenMosaicIdNotProvided()
        {
            var request = TestHelpers.CreateListCasesRequest(workerEmail: "example@hackney.gov.uk");
            var residents = new List<Person>
            {
                new Person { FirstName = "foo1", LastName = "bar1", DateOfBirth = DateTime.Now },
                new Person { FirstName = "foo2", LastName = "bar2", DateOfBirth = DateTime.Now }
            };

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residents: residents),
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns("1");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns("1");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(
                () => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));
            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 1));

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.Cases.Count.Should().Be(1);
            response.Cases.First().FirstName.Should().Be("foo1");
            response.Cases.First().LastName.Should().Be("bar1");
        }
    }
}
