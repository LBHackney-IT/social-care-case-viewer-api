using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class ResetTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();

            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object);
        }

        [Test]
        public void CallingResetWillDropTheMashReferralCollection()
        {
            _mashReferralGateway.Setup(x => x.Reset());

            _mashReferralUseCase.Reset();

            _mashReferralGateway.Verify(x => x.Reset());
        }

        [Test]
        public void CallingResetWillInsertHardCodedMashReferralsIntoMongoDb()
        {
            var referral1 = new SocialCareCaseViewerApi.V1.Infrastructure.MashReferral
            {
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Sally Samuels" },
                Stage = "Contact",
                ReferralDocumentURI = "hardcoded-referral-1-URI"
            };
            _mashReferralGateway.Setup(x => x.InsertDocument(referral1));

            _mashReferralUseCase.Reset();

            _mashReferralGateway.Verify(x => x.InsertDocument(It.IsAny<SocialCareCaseViewerApi.V1.Infrastructure.MashReferral>()));
        }
    }
}
