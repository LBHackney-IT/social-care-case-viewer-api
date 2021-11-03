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
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class UpdateMashReferralTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IWorkerGateway> _workerGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;
        private DateTime _dateTime;
        private readonly Worker _worker = TestHelpers.CreateWorker().ToDomain(false);
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _workerGateway = new Mock<IWorkerGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _workerGateway.Object, _systemTime.Object);

            _dateTime = DateTime.Now;
            _systemTime.Setup(x => x.Now).Returns(_dateTime);

            _workerGateway.Setup(x => x.GetWorkerByWorkerId(It.IsAny<int>())).Returns(_worker);
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenGetWorkerByWorkerIdReturnsNull()
        {
            var mashReferralId = _faker.Random.String2(20, "0123456789abcdef");
            var request = TestHelpers.CreateUpdateMashReferral();
            _workerGateway.Setup(x => x.GetWorkerByWorkerId(request.WorkerId));

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferralId);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with {request.WorkerId} not found");
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
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "screening-decision");
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());

            act.Should().Throw<MashReferralStageMismatchException>()
                .WithMessage($"Referral {mashReferral.Id} is in stage \"{mashReferral.Stage}\", this request requires the referral to be in stage \"screening\"");
        }

        [Test]
        public void SuccessfulUpdateOfMashReferralFromScreeningToFinalUpsertsUpdateReferralIntoMashReferralGatewayAndReturnsMashReferralResponse()
        {
            var mashReferral = TestHelpers.CreateMashReferral(stage: "screening");
            var request = TestHelpers.CreateUpdateMashReferral(updateType: "screening-decision");
            _mashReferralGateway.Setup(x => x.GetInfrastructureUsingId(mashReferral.Id.ToString())).Returns(mashReferral);

            var response = _mashReferralUseCase.UpdateMashReferral(request, mashReferral.Id.ToString());

            _mashReferralGateway.Verify(x => x.UpsertRecord(mashReferral), Times.Once);

            response.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral()
            {
                Id = mashReferral.Id.ToString(),
                Clients = mashReferral.Clients,
                Referrer = mashReferral.Referrer,
                Stage = "Final",
                AssignedTo = mashReferral.AssignedTo?.ToDomain(true).ToResponse(),
                CreatedAt = mashReferral.CreatedAt.ToString("O"),
                FinalDecision = mashReferral.FinalDecision,
                InitialDecision = mashReferral.InitialDecision,
                ScreeningDecision = mashReferral.ScreeningDecision,
                ScreeningUrgentContactRequired = mashReferral.ScreeningUrgentContactRequired,
                ScreeningCreatedAt = _dateTime.ToString("O"),
                ReferralCategory = mashReferral.ReferralCategory,
                RequestedSupport = mashReferral.RequestedSupport,
                ReferralDocumentURI = mashReferral.ReferralDocumentURI
            });
        }
    }
}
