using Bogus;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Residents
{
    [TestFixture]
    public class AllocateResidentTests
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

        private AllocateResidentToTheTeamRequest GetValidAllocateResidentToTheTeamRequest()
        {
            return new Faker<AllocateResidentToTheTeamRequest>()
                .RuleFor(x => x.PersonId, _faker.Random.Long())
                .RuleFor(x => x.TeamId, _faker.Random.Int());
        }

        [Test]
        public void ExecutePostCallsDatabaseGateway()
        {
            var request = GetValidAllocateResidentToTheTeamRequest();

            _residentUseCase.AllocateResidentToTheTeam(request);

            _mockDataBaseGateway.Verify(x => x.AllocateResidentToTheTeam(request));
        }
    }
}
