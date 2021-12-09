using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.MashReferral
{
    [TestFixture]
    public class GetMashReferralTests
    {
        private MashReferralController _mashReferralController = null!;
        private Mock<IMashReferralUseCase> _mashReferralUseCase = null!;

        [SetUp]
        public void Setup()
        {
            _mashReferralUseCase = new Mock<IMashReferralUseCase>();
            _mashReferralController = new MashReferralController(_mashReferralUseCase.Object);
        }

        [Test]
        public void GetMashReferralReturnsMashReferralResponseWith200Response()
        {
            var referral = TestHelpers.CreateMashReferral().ToDomain().ToResponse();
            _mashReferralUseCase.Setup(x => x.GetMashReferralUsingId(referral.Id)).Returns(referral);

            var response = _mashReferralController.GetMashReferral(referral.Id) as ObjectResult;

            response?.Value.Should().BeEquivalentTo(referral);
            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetMashReferralReturns404ResponseWhenNoReferralFound()
        {
            const long nonExistentId = 123L;
            _mashReferralUseCase.Setup(x => x.GetMashReferralUsingId(nonExistentId));

            var response = _mashReferralController.GetMashReferral(nonExistentId) as ObjectResult;

            response?.StatusCode.Should().Be(404);
        }
    }
}
