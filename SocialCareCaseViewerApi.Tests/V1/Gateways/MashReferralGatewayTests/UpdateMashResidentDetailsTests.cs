using System;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class UpdateMashResidentDetailsTests : DatabaseTests
    {
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void Setup()
        {
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_systemTime.Object, DatabaseContext);
        }

        [Test]
        public void WhenMashResidentAndMatchingPersonExistUpdatesSocialCareIdValueOnMashResident()
        {
            var personMatch = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(personMatch);
            DatabaseContext.SaveChanges();

            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
            var mashResident = MashResidentHelper.SaveMashResidentToDatabase(DatabaseContext, mashReferral.Id, null);

            mashResident.SocialCareId.Should().BeNull();

            var request = TestHelpers.CreateMashResidentUpdateRequest(personMatch.Id);

            var response = _mashReferralGateway.UpdateMashResident(request, mashResident.Id);
            response.Should().BeEquivalentTo(mashResident.ToDomain());
            response.SocialCareId.Should().Be(personMatch.Id);
        }

        [Test]
        public void WhenMashResidentWithLinkedPersonExistAndUpdateTypeIsUnLinkPersonUpdatesSocialCareIdValueOnMashResident()
        {
            var personMatch = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(personMatch);
            DatabaseContext.SaveChanges();

            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
            var mashResident = MashResidentHelper.SaveMashResidentToDatabase(DatabaseContext, mashReferral.Id, personMatch.Id);

            mashResident.SocialCareId.Should().NotBeNull();

            var request = TestHelpers.CreateMashResidentUpdateRequest(personMatch.Id, updateType: "UNLINK-PERSON");

            var response = _mashReferralGateway.UpdateMashResident(request, mashResident.Id);
            response.Should().BeEquivalentTo(mashResident.ToDomain());
            response.SocialCareId.Should().BeNull();
        }

        [Test]
        public void WhenMashResidentWithTheIdProvidedDoesNotExistThrowsNotFoundException()
        {
            var personId = _faker.Random.Int();
            var nonExistingMashResidentId = _faker.Random.Int();

            var request = TestHelpers.CreateMashResidentUpdateRequest(personId);

            Action act = () => _mashReferralGateway.UpdateMashResident(request, nonExistingMashResidentId);

            act.Should().Throw<MashResidentNotFoundException>()
                .WithMessage($"MASH resident with id {nonExistingMashResidentId} not found");
        }

        [Test]
        public void IfPersonWithTheIdProvidedDoesNotExistThrowsNotFoundException()
        {
            var nonExistingPersonId = _faker.Random.Int();

            var mashReferral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
            var mashResident = MashResidentHelper.SaveMashResidentToDatabase(DatabaseContext, mashReferral.Id);

            var request = TestHelpers.CreateMashResidentUpdateRequest(nonExistingPersonId);

            Action act = () => _mashReferralGateway.UpdateMashResident(request, mashResident.Id);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"Person with id {nonExistingPersonId} not found");
        }
    }
}
