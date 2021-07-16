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

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.PersonalRelationships
{
    [TestFixture]
    public class PersonalRelationshipExecuteDeleteUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private PersonalRelationshipsUseCase _personalRelationshipsUseCase;
        private CreatePersonalRelationshipRequest _request;
        private Person _person;
        private Person _otherPerson;
        private PersonalRelationshipType _typeInExistingRelationship;
        private PersonalRelationship _relationship;
        private PersonalRelationshipDetail _rel_details;
        private readonly PersonalRelationshipType _typeInRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipType("friend");

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _personalRelationshipsUseCase = new PersonalRelationshipsUseCase(_mockDatabaseGateway.Object);

            _mockDatabaseGateway.Setup(x => x.GetPersonalRelationshipTypeByDescription(_typeInRequest.Description))
                .Returns(_typeInRequest);

            _request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(type: _typeInRequest.Description);

            _person = TestHelpers.CreatePerson((int) _request.PersonId);
            _otherPerson = TestHelpers.CreatePerson((int) _request.OtherPersonId);

            _typeInExistingRelationship = PersonalRelationshipsHelper.CreatePersonalRelationshipType("partner");

            _relationship = PersonalRelationshipsHelper.CreatePersonalRelationship(_person, _otherPerson, _typeInExistingRelationship);
            _rel_details = PersonalRelationshipsHelper.CreatePersonalRelationshipDetail(_relationship.Id, "some details for the relationship");
        }

        [Test]
        public void WhenRelationshipDoesNotExistThrowsRelationshipNotFoundException()
        {
            Action act = () => _personalRelationshipsUseCase.ExecuteDelete(0);

            act.Should().Throw<RelationshipNotFoundException>()
                .WithMessage($"'relationshipId' with '{0}' was not found.");
        }

        [Test]
        public void WhenPersonHasPersonalRelationshipsDeleteThem()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonalRelationshipById(It.IsAny<long>())).Returns(_relationship);

            _personalRelationshipsUseCase.ExecuteDelete(_relationship.Id);

            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonalRelationshipById(_relationship.Id));
            _mockDatabaseGateway.Verify(gateway => gateway.DeleteRelationships(_relationship));
        }
    }
}
