using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.CaseStatus
{
    [TestFixture]
    public class CaseStatusTypeFieldsControllerTest
    {
        private CaseStatusController _caseStatusTypeFieldsController;
        private Mock<ICaseStatusesUseCase> _mockCaseStatusesUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<ICaseStatusesUseCase>();
            _caseStatusTypeFieldsController = new CaseStatusController(_mockCaseStatusesUseCase.Object);
        }

        [Test]
        public void GetCaseStatusTypeFieldsByTypeReturns200WhenCaseStatusTypeIsFound()
        {
            var getCaseStatusFieldsResponse = _fixture.Create<GetCaseStatusFieldsResponse>();
            _mockCaseStatusesUseCase.Setup(x => x.ExecuteGetFields(It.IsAny<GetCaseStatusFieldsRequest>()))
                .Returns(getCaseStatusFieldsResponse);

            var response = _caseStatusTypeFieldsController.GetCaseStatusTypeFields(new GetCaseStatusFieldsRequest { Type = "Test" }) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(getCaseStatusFieldsResponse);
        }

        [Test]
        public void GetCaseStatusTypeFieldsByTypeReturns404WhenCaseStatusTypeIsNotFound()
        {
            _mockCaseStatusesUseCase.Setup(x => x.ExecuteGetFields(It.IsAny<GetCaseStatusFieldsRequest>()))
                .Throws<CaseStatusNotFoundException>();

            var response = _caseStatusTypeFieldsController.GetCaseStatusTypeFields(new GetCaseStatusFieldsRequest { Type = "NonExistent" }) as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Case Status Type does not exist.");
        }
    }
}
