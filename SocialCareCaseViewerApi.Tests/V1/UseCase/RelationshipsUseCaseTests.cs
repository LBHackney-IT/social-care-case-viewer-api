using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class RelationshipsUseCaseTests
    {
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private RelationshipsUseCase _relationshipsUseCase;
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _relationshipsUseCase = new RelationshipsUseCase(_mockSocialCarePlatformAPIGateway.Object, _mockDatabaseGateway.Object);
        }

        [Test]
        public void ExecuteGetCallsDatabaseGateway()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(_faker.Random.Long());

            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(It.IsAny<long>()));
        }

        [Test]
        public void ExecuteGetCallsSocialCarePlatformAPIGateway()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(_faker.Random.Long());

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetRelationshipsByPersonId(It.IsAny<long>()));
        }

        [Test]
        public void ExecuteGetThrowsGetRelationshipsExceptionWithCorrectMessageWhenPersonIsNotFoundAndDatabaseGatewayReturnsNull()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns((Person) null);

            _relationshipsUseCase.Invoking(x => x.ExecuteGet(_faker.Random.Long()))
                .Should().Throw<GetRelationshipsException>()
                .WithMessage("Person not found");
        }

        [Test]
        public void ExecuteGetReturnsDefaultListRelationshipsResponseWhenSocialCarePlatformAPIReturnsNull()
        {
            var personId = _faker.Random.Long();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns((Relationships) null);
            var expectedResult = new ListRelationshipsResponse() { PersonId = personId };

            var result = _relationshipsUseCase.ExecuteGet(personId);

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void ExecuteGetCallsSocialCarePlatformAPIGatewayWhenPersonIdIsUsed()
        {
            var personId = _faker.Random.Long();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(personId);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetRelationshipsByPersonId(personId));
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipsResponseWhenPersonIsFoundAndSocialCarePlatformAPIReturnsValidResponse()
        {
            var personId = _faker.Random.Long();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(TestHelpers.CreateRelationships(personId));

            var result = _relationshipsUseCase.ExecuteGet(_faker.Random.Long());

            result.Should().NotBeNull();
            result.Should().BeOfType<ListRelationshipsResponse>();
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipsResponseWithCorrectRelatedPersonDetails()
        {
            var personId = _faker.Random.Long();
            var person = TestHelpers.CreatePerson();

            List<Person> children, others, parents, siblings;
            Relationships relationships;

            (children, others, parents, siblings, relationships) = TestHelpers.CreatePersonsWithRelationships(person.Id);

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(personId)).Returns(relationships);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(personId)).Returns(person);

            var personIds = new List<long>();

            personIds.AddRange(relationships.PersonalRelationships.Children);
            personIds.AddRange(relationships.PersonalRelationships.Other);
            personIds.AddRange(relationships.PersonalRelationships.Parents);
            personIds.AddRange(relationships.PersonalRelationships.Siblings);

            var personRecords = new List<Person>();
            personRecords.AddRange(children);
            personRecords.AddRange(others);
            personRecords.AddRange(parents);
            personRecords.AddRange(siblings);

            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(personRecords);

            var expectedResult = new ListRelationshipsResponse()
            {
                PersonId = personId,
                PersonalRelationships = new PersonalRelationships<RelatedPerson>()
                {
                    Children = AddRelatedPerson(children),
                    Other = AddRelatedPerson(others),
                    Parents = AddRelatedPerson(parents),
                    Siblings = AddRelatedPerson(siblings)
                }
            };

            var result = _relationshipsUseCase.ExecuteGet(personId);

            result.Should().NotBeNull();
            result.PersonalRelationships.Children.Count.Should().Be(2);
            result.PersonalRelationships.Children.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Children);

            result.PersonalRelationships.Other.Count.Should().Be(2);
            result.PersonalRelationships.Other.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Other);

            result.PersonalRelationships.Parents.Count.Should().Be(2);
            result.PersonalRelationships.Parents.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Parents);

            result.PersonalRelationships.Siblings.Count.Should().Be(2);
            result.PersonalRelationships.Siblings.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Siblings);
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipResponseWithEmptyListForRelationshipTypesThatDontHaveValues()
        {
            var personId = _faker.Random.Long();
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(personId)).Returns(new Relationships());
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(personId)).Returns(new Person());

            var result = _relationshipsUseCase.ExecuteGet(personId);

            result.Should().NotBeNull();
            result.PersonalRelationships.Should().NotBeNull();
            result.PersonalRelationships.Children.Should().NotBeNull();
            result.PersonalRelationships.Children.Count.Should().Be(0);
            result.PersonalRelationships.Other.Should().NotBeNull();
            result.PersonalRelationships.Other.Count.Should().Be(0);
            result.PersonalRelationships.Parents.Should().NotBeNull();
            result.PersonalRelationships.Parents.Count.Should().Be(0);
            result.PersonalRelationships.Siblings.Should().NotBeNull();
            result.PersonalRelationships.Siblings.Count.Should().Be(0);
        }

        [Test]
        public void ExecuteGetDoesntCallDatabaseGatewaysGetPersonsByListOfIdsMethodWhenSocialCarePlatformAPIGatewayDoesntReturnAnyRelationships()
        {
            var personId = _faker.Random.Long();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(personId)).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(personId);

            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()), Times.Never);
        }

        [Test]
        public void ExecuteGetCallsDatabaseGatewaysGetPersonsByListOfIdsWhenSocialCarePlatformAPIGatewayReturnsRelationships()
        {
            var personId = _faker.Random.Long();
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(personId)).Returns(TestHelpers.CreateRelationships(personId));
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(personId)).Returns(new Person());

            _relationshipsUseCase.ExecuteGet(personId);
            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()), Times.Once);
        }

        private static List<RelatedPerson> AddRelatedPerson(IEnumerable<Person> persons)
        {
            return persons.Select(x => new RelatedPerson()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();
        }
    }
}
