using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Linq;

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
            var person = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationships();
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            _relationshipsUseCase.ExecuteGet(person.Id);

            _mockDatabaseGateway.Verify(x => x.GetPersonWithPersonalRelationshipsByPersonId(person.Id, It.IsAny<bool>()));
        }

        [Test]
        public void WhenPersonIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetRelationshipsExceptionWithMessage()
        {
            var personId = 123456789;
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns((Person) null);

            _relationshipsUseCase.Invoking(x => x.ExecuteGet(personId))
                .Should().Throw<GetRelationshipsException>()
                .WithMessage("Person not found");
        }

        [Test]
        public void WhenPersonHasNoPersonalRelationshipsReturnsDefaultListRelationshipsResponse()
        {
            var person = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationships(withRelationships: false);
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            var result = _relationshipsUseCase.ExecuteGet(person.Id);

            result.Should().BeEquivalentTo(new ListRelationshipsResponse() { PersonId = person.Id });
        }

        [Test]
        public void WhenPersonHasPersonalRelationshipsReturnsRelationshipsMappedToType()
        {
            var person = PersonalRelationshipsHelper.CreatePersonWithPersonalRelationships();
            _mockDatabaseGateway.Setup(x => x.GetPersonWithPersonalRelationshipsByPersonId(It.IsAny<long>(), It.IsAny<bool>())).Returns(person);

            var result = _relationshipsUseCase.ExecuteGet(person.Id);

            result.PersonalRelationships.Grandparent.Should().HaveCount(1);
            result.PersonalRelationships.Grandparent.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            result.PersonalRelationships.Parent.Should().HaveCount(1);
            result.PersonalRelationships.Parent.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            result.PersonalRelationships.Child.Should().HaveCount(1);
            result.PersonalRelationships.Child.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            result.PersonalRelationships.Neighbour.Should().HaveCount(1);
            result.PersonalRelationships.Neighbour.FirstOrDefault().Should().BeOfType<RelatedPerson>();
            result.PersonalRelationships.Acquaintance.Should().HaveCount(1);
            result.PersonalRelationships.Acquaintance.FirstOrDefault().Should().BeOfType<RelatedPerson>();
        }
    }
}
