using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
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

        [Test]
        public void WhenGatewaysReturnsPersonNotFoundExceptionThrowsTheException()
        {
            var mashResident = TestHelpers.CreateMashResident().ToDomain();
            var request = TestHelpers.CreateMashResidentUpdateRequest();

            _mashReferralGateway.Setup(x => x.UpdateMashResident(request, mashResident.Id))
                .Throws(new PersonNotFoundException($"Person with id {request.SocialCareId} not found"));

            Action act = () => _mashResidentUseCase.UpdateMashResident(request, mashResident.Id);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"Person with id {request.SocialCareId} not found");
        }

        [Test]
        public void WhenGatewaysReturnsMashResidentNotFoundExceptionThrowsTheException()
        {
            var mashResidentId = TestHelpers.CreateMashResident().Id;
            var request = TestHelpers.CreateMashResidentUpdateRequest();

            _mashReferralGateway.Setup(x => x.UpdateMashResident(request, mashResidentId))
                .Throws(new MashResidentNotFoundException($"MASH resident with id {mashResidentId} not found"));

            Action act = () => _mashResidentUseCase.UpdateMashResident(request, mashResidentId);

            act.Should().Throw<MashResidentNotFoundException>()
                .WithMessage($"MASH resident with id {mashResidentId} not found");
        }
    }
}
