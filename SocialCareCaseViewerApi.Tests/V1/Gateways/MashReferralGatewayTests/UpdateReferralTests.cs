using System;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class UpdateReferralTests : DatabaseTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;

        private readonly Faker _faker = new Faker();

        private Worker _worker = null!;

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, _systemTime.Object, DatabaseContext);

            _worker = TestHelpers.CreateWorker();
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralNotFoundExceptionWhenGetInfrastructureUsingIdReturnsNull()
        {
            var mashReferralId = _faker.Random.Long(100);
            var request = TestHelpers.CreateUpdateMashReferral();

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferralId);

            act.Should().Throw<MashReferralNotFoundException>()
                .WithMessage($"MASH referral with id {mashReferralId} not found");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForScreeningDecisionAndReferralIsNotInScreeningStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"screening\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForContactDecisionAndReferralIsNotInContactStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "CONTACT-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"contact\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForInitialDecisionAndReferralIsNotInInitialStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"initial\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForFinalDecisionAndReferralIsNotInFinalStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"final\"");
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromContactToInitialUpdatesAndReturnsMashReferralDomain()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "CONTACT-DECISION");

            var response = _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            response.Should().BeEquivalentTo(mashReferral.ToDomain());
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromInitialToScreeningUpdatesAndReturnsMashReferralDomain()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "INITIAL");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");

            var response = _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            response.Should().BeEquivalentTo(mashReferral.ToDomain());
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromScreeningToFinalUpdatesAndReturnsMashReferralDomain()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "SCREENING");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");

            var response = _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            response.Should().BeEquivalentTo(mashReferral.ToDomain());
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromFinalToPostFinalUpdatesAndReturnsMashReferralDomain()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "FINAL");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");

            var response = _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            response.Should().BeEquivalentTo(mashReferral.ToDomain());
        }
    }
}
