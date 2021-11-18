using System;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class UpdateMashReferralTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;
        private DateTime _dateTime;
        private readonly SocialCareCaseViewerApi.V1.Infrastructure.Worker _worker = TestHelpers.CreateWorker();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object, _systemTime.Object);

            _dateTime = DateTime.Now;
            _systemTime.Setup(x => x.Now).Returns(_dateTime);

            _databaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>())).Returns(_worker);
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenGetWorkerByWorkerIdReturnsNull()
        {
            var mashReferralId = _faker.Random.String2(20, "0123456789abcdef");
            var request = TestHelpers.CreateUpdateMashReferral();
            _databaseGateway.Setup(x => x.GetWorkerByEmail(request.WorkerEmail));

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferralId);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email \"{request.WorkerEmail}\" not found");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralNotFoundExceptionWhenGetInfrastructureUsingIdReturnsNull()
        {
            var mashReferralId = _faker.Random.String2(20, "0123456789abcdef");
            var request = TestHelpers.CreateUpdateMashReferral();
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferralId));

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferralId);

            act.Should().Throw<MashReferralNotFoundException>()
                .WithMessage($"MASH referral with id {mashReferralId} not found");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForScreeningDecisionAndReferralIsNotInScreeningStage()
        {
            var mashReferral = TestHelpers.CreateMashReferral(stage: "not-screening");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"screening\"");
        }

        [Test]
        public void UpdatingMashReferralThrowsMashReferralStageMismatchExceptionWhenRequestUpdateIsForInitialDecisionAndReferralIsNotInInitialStage()
        {
            var mashReferral = TestHelpers.CreateMashReferral(stage: "not initial");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"initial decision\"");
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromScreeningToFinalUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        {
            var mashReferral = TestHelpers.CreateMashReferral(stage: "SCREENING");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "SCREENING-DECISION");
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);

            var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());

            _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);

            response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
            {
                Id = mashReferral.Id.ToString(),
                Clients = mashReferral.Clients,
                Referrer = mashReferral.Referrer,
                Stage = "FINAL",
                AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
                CreatedAt = mashReferral.CreatedAt.ToString("O"),
                FinalDecision = mashReferral.FinalDecision,
                InitialDecision = mashReferral.InitialDecision,
                InitialUrgentContactRequired = mashReferral.InitialUrgentContactRequired,
                InitialCreatedAt = mashReferral.InitialCreatedAt?.ToString("O"),
                ScreeningDecision = mashReferral.ScreeningDecision,
                ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
                ScreeningCreatedAt = _dateTime.ToString("O"),
                ReferralCategory = mashReferral.ReferralCategory,
                RequestedSupport = mashReferral.RequestedSupport,
                ReferralDocumentURI = mashReferral.ReferralDocumentURI
            });
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromInitialToScreeningUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        {
            var mashReferral = TestHelpers.CreateMashReferral(stage: "INITIAL");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "INITIAL-DECISION");
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);

            var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());

            _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);

            response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
            {
                Id = mashReferral.Id.ToString(),
                Clients = mashReferral.Clients,
                Referrer = mashReferral.Referrer,
                Stage = "SCREENING",
                AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
                CreatedAt = mashReferral.CreatedAt.ToString("O"),
                FinalDecision = mashReferral.FinalDecision,
                InitialDecision = mashReferral.InitialDecision,
                InitialUrgentContactRequired = mashReferral.InitialUrgentContactRequired,
                InitialCreatedAt = _dateTime.ToString("O"),
                ScreeningDecision = mashReferral.ScreeningDecision,
                ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
                ScreeningCreatedAt = mashReferral.ScreeningCreatedAt?.ToString("O"),
                ReferralCategory = mashReferral.ReferralCategory,
                RequestedSupport = mashReferral.RequestedSupport,
                ReferralDocumentURI = mashReferral.ReferralDocumentURI
            });
        }
    }
}
