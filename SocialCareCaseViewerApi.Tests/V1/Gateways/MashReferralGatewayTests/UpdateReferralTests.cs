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

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class UpdateReferralTests : DatabaseTests
    {
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;

        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_systemTime.Object, DatabaseContext);
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
                .WithMessage($"Referral {mashReferral.Id} is in stage '{mashReferral.Stage}', this request requires the referral to be in stage 'SCREENING'");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForContactDecisionAndReferralIsNotInContactStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "CONTACT-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage '{mashReferral.Stage}', this request requires the referral to be in stage 'CONTACT'");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForInitialDecisionAndReferralIsNotInInitialStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage '{mashReferral.Stage}', this request requires the referral to be in stage 'INITIAL'");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForFinalDecisionAndReferralIsNotInFinalStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage '{mashReferral.Stage}', this request requires the referral to be in stage 'FINAL'");
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenWorkerIsNotFoundUsingId()
        {
            var workerId = _faker.Random.Int();
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "ASSIGN-WORKER", workerId: workerId);
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with id {workerId} not found");
        }


        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenWorkerIsNotFoundUsingEmail()
        {
            var workerEmail = _faker.Person.Email;
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "ASSIGN-WORKER", workerEmail: workerEmail);
            request.WorkerId = null;
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email {workerEmail} not found");
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

        [Test]
        public void SuccessfulUpdateOfMashReferralAssignsWorkerAndReturnsMashReferralDomain()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "ASSIGN-WORKER");

            var response = _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            response.Should().BeEquivalentTo(mashReferral.ToDomain());
        }


    }
}
