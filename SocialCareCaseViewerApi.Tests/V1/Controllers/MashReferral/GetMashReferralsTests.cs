using System.Collections.Generic;
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
    public class GetMashReferralsTests
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
        public void GetMashReferralsReturnsAListOfMashReferralResponseWith200StatusResponse()
        {
            var request = TestHelpers.CreateQueryMashReferral();
            var referral1 = TestHelpers.CreateMashReferral2().ToDomain().ToResponse();
            var referral2 = TestHelpers.CreateMashReferral2().ToDomain().ToResponse();
            var referralResponse =
                new List<SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral_2> { referral1, referral2 };

            _mashReferralUseCase
                .Setup(x => x.GetMashReferrals(request))
                .Returns(referralResponse);

            var response = _mashReferralController.GetMashReferrals(request) as ObjectResult;

            response?.Value.Should().BeEquivalentTo(referralResponse);
            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetMashReferralsReturnsAnEmptyListOfMashReferralResponseWith200StatusResponseWheNoReferralsExist()
        {
            var request = TestHelpers.CreateQueryMashReferral();
            var referralResponse = new List<SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral_2>();

            _mashReferralUseCase
                .Setup(x => x.GetMashReferrals(request))
                .Returns(referralResponse);

            var response = _mashReferralController.GetMashReferrals(request) as ObjectResult;

            response?.Value.Should().BeEquivalentTo(referralResponse);
            response?.StatusCode.Should().Be(200);
        }
    }
}
