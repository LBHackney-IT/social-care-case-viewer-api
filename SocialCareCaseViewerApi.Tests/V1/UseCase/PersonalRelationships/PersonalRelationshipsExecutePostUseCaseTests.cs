using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Collections.Generic;
using System;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Relationships
{
    [TestFixture]
    public class PersonalRelationshipsExecutePostUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private PersonalRelationshipsUseCase _personalRelationshipsUseCase;
        private CreatePersonalRelationshipRequest _request;
        private Person _person;
        private Person _otherPerson;
        private readonly PersonalRelationshipType _typeInRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipType("friend");

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _personalRelationshipsUseCase = new PersonalRelationshipsUseCase(_mockDatabaseGateway.Object);

            _mockDatabaseGateway.Setup(x => x.GetPersonalRelationshipTypeByDescription(_typeInRequest.Description))
                .Returns(_typeInRequest);

            var typeInExistingRelationship = PersonalRelationshipsHelper.CreatePersonalRelationshipType("partner");
            _mockDatabaseGateway.Setup(x => x.GetPersonalRelationshipTypeByDescription(typeInExistingRelationship.Description))
                .Returns(typeInExistingRelationship);

            _request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(type: _typeInRequest.Description);

            _person = TestHelpers.CreatePerson((int) _request.PersonId);
            _otherPerson = TestHelpers.CreatePerson((int) _request.OtherPersonId);

            var personalRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(_person, _otherPerson, typeInExistingRelationship);
            _person.PersonalRelationships = new List<PersonalRelationship>() { personalRelationship };

            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()))
                .Returns(new List<Person>() { _person, _otherPerson });

            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_person);

            _mockDatabaseGateway.Setup(x => x.CreatePersonalRelationship(It.IsAny<CreatePersonalRelationshipRequest>()))
                .Returns(personalRelationship);
        }

        [Test]
        public void CallsDatabaseGatewayToCheckPersonAndOtherPersonExists()
        {
            _personalRelationshipsUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonsByListOfIds(new List<long>() { _request.PersonId, _request.OtherPersonId }));
        }

        [Test]
        public void WhenPersonDoesNotExistThrowsPersonNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()))
                .Returns(new List<Person>() { new Person() { Id = _request.OtherPersonId } });

            Action act = () => _personalRelationshipsUseCase.ExecutePost(_request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"'personId' with '{_request.PersonId}' was not found.");
        }

        [Test]
        public void WhenOtherPersonDoesNotExistThrowsPersonNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>()))
                .Returns(new List<Person>() { new Person() { Id = _request.PersonId } });

            Action act = () => _personalRelationshipsUseCase.ExecutePost(_request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"'otherPersonId' with '{_request.OtherPersonId}' was not found.");
        }

        [Test]
        public void CallsDatabaseGatewayToCheckTypeExists()
        {
            _personalRelationshipsUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonalRelationshipTypeByDescription(_request.Type));
        }

        [Test]
        public void WhenTypeDoesNotExistThrowsPersonalRelationshipTypeNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonalRelationshipTypeByDescription(It.IsAny<string>()))
                .Returns((PersonalRelationshipType) null);

            Action act = () => _personalRelationshipsUseCase.ExecutePost(_request);

            act.Should().Throw<PersonalRelationshipTypeNotFoundException>()
                .WithMessage($"'type' with '{_request.Type}' was not found.");
        }

        [Test]
        public void CallsDatabaseGatewayToCheckIfRelationshipWithTheSameTypeAlreadyExists()
        {
            _personalRelationshipsUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonWithPersonalRelationshipsByPersonId(_request.PersonId, It.IsAny<bool>()));
        }

        [Test]
        public void WhenRelationshipWithTheSameTypeAlreadyExistsThrowsPersonalRelationshipAlreadyExistsException()
        {
            var sameTypeAsRequest = _typeInRequest;
            _person.PersonalRelationships = new List<PersonalRelationship>() { PersonalRelationshipsHelper.CreatePersonalRelationship(_person, _otherPerson, sameTypeAsRequest) };
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_person);

            Action act = () => _personalRelationshipsUseCase.ExecutePost(_request);

            act.Should().Throw<PersonalRelationshipAlreadyExistsException>()
                .WithMessage($"Personal relationship with 'type' of '{_request.Type}' already exists.");
        }

        [Test]
        public void CallsDatabaseGatewayToCreatePersonalRelationshipForPerson()
        {
            _personalRelationshipsUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.CreatePersonalRelationship(
                It.Is<CreatePersonalRelationshipRequest>(
                    request =>
                        request.PersonId == _request.PersonId &&
                        request.OtherPersonId == _request.OtherPersonId &&
                        request.TypeId == _typeInRequest.Id &&
                        request.IsMainCarer == _request.IsMainCarer &&
                        request.IsInformalCarer == request.IsInformalCarer &&
                        request.Details == _request.Details
                )
            ));
        }

        [Test]
        public void CallsDatabaseGatewayToCreateInversePersonalRelationshipForOtherPerson()
        {
            _personalRelationshipsUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.CreatePersonalRelationship(
                It.Is<CreatePersonalRelationshipRequest>(
                    request =>
                        request.PersonId == _request.OtherPersonId &&
                        request.OtherPersonId == _request.PersonId &&
                        request.TypeId == _typeInRequest.InverseTypeId &&
                        request.IsMainCarer == null &&
                        request.IsInformalCarer == null &&
                        request.Details == null
                )
            ));
        }
    }
}
