using System;
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
        public void WhenThereArePersonalRelationshipsOfSameTypeReturnsAllPelationshipsForThatType()
        {
            var (person, otherPersons, personalRelationships, details) = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationshipsOfSameType();

            var response = personalRelationships.ToResponse();

            response.FirstOrDefault().Relationships.Should().HaveCount(2);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.Id == personalRelationships[0].Id);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.PersonId == otherPersons[0].Id);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.FirstName == otherPersons[0].FirstName);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.LastName == otherPersons[0].LastName);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.Gender == otherPersons[0].Gender);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.IsMainCarer == personalRelationships[0].IsMainCarer);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.IsInformalCarer == personalRelationships[0].IsInformalCarer);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.Details == details[0].Details);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.PersonId == otherPersons[1].Id);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.FirstName == otherPersons[1].FirstName);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.LastName == otherPersons[1].LastName);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.Gender == otherPersons[1].Gender);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.IsMainCarer == personalRelationships[1].IsMainCarer);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.IsInformalCarer == personalRelationships[1].IsInformalCarer);
            response.FirstOrDefault().Relationships.Should().Contain(r => r.Details == details[1].Details);
        }

        [Test]
        public void WhenDetailsIsNullDoesNotThrowNullReferenceException()
        {
            var (person, otherPersons, personalRelationships) = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationships();
            person.PersonalRelationships[0].Details = null;
            person.PersonalRelationships[1].Details = null;
            person.PersonalRelationships[2].Details = null;

            Action act = () => personalRelationships.ToResponse();

            act.Should().NotThrow<NullReferenceException>();
        }
    }
}
