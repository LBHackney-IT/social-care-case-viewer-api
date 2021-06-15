using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CaseControllerTests
    {
        private CaseController _caseController;
        private Mock<ICaseRecordsUseCase> _mockProcessDataUseCase;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataUseCase = new Mock<ICaseRecordsUseCase>();
            _caseController = new CaseController(_mockProcessDataUseCase.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void GetCaseByIdReturns200WhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _caseController.GetCaseByRecordId("caseString") as OkObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetCaseByIdReturnsCareCaseDataWhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _caseController.GetCaseByRecordId("caseString") as OkObjectResult;

            response?.Value.Should().BeEquivalentTo(stubbedCaseData);
        }

        [Test]
        public void GetCaseByIdReturns404WhenNoCaseIsFound()
        {
            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));

            var response = _caseController.GetCaseByRecordId("caseString") as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Document Not Found");
        }

        [Test]
        public void ListCasesReturns200WhenSuccessful()
        {
            var careCaseDataList = _fixture.Create<CareCaseDataList>();
            var listCasesRequest = new ListCasesRequest();

            _mockProcessDataUseCase.Setup(x => x.Execute(listCasesRequest)).Returns(careCaseDataList);
            var response = _caseController.GetCases(listCasesRequest) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(careCaseDataList);
        }

        [Test]
        public void ListCasesReturns404WhenNoCasesAreFound()
        {
            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<ListCasesRequest>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));

            var response = _caseController.GetCases(new ListCasesRequest()) as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Document Not Found");
        }
    }
}
