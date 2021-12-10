using System.Collections.Generic;
using System.Linq;
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
    public class GetMashReferralsTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<IWorkerGateway> _workerGateway = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _workerGateway = new Mock<IWorkerGateway>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object, _workerGateway.Object);
        }

        [Test]
        public void GetMashReferralsReturnsAListOfMashReferralResponse()
        {
            var query = TestHelpers.CreateQueryMashReferral();
            var gatewayResponse = TestHelpers.CreateMashReferral().ToDomain();

            _mashReferralGateway
                .Setup(x => x.GetReferralsUsingQuery(query))
                .Returns(new List<SocialCareCaseViewerApi.V1.Domain.MashReferral> { gatewayResponse });

            var response = _mashReferralUseCase.GetMashReferrals(query);

            response.ToList()[0].Should().BeEquivalentTo(gatewayResponse.ToResponse());
        }
    }
}
