using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.MashReferral
{
    [TestFixture]
    public class CreateMashReferralTests
    {
        private Mock<IMashReferralUseCase> _mashReferralUseCase;
        private MashReferralController _mashReferralController;

        [SetUp]
        public void Setup()
        {
            _mashReferralUseCase = new Mock<IMashReferralUseCase>();
            _mashReferralController = new MashReferralController(_mashReferralUseCase.Object);
        }

        [Test]
        public void CallsMashReferralUseCaseToInsertNewReferral()
        {
            _mashReferralUseCase.Setup(x => x.CreateNewMashReferral(It.IsAny<CreateReferralRequest>()));
            var request = TestHelpers.CreateNewMashReferralRequest();

            _mashReferralController.CreateNewContact(request);

            _mashReferralUseCase.Verify(x => x.CreateNewMashReferral(request));
        }

        [Test]
        public void WhenRequestIsValidReturnsSuccessfulResponse()
        {
            var referralRequest = TestHelpers.CreateNewMashReferralRequest();

            _mashReferralUseCase.Setup(x => x.CreateNewMashReferral(referralRequest)).Verifiable();

            var response = _mashReferralController.CreateNewContact(referralRequest) as ObjectResult;

            response?.StatusCode.Should().Be(201);
            response?.Value.Should().Be("Successfully created new contact referral");
        }

        [Test]
        public void WhenRequestIsInvalidReturns400()
        {
            const string exceptionMessage = "Referrer must have at least one character";
            var referralRequest = TestHelpers.CreateNewMashReferralRequest();
            referralRequest.Referrer = "";

            var response = _mashReferralController.CreateNewContact(referralRequest) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }
    }
}
