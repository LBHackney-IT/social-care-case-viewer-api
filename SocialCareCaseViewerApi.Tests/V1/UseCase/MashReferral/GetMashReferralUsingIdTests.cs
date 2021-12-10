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
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class GetMashReferralUsingIdTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<IWorkerGateway> _workerGateway = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;
        private readonly SocialCareCaseViewerApi.V1.Infrastructure.Worker _worker = TestHelpers.CreateWorker();

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
        public void GettingAMashReferralReturnsAMashReferralResponseObject()
        {
            var referral = TestHelpers.CreateMashReferral().ToDomain();
            _mashReferralGateway
                .Setup(x => x.GetReferralUsingId(referral.Id))
                .Returns(referral);

            var response = _mashReferralUseCase.GetMashReferralUsingId(referral.Id);

            response.Should().BeEquivalentTo(referral.ToResponse(), options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                return options;
            });
        }

        [Test]
        public void NotGettingAMashReferralReturnsNull()
        {
            const long nonExistentId = 123L;
            _mashReferralGateway
                .Setup(x => x.GetReferralUsingId(nonExistentId));

            var response = _mashReferralUseCase.GetMashReferralUsingId(nonExistentId);

            response.Should().BeNull();
        }
    }
}
