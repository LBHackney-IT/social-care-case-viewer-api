using System;
using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class GetReferralUsingIdTests : DatabaseTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private readonly Faker _faker = new Faker();
        private const string CollectionName = "mash-referrals";

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, DatabaseContext);
        }

        [Test]
        public void GetReferralUsingIdReturnsDomainMashReferral()
        {
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
                .Setup(x => x.LoadRecordById<MashReferral>(CollectionName, referral.Id))
                .Returns(referral);

            var response = _mashReferralGateway.GetReferralUsingId(referral.Id.ToString());

            response.Should().BeEquivalentTo(referral.ToDomain());
        }

        [Test]
        public void GetReferralUsingIdReturnsNullIfNoMashReferralFound()
        {
            var nonExistentId = _faker.Random.String2(24, "0123456789abcdef");

            _mongoGateway.Setup(x => x.LoadRecordById<MashReferral>(CollectionName, ObjectId.Parse(nonExistentId)));

            var response = _mashReferralGateway.GetReferralUsingId(nonExistentId);

            response.Should().BeNull();
        }
    }
}
