using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class WarningNotesUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private WarningNotesUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _classUnderTest = new WarningNotesUseCase(_mockDatabaseGateway.Object);
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
        public void PostWarningNoteCallsDatabaseGateway()
        {
            var request = _fixture.Create<PostWarningNoteRequest>();

            _classUnderTest.ExecutePost(request);

            _mockDatabaseGateway.Verify(x => x.PostWarningNote(request));
        }

        [Test]
        public void PostWarningNoteCallsDatabaseGatewayWithParameters()
        {
            var request = _fixture.Create<PostWarningNoteRequest>();

            _classUnderTest.ExecutePost(request);

            _mockDatabaseGateway.Verify(x => x.PostWarningNote(
                                    It.Is<PostWarningNoteRequest>(x => x == request)),
                                    Times.Once);
        }

        [Test]
        public void PostWarningNotesReturnsCorrectCaseNoteId()
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
    }
}
