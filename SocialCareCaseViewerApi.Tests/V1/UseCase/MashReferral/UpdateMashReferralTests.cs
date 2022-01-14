using System;
using Bogus;
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
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class UpdateMashReferralTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<IWorkerGateway> _workerGateway = null!;

        private IMashReferralUseCase _mashReferralUseCase = null!;
        private readonly SocialCareCaseViewerApi.V1.Infrastructure.Worker _worker = TestHelpers.CreateWorker();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _workerGateway = new Mock<IWorkerGateway>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object, _workerGateway.Object);

            _databaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>())).Returns(_worker);
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenGetWorkerByWorkerEmailReturnsNull()
        {
            var mashReferralId = _faker.Random.Long();
            var request = TestHelpers.CreateUpdateMashReferral();
            request.WorkerId = null;
            _databaseGateway.Setup(x => x.GetWorkerByEmail(request.WorkerEmail));

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferralId);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email \"{request.WorkerEmail}\" not found");
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenGetWorkerByWorkerIdReturnsNull()
        {
            var mashReferralId = _faker.Random.Long();
            var request = TestHelpers.CreateUpdateMashReferral();
            _workerGateway.Setup(x => x.GetWorkerByWorkerId(request.WorkerId!.Value));

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferralId);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email \"{request.WorkerEmail}\" not found");
        }

        [Test]
        public void UpdatingMashReferralUnAssignOnSuccessReturnsMashReferralResponse()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            request.UpdateType = "UNASSIGN-WORKER";

            var referral = TestHelpers.CreateMashReferral().ToDomain();

            _mashReferralGateway
                .Setup(x => x.UpdateReferral(request, referral.Id))
                .Returns(referral);

            var response = _mashReferralUseCase.UpdateMashReferral(request, referral.Id);

            response.Should().BeEquivalentTo(referral.ToResponse(), options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                return options;
            });
        }

        [Test]
        public void UpdatingMashReferralOnSuccessReturnsMashReferralResponse()
        {
            var worker = TestHelpers.CreateWorker();
            var request = TestHelpers.CreateUpdateMashReferral();
            request.WorkerId = null;

            var referral = TestHelpers.CreateMashReferral().ToDomain();

            _databaseGateway
                .Setup(x => x.GetWorkerByEmail(request.WorkerEmail))
                .Returns(worker);

            _mashReferralGateway
                .Setup(x => x.UpdateReferral(request, referral.Id))
                .Returns(referral);

            var response = _mashReferralUseCase.UpdateMashReferral(request, referral.Id);

            response.Should().BeEquivalentTo(referral.ToResponse(), options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                return options;
            });
        }
    }
}
