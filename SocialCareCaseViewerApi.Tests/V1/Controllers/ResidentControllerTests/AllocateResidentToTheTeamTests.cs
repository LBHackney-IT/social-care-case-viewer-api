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
    public class AllocateResidentToTheTeamTests
    {

        private ResidentController _residentController = null!;

        private AllocateResidentToTheTeamRequest _request = null!;
        private Mock<IResidentUseCase> _mockResidentUseCase = null!;
        private Mock<ICreateRequestAuditUseCase> _mockCreateRequestAuditUseCase = null!;

        [SetUp]

        public void SetUp()
        {
            _mockResidentUseCase = new Mock<IResidentUseCase>();
            _mockCreateRequestAuditUseCase = new Mock<ICreateRequestAuditUseCase>();
            _residentController = new ResidentController(_mockResidentUseCase.Object, _mockCreateRequestAuditUseCase.Object);
            _request = new AllocateResidentToTheTeamRequest();
        }

        [Test]
        public void AllocateResidentReturns204WhenSuccessful()
        {
            var result = _residentController.AllocateResidentToTheTeam(_request, 2) as NoContentResult;
            result?.StatusCode.Should().Be(204);
        }

        [Test]
        public void AllocateResidentReturns400WhenValidationResultsIsNotValid()
        {
            var request = _request;
            request.TeamId = 2;
            var response = _residentController.AllocateResidentToTheTeam(request, 0) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Resident Id must be grater than 0");
        }

        [Test]
        public void AllocateResidentReturns400StatusWhenPersonNotFoundExceptionExceptionThrown()
        {
            const string errorMessage = "Person not found";
            var request = _request;
            request.TeamId = 1;
            _mockResidentUseCase.Setup(x => x.AllocateResidentToTheTeam(_request))
                .Throws(new PersonNotFoundException(errorMessage));

            var response = _residentController.AllocateResidentToTheTeam(_request, 1) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void AllocateResidentReturns400StatusWhenTeamNotFoundExceptionThrown()
        {
            const string errorMessage = "Team not found";
            var request = _request;
            request.TeamId = 1;
            _mockResidentUseCase.Setup(x => x.AllocateResidentToTheTeam(_request))
                .Throws(new TeamNotFoundException(errorMessage));

            var response = _residentController.AllocateResidentToTheTeam(_request, 1) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }
    }
}