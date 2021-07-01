using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Factories;

namespace SocialCareCaseViewerApi.Tests.V1.Factories.Response
{
    public class ResponseFactoryListRelationshipsResponseTests
    {
        [Test]
        public void WhenNoPersonalRelationshipsReturnsEmptyList()
        {
            var personalRelationships = new List<PersonalRelationship>();

            var response = personalRelationships.ToResponse();

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenThereArePersonalRelationshipsReturnsAnObjectForEachExistingType()
        {
            var (person, _, personalRelationships) = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationships();

            var response = personalRelationships.ToResponse();

            response.Should().HaveCount(3);
            response.Should().Contain(pr => pr.Type == "parent");
            response.Should().Contain(pr => pr.Type == "child");
            response.Should().Contain(pr => pr.Type == "neighbour");
        }

        [Test]
        public void WhenThereArePersonalRelationshipsOfSameTypeReturnsAllPersonsForThatType()
        {
            var (person, otherPersons, personalRelationships) = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationshipsOfSameType();

            var response = personalRelationships.ToResponse();

            response.FirstOrDefault().Persons.Should().HaveCount(2);
            response.FirstOrDefault().Persons.Should().Contain(p => p.Id == otherPersons[0].Id);
            response.FirstOrDefault().Persons.Should().Contain(p => p.FirstName == otherPersons[0].FirstName);
            response.FirstOrDefault().Persons.Should().Contain(p => p.LastName == otherPersons[0].LastName);
            response.FirstOrDefault().Persons.Should().Contain(p => p.Gender == otherPersons[0].Gender);
            response.FirstOrDefault().Persons.Should().Contain(p => p.Id == otherPersons[1].Id);
            response.FirstOrDefault().Persons.Should().Contain(p => p.FirstName == otherPersons[1].FirstName);
            response.FirstOrDefault().Persons.Should().Contain(p => p.LastName == otherPersons[1].LastName);
            response.FirstOrDefault().Persons.Should().Contain(p => p.Gender == otherPersons[1].Gender);
        }
    }
}