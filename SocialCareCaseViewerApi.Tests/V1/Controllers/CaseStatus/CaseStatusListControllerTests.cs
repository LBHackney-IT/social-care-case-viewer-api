using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.CaseStatus
{
    [TestFixture]
    public class CaseStatusListControllerTests
    {
        private CaseStatusController _caseStatusController;
        private Mock<ICaseStatusesUseCase> _mockCaseStatusesUseCase;
        private ListCaseStatusesRequest _request = new ListCaseStatusesRequest();

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<ICaseStatusesUseCase>();
            _caseStatusController = new CaseStatusController(_mockCaseStatusesUseCase.Object);
        }

        [Test]
        public void ListCaseStatusesReturns200WhenPersonIsFound()
        {
            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteGet(_request))
                .Returns(new List<CaseStatusResponse>());

            var response = _caseStatusController.ListCaseStatuses(_request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void ListCaseStatuses404WithCorrectErrorMessageWhenPersonIsNotFound()
        {
            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteGet(_request))
                .Throws(new GetCaseStatusesException("Person not found"));

            var response = _caseStatusController.ListCaseStatuses(_request) as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListCaseStatuses200AndCaseStatusesWhenSuccessful()
        {
            var caseStatusResponse = TestHelpers.CreateCaseStatus().ToDomain().ToResponse();
            var caseStatusResponseList = new List<CaseStatusResponse> { caseStatusResponse };
            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteGet(_request))
                .Returns(caseStatusResponseList);

            var response = _caseStatusController.ListCaseStatuses(_request) as ObjectResult;

            response?.Value.Should().BeEquivalentTo(caseStatusResponseList);
        }
    }
}
