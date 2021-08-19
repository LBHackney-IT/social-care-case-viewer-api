using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CaseStatusTypeFieldsControllerTest
    {
        private CaseStatusTypeFieldsController _caseStatusTypeFieldsController;
        private Mock<IGetCaseStatusFieldsUseCase> _mockCaseStatusesUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<IGetCaseStatusFieldsUseCase>();
            _caseStatusTypeFieldsController = new CaseStatusTypeFieldsController(_mockCaseStatusesUseCase.Object);
        }

        [Test]
        public void GetCaseStatusTypeFieldsByTypeReturns200WhenCaseStatusTypeIsFound()
        {
            var getCaseStatusFieldsResponse = _fixture.Create<GetCaseStatusFieldsResponse>();
            _mockCaseStatusesUseCase.Setup(x => x.Execute(It.IsAny<GetCaseStatusFieldsRequest>()))
                .Returns(getCaseStatusFieldsResponse);

            var response = _caseStatusTypeFieldsController.GetCaseStatusTypeFields("Test") as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(getCaseStatusFieldsResponse);
        }

        [Test]
        public void GetCaseStatusTypeFieldsByTypeReturns404WhenCaseStatusTypeIsNotFound()
        {
            _mockCaseStatusesUseCase.Setup(x => x.Execute(It.IsAny<GetCaseStatusFieldsRequest>()))
                .Throws<CaseStatusNotFoundException>();

            var response = _caseStatusTypeFieldsController.GetCaseStatusTypeFields("NonExistent") as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Case Status Type does not exist.");
        }
    }
}
