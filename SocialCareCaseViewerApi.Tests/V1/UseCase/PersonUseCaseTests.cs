using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;
using dbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class PersonUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private PersonUseCase _personUseCase;
        private Fixture _fixture;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _personUseCase = new PersonUseCase(_mockDataBaseGateway.Object);
            _fixture = new Fixture();
            _faker = new Faker();
        }

        private GetPersonRequest GetValidGetPersonRequest()
        {
            return _fixture.Build<GetPersonRequest>()
                .With(x => x.Id, _faker.Random.Long(1))
                .Create();
        }

        private UpdatePersonRequest GetValidUpdatePersonRequest()
        {
            return new Faker<UpdatePersonRequest>()
                .RuleFor(x => x.Id, _faker.Random.Long());
        }

        [Test]
        public void ExecuteGetCallsDatabaseGateway()
        {
            var request = GetValidGetPersonRequest();

            _personUseCase.ExecuteGet(request);

            _mockDataBaseGateway.Verify(x => x.GetPersonDetailsById(request.Id));
        }

        [Test]
        public void ExecuteGetReturnsNullWhenPersonNotFound()
        {
            var request = GetValidGetPersonRequest();

            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request.Id)).Returns((dbPerson) null);

            var result = _personUseCase.ExecuteGet(request);

            result.Should().BeNull();
        }

        [Test]
        public void ExecuteGetReturnsGetPersonResponseWhenPersonIsFound()
        {
            var request = GetValidGetPersonRequest();

            dbPerson person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request.Id)).Returns(person);

            var response = _personUseCase.ExecuteGet(request);

            response.Should().BeOfType<GetPersonResponse>();
        }

        [Test]
        public void ExecutePatchCallsDatabaseGateway()
        {
            var request = GetValidUpdatePersonRequest();

            _personUseCase.ExecutePatch(request);

            _mockDataBaseGateway.Verify(x => x.UpdatePerson(request));
        }
    }
}
