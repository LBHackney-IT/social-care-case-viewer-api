using System.Collections.Generic;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ProcessDataGatewayTests : DatabaseTests
    {
        private ProcessDataGateway _classUnderTest;
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private Mock<ISccvDbContext> _mockSccvDbContext;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockSccvDbContext = new Mock<ISccvDbContext>();
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _classUnderTest = new ProcessDataGateway(_mockSccvDbContext.Object, _mockSocialCarePlatformAPIGateway.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void GetProcessDataShouldCallGetHistoricCaseNotesAndVisitsByPersonId()
        {
            var request = _fixture.Create<ListCasesRequest>();
            request.MosaicId = "55";

            _mockSocialCarePlatformAPIGateway
                .Setup(x => x.GetHistoricCaseNotesAndVisitsByPersonId(55))
                .Returns(new List<ResidentHistoricRecord> { TestHelper.CreateResidentHistoricRecordCaseNote(), TestHelper.CreateResidentHistoricRecordVisit() });

            _classUnderTest.GetProcessData(request, "52");
        }

    }
}
