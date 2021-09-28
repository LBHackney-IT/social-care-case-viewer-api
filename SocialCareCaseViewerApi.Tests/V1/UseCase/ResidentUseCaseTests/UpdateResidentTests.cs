using Bogus;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.ResidentUseCaseTests
{
    [TestFixture]
    public class UpdateResidentTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway = null!;
        private ResidentUseCase _residentUseCase = null!;
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _residentUseCase = new ResidentUseCase(_mockDataBaseGateway.Object);
        }

        private UpdatePersonRequest GetValidUpdatePersonRequest()
        {
            return new Faker<UpdatePersonRequest>()
                .RuleFor(x => x.Id, _faker.Random.Long());
        }

        [Test]
        public void ExecutePatchCallsDatabaseGateway()
        {
            var request = GetValidUpdatePersonRequest();

            _residentUseCase.UpdateResident(request);

            _mockDataBaseGateway.Verify(x => x.UpdatePerson(request));
        }
    }
}
