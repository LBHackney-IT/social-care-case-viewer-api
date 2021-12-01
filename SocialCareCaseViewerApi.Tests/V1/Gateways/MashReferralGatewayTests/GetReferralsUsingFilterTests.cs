using System;
using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class GetReferralsUsingFilterTests : DatabaseTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private const string CollectionName = "mash-referrals";

        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, _databaseGateway.Object, _systemTime.Object, DatabaseContext);
        }

        [Test]
        public void GetReferralUsingFilterReturnsListOfDomainMashReferral()
        {
            var filter = MashReferralUseCase.GenerateFilter(new QueryMashReferrals());
            var referral = new MashReferral
            {
                Id = ObjectId.Parse(_faker.Random.String2(24, "0123456789abcdef")),
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Sally Samuels" },
                Stage = "Contact",
                ReferralDocumentURI = "hardcoded-referral-1-URI"
            };
            _mongoGateway
                .Setup(x => x.LoadMashReferralsByFilter(CollectionName, filter))
                .Returns(new List<MashReferral> { referral });

            var response = _mashReferralGateway.GetReferralsUsingFilter(filter);

            response.Should().BeEquivalentTo(referral.ToDomain());
        }

        [Test]
        public void GetReferralUsingFilterReturnsEmptyListWhenNoMashReferralsFound()
        {
            var filter = MashReferralUseCase.GenerateFilter(new QueryMashReferrals());
            _mongoGateway
                .Setup(x => x.LoadMashReferralsByFilter(CollectionName, filter))
                .Returns(new List<MashReferral>());

            var response = _mashReferralGateway.GetReferralsUsingFilter(filter);

            response.Should().BeEquivalentTo(new List<SocialCareCaseViewerApi.V1.Domain.MashReferral>());
        }
    }
}
