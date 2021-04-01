using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CaseNotesUseCaseTests
    {
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private CaseNotesUseCase _caseNotesUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _caseNotesUseCase = new CaseNotesUseCase(_mockSocialCarePlatformAPIGateway.Object);
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = "1" };

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetCaseNotesByPersonId(It.IsAny<string>())).Returns(new ListCaseNotesResponse());

            _caseNotesUseCase.ExecuteGetByPersonId(request.Id);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetCaseNotesByPersonId(It.IsAny<string>()));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWithParameterWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = "1" };

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetCaseNotesByPersonId(request.Id)).Returns(new ListCaseNotesResponse());

            _caseNotesUseCase.ExecuteGetByPersonId(request.Id);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetCaseNotesByPersonId(request.Id));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWhenNoteIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = "1" };

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetCaseNoteById(It.IsAny<string>())).Returns(new CaseNote());

            var response = _caseNotesUseCase.ExecuteGetById(request.Id);

            Assert.IsInstanceOf<CaseNoteResponse>(response);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetCaseNoteById(It.IsAny<string>()));
        }

        [Test]
        public void GetCaseNotesCallsSocialCarePlatformAPIWithParameterWhenNoteIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = "1" };

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetCaseNoteById(request.Id)).Returns(new CaseNote());

            var response = _caseNotesUseCase.ExecuteGetById(request.Id);

            Assert.IsInstanceOf<CaseNoteResponse>(response);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetCaseNoteById(request.Id));
        }
    }
}
