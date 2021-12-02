using System;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
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
        private IMashReferralUseCase _mashReferralUseCase = null!;
        private readonly SocialCareCaseViewerApi.V1.Infrastructure.Worker _worker = TestHelpers.CreateWorker();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object);

            _databaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>())).Returns(_worker);
        }

        [Test]
        public void UpdatingMashReferralThrowsWorkerNotFoundExceptionWhenGetWorkerByWorkerIdReturnsNull()
        {
            var mashReferralId = _faker.Random.Long();
            var request = TestHelpers.CreateUpdateMashReferral();
            _databaseGateway.Setup(x => x.GetWorkerByEmail(request.WorkerEmail));

            Action act = () => _mashReferralUseCase.UpdateMashReferral(request, mashReferralId);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email \"{request.WorkerEmail}\" not found");
        }
    }
}
