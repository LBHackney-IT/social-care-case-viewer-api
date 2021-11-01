using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.MashReferral
{
    [TestFixture]
    public class ResetMashReferralsTests
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
        public void ResettingMashReferralsReturns200Response()
        {
            _mashReferralUseCase.Setup(x => x.Reset());

            var response = _mashReferralController.ResetMashReferrals() as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }
    }
}
