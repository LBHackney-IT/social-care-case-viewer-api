using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;

namespace SocialCareCaseViewerApi.Tests.V1.Factories.Response
{
    public class ResponseFactoryListRelationshipsResponseTests
    {
        [Test]
        public void WhenAllPersonalRelationshipTypesReturnsRelationshipsForEachType()
        {
            var personalRelationships = PersonalRelationshipsHelper.CreatePersonalRelationshipsWithAllTypes();

            var response = personalRelationships.ToResponse();

            response.Acquaintance.Should().HaveCount(1);
            response.AuntUncle.Should().HaveCount(1);
            response.Child.Should().HaveCount(1);
            response.Cousin.Should().HaveCount(1);
            response.ExPartner.Should().HaveCount(1);
            response.ExSpouse.Should().HaveCount(1);
            response.FosterCarer.Should().HaveCount(1);
            response.FosterCarerSupportCarer.Should().HaveCount(1);
            response.FosterChild.Should().HaveCount(1);
            response.Friend.Should().HaveCount(1);
            response.Grandchild.Should().HaveCount(1);
            response.Grandparent.Should().HaveCount(1);
            response.GreatGrandchild.Should().HaveCount(1);
            response.GreatGrandparent.Should().HaveCount(1);
            response.HalfSibling.Should().HaveCount(1);
            response.InContactWith.Should().HaveCount(1);
            response.Neighbour.Should().HaveCount(1);
            response.NieceNephew.Should().HaveCount(1);
            response.Other.Should().HaveCount(1);
            response.Parent.Should().HaveCount(1);
            response.ParentOfUnbornChild.Should().HaveCount(1);
            response.Partner.Should().HaveCount(1);
            response.PrivateFosterCarer.Should().HaveCount(1);
            response.PrivateFosterChild.Should().HaveCount(1);
            response.Sibling.Should().HaveCount(1);
            response.SiblingOfUnbornChild.Should().HaveCount(1);
            response.Spouse.Should().HaveCount(1);
            response.StepChild.Should().HaveCount(1);
            response.StepParent.Should().HaveCount(1);
            response.StepSibling.Should().HaveCount(1);
            response.SupportCarerFosterCarer.Should().HaveCount(1);
            response.UnbornChild.Should().HaveCount(1);
            response.UnbornSibling.Should().HaveCount(1);
        }

        [Test]
        public void WhenSomePersonalRelationshipTypesReturnsRelationshipsForEachExisitingType()
        {
            var (personalRelationships, _) = PersonalRelationshipsHelper.CreatePersonalRelationshipsWithSomeTypes();

            var response = personalRelationships.ToResponse();

            response.Acquaintance.Should().HaveCount(1);
            response.Child.Should().HaveCount(1);
            response.Grandparent.Should().HaveCount(1);
            response.Neighbour.Should().HaveCount(1);
            response.Parent.Should().HaveCount(1);

            response.AuntUncle.Should().HaveCount(0);
            response.Cousin.Should().HaveCount(0);
            response.ExPartner.Should().HaveCount(0);
            response.ExSpouse.Should().HaveCount(0);
            response.FosterCarer.Should().HaveCount(0);
            response.FosterCarerSupportCarer.Should().HaveCount(0);
            response.FosterChild.Should().HaveCount(0);
            response.Friend.Should().HaveCount(0);
            response.Grandchild.Should().HaveCount(0);
            response.GreatGrandchild.Should().HaveCount(0);
            response.GreatGrandparent.Should().HaveCount(0);
            response.HalfSibling.Should().HaveCount(0);
            response.InContactWith.Should().HaveCount(0);
            response.NieceNephew.Should().HaveCount(0);
            response.Other.Should().HaveCount(0);
            response.ParentOfUnbornChild.Should().HaveCount(0);
            response.Partner.Should().HaveCount(0);
            response.PrivateFosterCarer.Should().HaveCount(0);
            response.PrivateFosterChild.Should().HaveCount(0);
            response.Sibling.Should().HaveCount(0);
            response.SiblingOfUnbornChild.Should().HaveCount(0);
            response.Spouse.Should().HaveCount(0);
            response.StepChild.Should().HaveCount(0);
            response.StepParent.Should().HaveCount(0);
            response.StepSibling.Should().HaveCount(0);
            response.SupportCarerFosterCarer.Should().HaveCount(0);
            response.UnbornChild.Should().HaveCount(0);
            response.UnbornSibling.Should().HaveCount(0);
        }

        [Test]
        public void ReturnsRelatedPersonForEachPersonalRelationshipInType()
        {
            var (personalRelationships, otherPersons) = PersonalRelationshipsHelper.CreatePersonalRelationshipsWithSomeTypes();

            var response = personalRelationships.ToResponse();

            response.Acquaintance.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            response.Acquaintance.FirstOrDefault().Id.Should().Equals(otherPersons[0].Id);
            response.Acquaintance.FirstOrDefault().FirstName.Should().Equals(otherPersons[0].FirstName);
            response.Acquaintance.FirstOrDefault().LastName.Should().Equals(otherPersons[0].LastName);
            response.Acquaintance.FirstOrDefault().Gender.Should().Equals(otherPersons[0].Gender);

            response.Child.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            response.Child.FirstOrDefault().Id.Should().Be(otherPersons[1].Id);
            response.Child.FirstOrDefault().FirstName.Should().Be(otherPersons[1].FirstName);
            response.Child.FirstOrDefault().LastName.Should().Be(otherPersons[1].LastName);
            response.Child.FirstOrDefault().Gender.Should().Be(otherPersons[1].Gender);

            response.Grandparent.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            response.Grandparent.FirstOrDefault().Id.Should().Be(otherPersons[2].Id);
            response.Grandparent.FirstOrDefault().FirstName.Should().Be(otherPersons[2].FirstName);
            response.Grandparent.FirstOrDefault().LastName.Should().Be(otherPersons[2].LastName);
            response.Grandparent.FirstOrDefault().Gender.Should().Be(otherPersons[2].Gender);

            response.Neighbour.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            response.Neighbour.FirstOrDefault().Id.Should().Be(otherPersons[3].Id);
            response.Neighbour.FirstOrDefault().FirstName.Should().Be(otherPersons[3].FirstName);
            response.Neighbour.FirstOrDefault().LastName.Should().Be(otherPersons[3].LastName);
            response.Neighbour.FirstOrDefault().Gender.Should().Be(otherPersons[3].Gender);

            response.Parent.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            response.Parent.FirstOrDefault().Id.Should().Be(otherPersons[4].Id);
            response.Parent.FirstOrDefault().FirstName.Should().Be(otherPersons[4].FirstName);
            response.Parent.FirstOrDefault().LastName.Should().Be(otherPersons[4].LastName);
            response.Parent.FirstOrDefault().Gender.Should().Be(otherPersons[4].Gender);
        }
    }
}
