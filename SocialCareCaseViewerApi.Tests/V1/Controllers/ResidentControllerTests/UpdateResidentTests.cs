using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.ResidentControllerTests
{
    [TestFixture]
    public class UpdateResidentTests
    {
        private ResidentController _residentController = null!;
        private Mock<IResidentUseCase> _mockResidentUseCase = null!;
        private Mock<ICreateRequestAuditUseCase> _mockCreateRequestAuditUseCase = null!;

        [SetUp]
        public void SetUp()
        {
            _mockResidentUseCase = new Mock<IResidentUseCase>();
            _mockCreateRequestAuditUseCase = new Mock<ICreateRequestAuditUseCase>();
            _residentController = new ResidentController(_mockResidentUseCase.Object, _mockCreateRequestAuditUseCase.Object);
        }

        [Test]
        public void UpdatePersonReturns204WhenSuccessful()
        {
            var request = new UpdatePersonRequest();

            var result = _residentController.UpdatePerson(request) as ObjectResult;

            result?.StatusCode.Should().Be(204);
        }

        [Test]
        public void PatchPersonReturns204WhenSuccessful()
        {
            var request = new PatchPersonRequest();

            var result = _residentController.PatchPerson(request) as ObjectResult;

            result?.StatusCode.Should().Be(204);
        }

        [Test]
        public void UpdatePersonReturns404WhenPersonNotFound()
        {
            var request = new UpdatePersonRequest();

            _mockResidentUseCase
                .Setup(x => x.UpdateResident(request))
                .Throws(new UpdatePersonException("Person not found"));

            var result = _residentController.UpdatePerson(request) as ObjectResult;

            result?.StatusCode.Should().Be(404);
            result?.Value.Should().Be("Person not found");
        }
    }
}
