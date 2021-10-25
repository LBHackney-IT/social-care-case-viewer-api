using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class InsertDocumentTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private const string CollectionName = "mash-referrals";

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();

            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object);
        }

        [Test]
        public void CallingInsertDocumentWillTellMongoToInsertRecord()
        {
            var referral = new SocialCareCaseViewerApi.V1.Infrastructure.MashReferral
            {
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Sally Samuels" },
                Stage = "Contact",
                ReferralDocumentURI = "hardcoded-referral-1-URI"
            };
            _mongoGateway.Setup(x => x.InsertRecord(CollectionName, referral));

            _mashReferralGateway.InsertDocument(referral);

            _mongoGateway.Verify(x => x.InsertRecord(CollectionName, referral));
        }
    }
}
