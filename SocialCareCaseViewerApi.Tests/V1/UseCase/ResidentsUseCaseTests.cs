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
    public class ResidentUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private Mock<IMosaicAPIGateway> _mockMosaicAPIGateway;
        private ResidentsUseCase _residentsUseCase;
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _mockMosaicAPIGateway = new Mock<IMosaicAPIGateway>();
            _residentsUseCase = new ResidentsUseCase(_mockDataBaseGateway.Object, _mockMosaicAPIGateway.Object);
        }

        [Test]
        public void ExecuteGetCallsDatabaseGateway()
        {
            var request = _faker.Random.Long();

            _residentsUseCase.ExecuteGet(request);

            _mockDataBaseGateway.Verify(x => x.GetPersonDetailsById(request));
        }

        [Test]
        public void ExecuteGetReturnsNullWhenPersonNotFound()
        {
            var request = _faker.Random.Long();
            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request)).Returns((dbPerson) null);

            var result = _residentsUseCase.ExecuteGet(request);

            result.Should().BeNull();
        }

        [Test]
        public void ExecuteGetReturnsGetPersonResponseWhenPersonIsFound()
        {
            var request = _faker.Random.Long();
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            _mockDataBaseGateway.Setup(x => x.GetPersonDetailsById(request)).Returns(person);

            var response = _residentsUseCase.ExecuteGet(request);

            response.Should().BeOfType<GetPersonResponse>();
        }

        [Test]
        public void ExecutePatchCallsDatabaseGateway()
        {
            var request = new Faker<UpdatePersonRequest>()
                .RuleFor(x => x.Id, _faker.Random.Long())
                .Generate();
            _mockDataBaseGateway.Setup(x => x.UpdatePerson(request));

            _residentsUseCase.ExecutePatch(request);

            _mockDataBaseGateway.Verify(x => x.UpdatePerson(request));
        }
    }
}
