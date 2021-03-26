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
            var request = _fixture.Create<CreateWarningNoteRequest>();

            var responseObject = _fixture.Create<CreateWarningNoteResponse>();

            _mockDatabaseGateway
                .Setup(x => x.CreateWarningNote(
                    It.IsAny<CreateWarningNoteRequest>()))
                .Returns(responseObject);

            var response = _classUnderTest.ExecutePost(new CreateWarningNoteRequest());

            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(responseObject);
        }

        [Test]
        public void CreateWarningNoteCallsDatabaseGateway()
        {
            var request = _fixture.Create<CreateWarningNoteRequest>();

            _classUnderTest.ExecutePost(request);

            _mockDatabaseGateway.Verify(x => x.CreateWarningNote(request));
        }

        [Test]
        public void CreateWarningNoteCallsDatabaseGatewayWithParameters()
        {
            var request = _fixture.Create<CreateWarningNoteRequest>();

            _classUnderTest.ExecutePost(request);

            _mockDatabaseGateway.Verify(x => x.CreateWarningNote(
                                    It.Is<CreateWarningNoteRequest>(x => x == request)),
                                    Times.Once);
        }

        [Test]
        public void CreateWarningNotesReturnsCorrectCaseNoteId()
        {
            var request = new CreateWarningNoteRequest();

            var expectedResponse = new CreateWarningNoteResponse() { CaseNoteId = _fixture.Create<string>() };

            _mockDatabaseGateway
                .Setup(x => x.CreateWarningNote(
                    It.Is<CreateWarningNoteRequest>(x => x == request)))
                .Returns(expectedResponse);

            var response = _classUnderTest.ExecutePost(request);

            response.CaseNoteId.Should().BeEquivalentTo(expectedResponse.CaseNoteId);
        }
    }
}
