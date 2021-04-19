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
using System;
using dbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class PersonUsecaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private PersonUseCase _personUsecase;
        private Fixture _fixture;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _personUsecase = new PersonUseCase(_mockDataBaseGateway.Object);
            _fixture = new Fixture();
            _faker = new Faker();
        }

        private GetPersonRequest GetValidGetPersonRequest()
        {
            return _fixture.Build<GetPersonRequest>()
                .With(x => x.Id, _faker.Random.Long(1, long.MaxValue))
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

            _personUsecase.ExecuteGet(request);

            _mockDataBaseGateway.Verify(x => x.GetPersonDetailsById(request.Id));
        }

        [Test]
        public void ExecuteGetReturnsNullWhenPersonNotFound()
        {
            var request = GetValidGetPersonRequest();

            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request.Id)).Returns((dbPerson) null);

            var result = _personUsecase.ExecuteGet(request);

            result.Should().BeNull();
        }

        [Test]
        public void ExecuteGetReturnsGetPersonResponseWhenPersonIsFound()
        {
            var request = GetValidGetPersonRequest();

            dbPerson person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request.Id)).Returns(person);

            var response = _personUsecase.ExecuteGet(request);

            response.Should().BeOfType<GetPersonResponse>();
        }



        [Test]
        public void ExecutePatchCallsDatabaseGateway()
        {
            var request = GetValidUpdatePersonRequest();

            _personUsecase.ExecutePatch(request);

            _mockDataBaseGateway.Verify(x => x.UpdatePerson(request));
        }

        [Test]
        public void ExecutePatchReturnsCorrectMessageWhenPersonNotFound()
        {
            var request = GetValidUpdatePersonRequest();

            _mockDataBaseGateway.Setup(x => x.UpdatePerson(request)).Returns(new UpdatePersonResponse() { Message = "PersonNotFound" });

            var result = _personUsecase.ExecutePatch(request);

            result.Should().NotBeNull();
            result.Message.Should().Be("PersonNotFound");
        }

        [Test]
        public void ExecutePatchReturnsNullWhenPersonIsFoundAndUpdated()
        {
            var request = GetValidUpdatePersonRequest();

            _mockDataBaseGateway.Setup(x => x.UpdatePerson(request)).Returns((UpdatePersonResponse) null);

            var response = _personUsecase.ExecutePatch(request);

            response.Should().BeNull();
        }
    }
}
