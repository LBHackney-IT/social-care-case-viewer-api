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
        private RelationshipsV1UseCase _relationshipsV1UseCase;

        [SetUp]
        public void SetUp()
        {
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _relationshipsV1UseCase = new RelationshipsV1UseCase(_mockSocialCarePlatformAPIGateway.Object, _mockDatabaseGateway.Object);
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
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(It.IsAny<long>())).Returns(new RelationshipsV1());

            _relationshipsV1UseCase.ExecuteGet(GetValidRequest());

            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(It.IsAny<long>()));
        }

        [Test]
        public void ExcecuteGetCallsSocialCarePlatformAPIGateway()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(It.IsAny<long>())).Returns(new RelationshipsV1());

            _relationshipsV1UseCase.ExecuteGet(GetValidRequest());

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetRelationshipsByPersonIdV1(It.IsAny<long>()));
        }

        [Test]
        public void ExecuteGetThrowsGetRelationshipsExceptionWithCorrectMessageWhenPersonIsNotFoundAndDatabaseGatewayReturnsNull()
        {
            var request = GetValidRequest();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns((Person) null);

            _relationshipsV1UseCase.Invoking(x => x.ExecuteGet(request))
                .Should().Throw<GetRelationshipsException>()
                .WithMessage("Person not found");
        }

        [Test]
        public void ExecuteGetReturnsDefaultListRelationshipsResponseWhenSocialCarePlatformAPIReturnsNull()
        {
            var request = GetValidRequest();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(It.IsAny<long>())).Returns((RelationshipsV1) null);

            var expectedResult = new ListRelationshipsV1Response() { PersonId = request.PersonId };

            var result = _relationshipsV1UseCase.ExecuteGet(request);

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void ExecuteGetCallsSocialCarePlatformAPIGatewayWhenPersonIdIsUsed()
        {
            var request = GetValidRequest();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(It.IsAny<long>())).Returns(new RelationshipsV1());

            _relationshipsV1UseCase.ExecuteGet(request);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetRelationshipsByPersonIdV1(request.PersonId));
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipsResponseWhenPersonIsFoundAndSocialCarePlatformAPIReturnsValidResponse()
        {
            var request = GetValidRequest();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(It.IsAny<long>())).Returns(TestHelpers.CreateRelationshipsV1(request.PersonId));

            var result = _relationshipsV1UseCase.ExecuteGet(request);

            result.Should().BeOfType<ListRelationshipsV1Response>();
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipsResponseWithCorrectRelatedPersonDetails()
        {
            var request = GetValidRequest();

            Person person = TestHelpers.CreatePerson();

            List<Person> children, others, parents, siblings;
            RelationshipsV1 relationships;

            (children, others, parents, siblings, relationships) = TestHelpers.CreatePersonsWithRelationshipsV1(person.Id);

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(request.PersonId)).Returns(relationships);
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
                PersonalRelationships = new PersonalRelationshipsV1<RelatedPersonV1>()
                {
                    Children = AddRelatedPerson(children),
                    Other = AddRelatedPerson(others),
                    Parents = AddRelatedPerson(parents),
                    Siblings = AddRelatedPerson(siblings)
                }
            };

            var result = _relationshipsV1UseCase.ExecuteGet(request);

            result.PersonalRelationships.Children.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Children);
            result.PersonalRelationships.Other.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Other);
            result.PersonalRelationships.Parents.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Parents);
            result.PersonalRelationships.Siblings.Should().BeEquivalentTo(expectedResult.PersonalRelationships.Siblings);
        }

        [Test]
        public void ExecuteGetReturnsListRelationshipResponseWithEmptyListForRelationshipTypesThatDontHaveValues()
        {
            var request = GetValidRequest();

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(request.PersonId)).Returns(new RelationshipsV1());
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.PersonId)).Returns(new Person());

            var result = _relationshipsV1UseCase.ExecuteGet(request);

            result.PersonalRelationships.Children.Count.Should().Be(0);
            result.PersonalRelationships.Other.Count.Should().Be(0);
            result.PersonalRelationships.Parents.Count.Should().Be(0);
            result.PersonalRelationships.Siblings.Count.Should().Be(0);
        }

        [Test]
        public void ExecuteGetDoesntCallDatabaseGatewaysGetPersonsByListOfIdsMethodWhenSocialCarePlatformAPIGatewayDoesntReturnAnyRelationships()
        {
            var request = GetValidRequest();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(new Person());
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(request.PersonId)).Returns(new RelationshipsV1());

            _relationshipsV1UseCase.ExecuteGet(request);
            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()), Times.Never);
        }

        [Test]
        public void ExecuteGetCallsDatabaseGatewaysGetPersonsByListOfIdsWhenSocialCarePlatformAPIGatewayReturnsRelationships()
        {
            var request = GetValidRequest();
            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetRelationshipsByPersonIdV1(request.PersonId)).Returns(TestHelpers.CreateRelationshipsV1(request.PersonId));
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.PersonId)).Returns(new Person());

            _relationshipsV1UseCase.ExecuteGet(request);
            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()), Times.Once);
        }

        private static List<RelatedPersonV1> AddRelatedPerson(List<Person> persons)
        {
            return persons.Select(x => new RelatedPersonV1()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();
        }
    }
}
