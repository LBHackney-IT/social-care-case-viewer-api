using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class RelationshipsV1UseCaseTests
    {
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private RelationshipsV1UseCase _relationshipsUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _relationshipsUseCase = new RelationshipsV1UseCase(_mockSocialCarePlatformAPIGateway.Object, _mockDatabaseGateway.Object);
        }

        private static ListRelationshipsV1Request GetValidRequest()
        {
            return new ListRelationshipsV1Request() { PersonId = 555666777 };
        }

        [Test]
        public void ExecuteGetCallsDatabaseGateway()
        {
            var request = GetValidRequest();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(GetValidRequest());

            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(It.IsAny<long>()));
        }

        [Test]
        public void ExcecuteGetCallsSocialCarePlatformAPIGateway()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(GetValidRequest());

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetRelationshipsByPersonId(It.IsAny<long>()));
        }

        [Test]
        public void ExecuteGetThrowsGetRelationshipsExceptionWithCorrectMessageWhenPersonIsNotFoundAndDatabaseGatewayReturnsNull()
        {
            var request = GetValidRequest();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns((Person) null);

            _relationshipsUseCase.Invoking(x => x.ExecuteGet(request))
                .Should().Throw<GetRelationshipsException>()
                .WithMessage("Person not found");
        }

        [Test]
        public void ExecuteGetReturnsDefaultListRelationshipsResponseWhenSocialCarePlatformAPIReturnsNull()
        {
            var request = GetValidRequest();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns((Relationships) null);

            var expectedResult = new ListRelationshipsV1Response() { PersonId = request.PersonId };

            var result = _relationshipsUseCase.ExecuteGet(request);

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void ExecuteGetCallsSocialCarePlatformAPIGatewayWhenPersonIdIsUsed()
        {
            var request = GetValidRequest();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(request);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetRelationshipsByPersonId(request.PersonId));
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipsResponseWhenPersonIsFoundAndSocialCarePlatformAPIReturnsValidResponse()
        {
            var request = GetValidRequest();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(It.IsAny<long>())).Returns(TestHelpers.CreateRelationships(request.PersonId));

            var result = _relationshipsUseCase.ExecuteGet(request);

            result.Should().NotBeNull();

            result.Should().BeOfType<ListRelationshipsV1Response>();
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipsResponseWithCorrectRelatedPersonDetails()
        {
            var request = GetValidRequest();

            Person person = TestHelpers.CreatePerson();

            List<Person> children, others, parents, siblings;
            Relationships relationships;

            (children, others, parents, siblings, relationships) = TestHelpers.CreatePersonsWithRelationships(person.Id);

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(request.PersonId)).Returns(relationships);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.PersonId)).Returns(person);

            var personIds = new List<long>();

            personIds.AddRange(relationships.PersonalRelationships.Children);
            personIds.AddRange(relationships.PersonalRelationships.Other);
            personIds.AddRange(relationships.PersonalRelationships.Parents);
            personIds.AddRange(relationships.PersonalRelationships.Siblings);

            List<Person> personRecords = new List<Person>();
            personRecords.AddRange(children);
            personRecords.AddRange(others);
            personRecords.AddRange(parents);
            personRecords.AddRange(siblings);

            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(personRecords);

            var expectedResult = new ListRelationshipsV1Response()
            {
                PersonId = request.PersonId,
                PersonalRelationships = new PersonalRelationships<RelatedPerson>()
                {
                    Children = AddRelatedPerson(children),
                    Other = AddRelatedPerson(others),
                    Parents = AddRelatedPerson(parents),
                    Siblings = AddRelatedPerson(siblings)
                }
            };

            var result = _relationshipsUseCase.ExecuteGet(request);

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
            var request = GetValidRequest();

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(request.PersonId)).Returns(new Relationships());
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.PersonId)).Returns(new Person());

            var result = _relationshipsUseCase.ExecuteGet(request);

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
            var request = GetValidRequest();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(request.PersonId)).Returns(new Relationships());

            _relationshipsUseCase.ExecuteGet(request);
            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()), Times.Never);
        }

        [Test]
        public void ExecuteGetCallsDatabaseGatewaysGetPersonsByListOfIdsWhenSocialCarePlatformAPIGatewayReturnsRelationships()
        {
            var request = GetValidRequest();
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonId(request.PersonId)).Returns(TestHelpers.CreateRelationships(request.PersonId));
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.PersonId)).Returns(new Person());

            _relationshipsUseCase.ExecuteGet(request);
            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()), Times.Once);
        }

        private static List<RelatedPerson> AddRelatedPerson(List<Person> persons)
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
