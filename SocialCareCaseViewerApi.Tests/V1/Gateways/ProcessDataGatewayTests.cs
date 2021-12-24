using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ProcessDataGatewayTests : DatabaseTests
    {
        private Mock<IHistoricalSocialCareGateway> _mockHistoricalSocialCareGateway = null!;
        private ProcessDataGateway _processDataGateway = null!;
        private ListCasesRequest _listCasesRequest = null!;
        private Fixture _fixture = null!;
        private readonly long _mosaicId = 1L;

        [SetUp]
        public void SetUp()
        {
            _mockHistoricalSocialCareGateway = new Mock<IHistoricalSocialCareGateway>();
            _processDataGateway = new ProcessDataGateway(MongoDbTestContext, _mockHistoricalSocialCareGateway.Object);

            _listCasesRequest = TestHelpers.CreateListCasesRequest(mosaicId: _mosaicId);
            _mockHistoricalSocialCareGateway.Setup(x => x.GetVisitInformationByPersonId(It.IsAny<long>())).Returns(new List<Visit>());
            _mockHistoricalSocialCareGateway.Setup(x => x.GetAllCaseNotes(It.IsAny<long>())).Returns(new List<CaseNote>());

            _fixture = new Fixture();
        }

        [Test]
        public void GetProcessDataCallsHistoricalSocialCareGatewayWhenMosaicIDIsProvided()
        {
            _processDataGateway.GetProcessData(_listCasesRequest, null);
            _mockHistoricalSocialCareGateway.Verify(x => x.GetVisitInformationByPersonId(It.IsAny<long>()), Times.Once);
            _mockHistoricalSocialCareGateway.Verify(x => x.GetAllCaseNotes(It.IsAny<long>()), Times.Once);
        }

        [Test]
        public void GetProcessDataCallsHistoricalSocialCareGatewaysGetVisitsByPersonIdMethodWithCorrectParameterWhenMosaicIDIsProvided()
        {
            _processDataGateway.GetProcessData(_listCasesRequest, null);
            _mockHistoricalSocialCareGateway.Verify(x => x.GetVisitInformationByPersonId(_mosaicId), Times.Once);
        }

        [Test]
        public void GetProcessDataCallsHistoricalSocialCareGatewaysGetCaseNotesByPersonIdMethodWithCorrectParameterWhenMosaicIDIsProvided()
        {
            _processDataGateway.GetProcessData(_listCasesRequest, null);
            _mockHistoricalSocialCareGateway.Verify(x => x.GetAllCaseNotes(_mosaicId), Times.Once);
        }

        [Test]
        public void GetProcessDataReturnsHistoricalVisitsForGivenMosaicId()
        {
            var historicalVisitsForMatchingPerson = TestHelpers.CreateVisit(personId: _mosaicId);

            var visits = new List<Visit>
            {
                historicalVisitsForMatchingPerson
            };

            _mockHistoricalSocialCareGateway.Setup(x => x.GetVisitInformationByPersonId(_mosaicId)).Returns(visits);

            var response = _processDataGateway.GetProcessData(_listCasesRequest, null);

            response.Item1.Count().Should().Be(visits.Count);
            (response.Item1 is IEnumerable<CareCaseData>).Should().BeTrue();
        }

        [Test]
        public void GetProcessDataReturnsHistoricalCaseNotesForGivenMosaicId()
        {
            var caseNote = TestHelpers.CreateCaseNote();
            var caseNotes = new List<CaseNote>
            {
                caseNote
            };

            _mockHistoricalSocialCareGateway.Setup(x => x.GetAllCaseNotes(_mosaicId)).Returns(caseNotes);

            var response = _processDataGateway.GetProcessData(_listCasesRequest, null);

            response.Item1.Count().Should().Be(caseNotes.Count);
            (response.Item1 is IEnumerable<CareCaseData>).Should().BeTrue();
        }

        [Test]
        [TestCase("not a number")]
        [TestCase("9223372036854775808555")] //out of range
        public void ThrowsProcessDataGatewayExceptionWhenPersonIdConversionFails(string personId)
        {
            _listCasesRequest.MosaicId = personId;

            Action act = () => _processDataGateway.GetProcessData(_listCasesRequest, null);

            act.Should().Throw<ProcessDataGatewayException>().WithMessage($"PersonId conversion failure for id {_listCasesRequest.MosaicId}");
        }
    }
}
