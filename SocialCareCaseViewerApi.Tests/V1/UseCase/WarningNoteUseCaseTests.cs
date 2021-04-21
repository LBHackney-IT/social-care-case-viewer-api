using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;
using dbWarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class WarningNoteUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private WarningNoteUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _classUnderTest = new WarningNoteUseCase(_mockDatabaseGateway.Object);
            _fixture = new Fixture();
        }

        #region Create Warning Note
        [Test]
        public void ExecutePostReturnsTheResponse()
        {
            var request = _fixture.Create<PostWarningNoteRequest>();

            var responseObject = _fixture.Create<PostWarningNoteResponse>();

            _mockDatabaseGateway
                .Setup(x => x.PostWarningNote(
                    It.IsAny<PostWarningNoteRequest>()))
                .Returns(responseObject);

            var response = _classUnderTest.ExecutePost(new PostWarningNoteRequest());

            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(responseObject);
        }

        [Test]
        public void ExecutePostCallsDatabaseGatewayWithParametersOneTime()
        {
            var request = _fixture.Create<PostWarningNoteRequest>();

            _classUnderTest.ExecutePost(request);

            _mockDatabaseGateway.Verify(x => x.PostWarningNote(request),
                                    Times.Once);
        }

        [Test]
        public void ExecutePostReturnsCorrectCaseNoteId()
        {
            var request = new PostWarningNoteRequest();

            var expectedResponse = new PostWarningNoteResponse() { CaseNoteId = _fixture.Create<string>() };

            _mockDatabaseGateway
                .Setup(x => x.PostWarningNote(
                    It.Is<PostWarningNoteRequest>(x => x == request)))
                .Returns(expectedResponse);

            var response = _classUnderTest.ExecutePost(request);

            response.CaseNoteId.Should().BeEquivalentTo(expectedResponse.CaseNoteId);
        }
        #endregion

        #region Get Warning Note
        [Test]
        public void ExecuteGetReturnsAGetWarningNoteResponse()
        {
            var testPersonId = _fixture.Create<long>();

            _mockDatabaseGateway.Setup(
                x => x.GetWarningNotes(It.IsAny<long>()))
                .Returns(new List<dbWarningNote>());

            var response = _classUnderTest.ExecuteGet(testPersonId);

            response.Should().NotBeNull();
            response.Should().BeOfType<ListWarningNotesResponse>();
        }

        [Test]
        public void ExecuteGetCallsTheDatabaseGateWayWithAParameter()
        {
            var testPersonId = _fixture.Create<long>();

            _mockDatabaseGateway.Setup(
                x => x.GetWarningNotes(It.IsAny<long>()))
                .Returns(new List<dbWarningNote>());

            _classUnderTest.ExecuteGet(testPersonId);
            _mockDatabaseGateway.Verify(
                x => x.GetWarningNotes(testPersonId), Times.Once);
        }

        [Test]
        public void ExecuteGetReturnsTheExpectedResponse()
        {
            var testPersonId = _fixture.Create<long>();

            var stubbedWarningNotesList = _fixture.Build<dbWarningNote>()
                                    .Without(x => x.Person)
                                    .CreateMany().ToList();

            _mockDatabaseGateway.Setup(
                x => x.GetWarningNotes(It.IsAny<long>()))
                .Returns(stubbedWarningNotesList);

            var expectedResponse = new ListWarningNotesResponse
            {
                WarningNotes = stubbedWarningNotesList.Select(wn => wn.ToDomain()).ToList()
            };

            var response = _classUnderTest.ExecuteGet(testPersonId);

            response.Should().BeEquivalentTo(expectedResponse);
        }
        #endregion
    }
}
