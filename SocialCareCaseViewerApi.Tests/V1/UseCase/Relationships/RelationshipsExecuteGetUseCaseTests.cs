using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Relationships
{
    [TestFixture]
    public class RelationshipsExecuteGetUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private RelationshipsUseCase _relationshipsUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _relationshipsUseCase = new RelationshipsUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void CallsDatabaseGateway()
        {
            var person = TestHelpers.CreatePerson();
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            _relationshipsUseCase.ExecuteGet(person.Id);

            _mockDatabaseGateway.Verify(x => x.GetPersonWithPersonalRelationshipsByPersonId(person.Id, It.IsAny<bool>()));
        }

        [Test]
        public void WhenPersonIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetRelationshipsExceptionWithMessage()
        {
            var person = TestHelpers.CreatePerson();
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns((Person) null);

            _relationshipsUseCase.Invoking(x => x.ExecuteGet(person.Id))
                .Should().Throw<GetRelationshipsException>()
                .WithMessage("Person not found");
        }

        [Test]
        public void WhenPersonHasPersonalRelationshipsAsNullReturnsDefaultListRelationshipsResponse()
        {
            var person = TestHelpers.CreatePerson();
            person.PersonalRelationships = null;
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            var result = _relationshipsUseCase.ExecuteGet(person.Id);

            result.Should().BeEquivalentTo(new ListRelationshipsResponse() { PersonId = person.Id });
        }

        [Test]
        public void WhenPersonHasPersonalRelationshipsAsEmptyListReturnsDefaultListRelationshipsResponse()
        {
            var person = TestHelpers.CreatePerson();
            person.PersonalRelationships = new List<PersonalRelationship>();
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            var result = _relationshipsUseCase.ExecuteGet(person.Id);

            result.Should().BeEquivalentTo(new ListRelationshipsResponse() { PersonId = person.Id });
        }

        [Test]
        public void WhenPersonHasPersonalRelationshipsReturnsRelationshipsMappedToType()
        {
            var (person, _, _) = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationships();
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            var result = _relationshipsUseCase.ExecuteGet(person.Id);

            result.PersonalRelationships.Should().HaveCount(3);
        }
    }
}
