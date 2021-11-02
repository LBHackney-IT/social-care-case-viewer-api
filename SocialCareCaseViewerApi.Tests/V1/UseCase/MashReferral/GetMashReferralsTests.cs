using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    public class GetMashReferralsTests
    {

        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IWorkerGateway> _workerGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _workerGateway = new Mock<IWorkerGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _workerGateway.Object, _systemTime.Object);
        }

        [Test]
        public void GetMashReferralsReturnsAListOfMashReferralResponse()
        {
            var query = TestHelpers.CreateQueryMashReferral();
            var gatewayResponse = TestHelpers.CreateMashReferral().ToDomain();

            _mashReferralGateway
                .Setup(x => x.GetReferralsUsingFilter(It.IsAny<FilterDefinition<SocialCareCaseViewerApi.V1.Infrastructure.MashReferral>>()))
                .Returns(new List<SocialCareCaseViewerApi.V1.Domain.MashReferral> { gatewayResponse });

            var response = _mashReferralUseCase.GetMashReferrals(query);

            response.ToList()[0].Should().BeEquivalentTo(gatewayResponse.ToResponse());
        }

        [Test]
        public void FilterWillQueryForEverythingIfNoValuesPassed()
        {
            var filter = MashReferralUseCase.GenerateFilter(new QueryMashReferrals());
            const string expectedJson = "{ }";

            filter.RenderToJson().Should().Be(expectedJson);
        }

        [Test]
        public void FilterWillQueryReferralId()
        {
            var mashReferralId = _faker.Random.String2(24, "0123456789abcdef");
            var query = TestHelpers.CreateQueryMashReferral(mashReferralId);
            var expectedJson = "{ \"_id\" : ObjectId(\"" + mashReferralId + "\") }";

            var filter = MashReferralUseCase.GenerateFilter(query);

            filter.RenderToJson().Should().Be(expectedJson);
        }
    }
}
