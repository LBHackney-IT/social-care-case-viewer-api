using System;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
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
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;

        private readonly Faker _faker = new Faker();

        private Worker _worker = null!;

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, _databaseGateway.Object, _systemTime.Object, DatabaseContext);

            _worker = TestHelpers.CreateWorker();
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenGetWorkerByWorkerIdReturnsNull()
        {
            var mashReferralId = _faker.Random.Long(100);
            var request = TestHelpers.CreateUpdateMashReferral();
            _databaseGateway.Setup(x => x.GetWorkerByEmail(request.WorkerEmail));

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferralId);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email \"{request.WorkerEmail}\" not found");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralNotFoundExceptionWhenGetInfrastructureUsingIdReturnsNull()
        {
            var mashReferralId = _faker.Random.Long(100);
            var request = TestHelpers.CreateUpdateMashReferral();

            _databaseGateway
                .Setup(x => x.GetWorkerByEmail(request.WorkerEmail))
                .Returns(_worker);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferralId);

            act.Should().Throw<MashReferralNotFoundException>()
                .WithMessage($"MASH referral with id {mashReferralId} not found");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForScreeningDecisionAndReferralIsNotInScreeningStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            _databaseGateway
                .Setup(x => x.GetWorkerByEmail(request.WorkerEmail))
                .Returns(_worker);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"screening\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForContactDecisionAndReferralIsNotInContactStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "CONTACT-DECISION");
            _databaseGateway
                .Setup(x => x.GetWorkerByEmail(request.WorkerEmail))
                .Returns(_worker);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"contact\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForInitialDecisionAndReferralIsNotInInitialStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            _databaseGateway
                .Setup(x => x.GetWorkerByEmail(request.WorkerEmail))
                .Returns(_worker);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"initial\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForFinalDecisionAndReferralIsNotInFinalStage()
        {
            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
            _databaseGateway
                .Setup(x => x.GetWorkerByEmail(request.WorkerEmail))
                .Returns(_worker);

            Action act = () => _mashReferralGateway.UpdateReferral(request, mashReferral.Id);

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"final\"");
        }
        //
        // [Test]
        // public void SuccessfulUpdateOfMashReferralFromScreeningToFinalUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        // {
        //     var mashReferral = TestHelpers.CreateMashReferral(stage: "SCREENING");
        //     var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
        //     _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);
        //
        //     var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());
        //
        //     _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);
        //
        //     response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
        //     {
        //         Id = mashReferral.Id.ToString(),
        //         Clients = mashReferral.Clients,
        //         Referrer = mashReferral.Referrer,
        //         Stage = "FINAL",
        //         AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
        //         CreatedAt = mashReferral.CreatedAt.ToString("O"),
        //         ContactUrgentContactRequired = mashReferral.ContactUrgentContactRequired,
        //         ContactCreatedAt = mashReferral.ContactCreatedAt?.ToString("O"),
        //         InitialDecision = mashReferral.InitialDecision,
        //         InitialUrgentContactRequired = mashReferral.InitialUrgentContactRequired,
        //         InitialReferralCategory = mashReferral.InitialReferralCategory,
        //         InitialCreatedAt = mashReferral.InitialCreatedAt?.ToString("O"),
        //         ScreeningDecision = mashReferral.ScreeningDecision,
        //         ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
        //         ScreeningCreatedAt = _dateTime.ToString("O"),
        //         FinalDecision = mashReferral.FinalDecision,
        //         FinalReferralCategory = mashReferral.FinalReferralCategory,
        //         FinalUrgentContactRequired = mashReferral.FinalUrgentContactRequired,
        //         FinalCreatedAt = mashReferral.FinalCreatedAt?.ToString("O"),
        //         RequestedSupport = mashReferral.RequestedSupport,
        //         ReferralDocumentURI = mashReferral.ReferralDocumentURI
        //     });
        // }
        //
        // [Test]
        // public void SuccessfulUpdateOfMashReferralFromInitialToScreeningUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        // {
        //     var mashReferral = TestHelpers.CreateMashReferral(stage: "INITIAL");
        //     var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
        //     _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);
        //
        //     var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());
        //
        //     _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);
        //
        //     response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
        //     {
        //         Id = mashReferral.Id.ToString(),
        //         Clients = mashReferral.Clients,
        //         Referrer = mashReferral.Referrer,
        //         Stage = "SCREENING",
        //         AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
        //         CreatedAt = mashReferral.CreatedAt.ToString("O"),
        //         ContactUrgentContactRequired = mashReferral.ContactUrgentContactRequired,
        //         ContactCreatedAt = mashReferral.ContactCreatedAt?.ToString("O"),
        //         InitialDecision = mashReferral.InitialDecision,
        //         InitialUrgentContactRequired = mashReferral.InitialUrgentContactRequired,
        //         InitialReferralCategory = mashReferral.InitialReferralCategory,
        //         InitialCreatedAt = _dateTime.ToString("O"),
        //         ScreeningDecision = mashReferral.ScreeningDecision,
        //         ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
        //         ScreeningCreatedAt = mashReferral.ScreeningCreatedAt?.ToString("O"),
        //         FinalDecision = mashReferral.FinalDecision,
        //         FinalReferralCategory = mashReferral.FinalReferralCategory,
        //         FinalUrgentContactRequired = mashReferral.FinalUrgentContactRequired,
        //         FinalCreatedAt = mashReferral.FinalCreatedAt?.ToString("O"),
        //         RequestedSupport = mashReferral.RequestedSupport,
        //         ReferralDocumentURI = mashReferral.ReferralDocumentURI
        //     });
        // }
        //
        // [Test]
        // public void SuccessfulUpdateOfMashReferralFromFinalToPostFinalUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        // {
        //     var mashReferral = TestHelpers.CreateMashReferral(stage: "FINAL");
        //     var request = TestHelpers.CreateUpdateMashReferral(updateType: "FINAL-DECISION");
        //     _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);
        //
        //     var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());
        //
        //     _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);
        //
        //     response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
        //     {
        //         Id = mashReferral.Id.ToString(),
        //         Clients = mashReferral.Clients,
        //         Referrer = mashReferral.Referrer,
        //         Stage = "POST-FINAL",
        //         AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
        //         CreatedAt = mashReferral.CreatedAt.ToString("O"),
        //         ContactUrgentContactRequired = mashReferral.ContactUrgentContactRequired,
        //         ContactCreatedAt = mashReferral.ContactCreatedAt?.ToString("O"),
        //         InitialDecision = mashReferral.InitialDecision,
        //         InitialUrgentContactRequired = mashReferral.InitialUrgentContactRequired,
        //         InitialReferralCategory = mashReferral.InitialReferralCategory,
        //         InitialCreatedAt = mashReferral.InitialCreatedAt?.ToString("O"),
        //         ScreeningDecision = mashReferral.ScreeningDecision,
        //         ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
        //         ScreeningCreatedAt = mashReferral.ScreeningCreatedAt?.ToString("O"),
        //         FinalDecision = mashReferral.FinalDecision,
        //         FinalReferralCategory = mashReferral.FinalReferralCategory,
        //         FinalUrgentContactRequired = mashReferral.FinalUrgentContactRequired,
        //         FinalCreatedAt = _dateTime.ToString("O"),
        //         RequestedSupport = mashReferral.RequestedSupport,
        //         ReferralDocumentURI = mashReferral.ReferralDocumentURI
        //     });
        // }
        //
        // [Test]
        // public void SuccessfulUpdateOfMashReferralFromContactToInitialUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        // {
        //     var mashReferral = TestHelpers.CreateMashReferral(stage: "CONTACT");
        //     var request = TestHelpers.CreateUpdateMashReferral(updateType: "CONTACT-DECISION");
        //     _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);
        //
        //     var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());
        //
        //     _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);
        //
        //     response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
        //     {
        //         Id = mashReferral.Id.ToString(),
        //         Clients = mashReferral.Clients,
        //         Referrer = mashReferral.Referrer,
        //         Stage = "INITIAL",
        //         AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
        //         CreatedAt = mashReferral.CreatedAt.ToString("O"),
        //         ContactUrgentContactRequired = mashReferral.ContactUrgentContactRequired,
        //         ContactCreatedAt = _dateTime.ToString("O"),
        //         InitialDecision = mashReferral.InitialDecision,
        //         InitialUrgentContactRequired = mashReferral.InitialUrgentContactRequired,
        //         InitialReferralCategory = mashReferral.InitialReferralCategory,
        //         InitialCreatedAt = mashReferral.InitialCreatedAt?.ToString("O"),
        //         ScreeningDecision = mashReferral.ScreeningDecision,
        //         ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
        //         ScreeningCreatedAt = mashReferral.ScreeningCreatedAt?.ToString("O"),
        //         FinalDecision = mashReferral.FinalDecision,
        //         FinalReferralCategory = mashReferral.FinalReferralCategory,
        //         FinalUrgentContactRequired = mashReferral.FinalUrgentContactRequired,
        //         FinalCreatedAt = mashReferral.FinalCreatedAt?.ToString("O"),
        //         RequestedSupport = mashReferral.RequestedSupport,
        //         ReferralDocumentURI = mashReferral.ReferralDocumentURI
        //     });
        // }

    }
}
