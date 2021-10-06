using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using dbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Residents
{
    [TestFixture]
    public class GetResidentTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway = null!;
        private ResidentUseCase _residentUseCase = null!;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _residentUseCase = new ResidentUseCase(_mockDataBaseGateway.Object);

        }

        private GetPersonRequest GetValidGetPersonRequest()
        {
            return _fixture.Build<GetPersonRequest>()
                .With(x => x.Id, _faker.Random.Long(1))
                .Create();
        }

        [Test]
        public void ExecuteGetCallsDatabaseGateway()
        {
            var request = GetValidGetPersonRequest();

            _residentUseCase.GetResident(request);

            _mockDataBaseGateway.Verify(x => x.GetPersonDetailsById(request.Id));
        }

        [Test]
        public void ExecuteGetReturnsNullWhenPersonNotFound()
        {
            var request = GetValidGetPersonRequest();

            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request.Id));

            var result = _residentUseCase.GetResident(request);

            result.Should().BeNull();
        }

        [Test]
        public void ExecuteGetReturnsGetPersonResponseWhenPersonIsFound()
        {
            var request = GetValidGetPersonRequest();
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request.Id)).Returns(person);

            var response = _residentUseCase.GetResident(request);

            response.Should().BeOfType<GetPersonResponse>();
        }
    }
}
