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
 

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral

{
    public class GetMashReferralsUsingQueryTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
         private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new MashReferralGateway();

        }

        [Test]
        public void GetMashReferralsReturnsAListOfMashReferralResponse()
        {
            var query = TestHelpers.CreateQueryMashReferral("1");
            var gatewayResponse = TestHelpers.CreateMashReferral2().ToDomain();

          
            var response = _mashReferralGateway.GetReferralsUsingQuery(query);

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
