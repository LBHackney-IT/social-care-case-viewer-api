using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
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
        public void GetResidentCasesCallMongoGatewayAndReturnsResidentsSubmittedCases()
        {
            var request = TestHelpers.CreateListCasesRequest();

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, residentId: int.Parse(request.MosaicId ?? ""))
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(
                () => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));
            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>()))
                .Returns(expectedResponse);

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.Cases.Count.Should().Be(2);
            response.Cases.Should().BeEquivalentTo(expectedResponse.Take(2).Select(x => x.ToCareCaseData(request)).ToList());
        }
    }
}
