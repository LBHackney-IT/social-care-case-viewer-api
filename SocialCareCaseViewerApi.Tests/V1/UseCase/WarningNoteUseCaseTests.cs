using System.Collections.Generic;
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
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;

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

        [Test]
        public void ExecuteGetReturnsAGetWarningNoteResponse()
        {
            _mockDatabaseGateway.Setup(
                x => x.GetWarningNotes(It.IsAny<GetWarningNoteRequest>()))
                .Returns(new List<dbWarningNote>());

            var response = _classUnderTest.ExecuteGet(new GetWarningNoteRequest());

            response.Should().NotBeNull();
            response.Should().BeOfType<List<WarningNote>>();
        }

        [Test]
        public void ExecuteGetCallsTheDatabaseGateWayWithAParameter()
        {
            _mockDatabaseGateway.Setup(
                x => x.GetWarningNotes(It.IsAny<GetWarningNoteRequest>()))
                .Returns(new List<dbWarningNote>());

            var request = new GetWarningNoteRequest();

            _classUnderTest.ExecuteGet(request);
            _mockDatabaseGateway.Verify(
                x => x.GetWarningNotes(request), Times.Once);
        }

        [Test]
        public void ExecuteGetReturnsTheExpectedResponse()
        {
            var stubbedWarningNote = _fixture.Build<dbWarningNote>()
                                    .Without(x => x.Person)
                                    .Create();
            var stubbedList = new List<dbWarningNote>
            {
                stubbedWarningNote
            };

            _mockDatabaseGateway.Setup(
                x => x.GetWarningNotes(It.IsAny<GetWarningNoteRequest>()))
                .Returns(stubbedList);

            var expectedResponse = new List<WarningNote>
                {
                    stubbedWarningNote.ToDomain()
                };

            var response = _classUnderTest.ExecuteGet(new GetWarningNoteRequest());

            response.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void ExecutePatchCallsTheDatabaseGatewayOnce()
        {
            var request = _fixture.Create<PatchWarningNoteRequest>();

            _classUnderTest.ExecutePatch(request);

            _mockDatabaseGateway.Verify(x => x.PatchWarningNote(request),
                                    Times.Once);

        }
    }
}
