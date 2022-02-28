using AutoFixture;
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
        public void AllocateresidentReturns204WhenSuccessful()
        {
            var result = _residentController.AllocateResidentToTheTeam(_request, 2) as NoContentResult;
            result?.StatusCode.Should().Be(204);

        }
        [Test]
        public void AllocateResidentReturns400WhenPersonNotFound()
        {

            _mockResidentUseCase
                .Setup(x => x.AllocateResidentToTheTeam(_request))
                .Throws(new PersonNotFoundException("Person not found"));

            var result = _residentController.AllocateResidentToTheTeam(_request, 999) as ObjectResult;

            result?.StatusCode.Should().Be(400);
        }
    }

}
