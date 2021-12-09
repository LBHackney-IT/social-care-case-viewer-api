using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests

{
    public class CreateNewMashReferralTests : DatabaseTests
    {
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;

        [SetUp]
        public void Setup()
        {
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_systemTime.Object, DatabaseContext);

            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void CreateNewMashReferral()
        {
            var dateTime = DateTime.Now;
            _systemTime.Setup(x => x.Now).Returns(dateTime);    
            var request = TestHelpers.CreateNewMashReferralRequest(); 

            var referral=_mashReferralGateway.CreateReferral(request);

            referral.Referrer.Should().BeEquivalentTo(request.Referrer);
            referral.RequestedSupport.Should().BeEquivalentTo(request.RequestedSupport);
            referral.ReferralDocumentURI.Should().BeEquivalentTo(request.ReferralUri);
            referral.Stage.Should().BeEquivalentTo("CONTACT");
            referral.ReferralCreatedAt.Should().Be(dateTime);
            referral.MashResidents.Count.Should().Be(request.MashResidents.Count);

            for(var i =0; i <request.MashResidents.Count; i++){
                var firstResident = referral.MashResidents[i];
                var requestFirstResident = request.MashResidents[i];
                firstResident.FirstName.Should().BeEquivalentTo(requestFirstResident.FirstName);
                firstResident.LastName.Should().BeEquivalentTo(requestFirstResident.LastName);
                firstResident.Address.Should().BeEquivalentTo(requestFirstResident.Address);
                firstResident.Ethnicity.Should().BeEquivalentTo(requestFirstResident.Ethnicity);
                firstResident.Gender.Should().BeEquivalentTo(requestFirstResident.Gender);
                firstResident.Postcode.Should().BeEquivalentTo(requestFirstResident.Postcode);
                firstResident.School.Should().BeEquivalentTo(requestFirstResident.School);
                firstResident.FirstLanguage.Should().BeEquivalentTo(requestFirstResident.FirstLanguage);
                firstResident.DateOfBirth.Should().Be(requestFirstResident.DateOfBirth);
            }    


        }
    }
}
