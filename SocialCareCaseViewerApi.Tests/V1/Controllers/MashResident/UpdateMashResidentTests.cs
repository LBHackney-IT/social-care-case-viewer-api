using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.MashResident
{
    [TestFixture]
    public class UpdateMashResidentTests
    {
        private readonly Faker _faker = new Faker();
        private Mock<IMashResidentUseCase> _mashResidentUseCase = null!;
        private long _fakeMashResidentId;
        private MashResidentController _mashResidentController = null!;

        [SetUp]
        public void Setup()
        {
            _mashResidentUseCase = new Mock<IMashResidentUseCase>();
            _mashResidentController = new MashResidentController(_mashResidentUseCase.Object);
            _fakeMashResidentId = _faker.Random.Long(1, 2);
        }

        [Test]
        public void WhenSuccessfullyUpdateActionReturns200AndUpdatedMashResident()
        {
            var matchingPersonId = _faker.Random.Long(3, 4);
            var request = TestHelpers.CreateMashResidentUpdateRequest(matchingPersonId);

            var updatedMashResident = TestHelpers.CreateMashResident(_fakeMashResidentId).ToDomain().ToResponse();
            updatedMashResident.SocialCareId = matchingPersonId;

            _mashResidentUseCase.Setup(x => x.UpdateMashResident(request, _fakeMashResidentId))
                .Returns(updatedMashResident);

            var response = _mashResidentController.UpdateMashResident(request, _fakeMashResidentId) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(updatedMashResident);
        }

        [Test]
        public void WhenMashResidentUseCaseUpdateMashResidentThrowsPersonNotFoundExceptionReturnsBadRequest()
        {
            var matchingPersonId = _faker.Random.Long(3, 4);
            var request = TestHelpers.CreateMashResidentUpdateRequest(matchingPersonId);
            const string errorMessage = "test-error-message";
            _mashResidentUseCase
                .Setup(x => x.UpdateMashResident(request, _fakeMashResidentId))
                .Throws(new PersonNotFoundException(errorMessage));

            var response = _mashResidentController.UpdateMashResident(request, _fakeMashResidentId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void WhenMashResidentUseCaseUpdateMashResidentThrowsMashResidentNotFoundExceptionReturnsBadRequest()
        {
            var matchingPersonId = _faker.Random.Long(3, 4);
            var request = TestHelpers.CreateMashResidentUpdateRequest(matchingPersonId);
            const string errorMessage = "test-error-message";
            _mashResidentUseCase
                .Setup(x => x.UpdateMashResident(request, _fakeMashResidentId))
                .Throws(new MashResidentNotFoundException(errorMessage));

            var response = _mashResidentController.UpdateMashResident(request, _fakeMashResidentId) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }
    }
}
