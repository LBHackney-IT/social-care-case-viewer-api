using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class InsertDocumentTests : DatabaseTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private const string CollectionName = "mash-referrals";

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, _systemTime.Object, DatabaseContext);
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
