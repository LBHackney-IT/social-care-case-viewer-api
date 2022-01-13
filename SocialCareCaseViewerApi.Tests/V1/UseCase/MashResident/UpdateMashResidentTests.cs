using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashResident
{
    [TestFixture]
    public class UpdateMashResidentTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private IMashResidentUseCase _mashResidentUseCase = null!;

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _mashResidentUseCase = new MashResidentUseCase(_mashReferralGateway.Object);
        }

        [Test]
        public void SuccessfulUpdateReturnsMashResidentResponse()
        {
            var linkRequest = TestHelpers.CreateMashResidentUpdateRequest();

            var mashResident = TestHelpers.CreateMashResident().ToDomain();
            mashResident.SocialCareId = linkRequest.SocialCareId;

            _mashReferralGateway
                .Setup(x => x.UpdateMashResident(linkRequest, mashResident.Id))
                .Returns(mashResident);

            var response = _mashResidentUseCase.UpdateMashResident(linkRequest, mashResident.Id);

            response.Should().BeEquivalentTo(mashResident.ToResponse(), options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                return options;
            });

            response.SocialCareId.Should().Be(mashResident.SocialCareId);
        }
    }
}
