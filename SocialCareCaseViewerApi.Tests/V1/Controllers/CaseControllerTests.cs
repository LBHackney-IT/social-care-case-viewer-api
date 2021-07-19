using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CaseControllerTests
    {
        private CaseController _caseController;
        private Mock<ICaseRecordsUseCase> _mockProcessDataUseCase;
        private Mock<ICreateRequestAuditUseCase> _mockCreateRequestAuditUseCase;
        private Fixture _fixture;
        private Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataUseCase = new Mock<ICaseRecordsUseCase>();
            _mockCreateRequestAuditUseCase = new Mock<ICreateRequestAuditUseCase>();
            _caseController = new CaseController(_mockProcessDataUseCase.Object, _mockCreateRequestAuditUseCase.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void GetCaseByIdReturns200WhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();
            var stubbedRequest = _fixture.Create<GetCaseNotesRequest>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _caseController.GetCaseByRecordId(stubbedRequest) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetCaseByIdReturnsCareCaseDataWhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();
            var stubbedRequest = _fixture.Create<GetCaseNotesRequest>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _caseController.GetCaseByRecordId(stubbedRequest) as OkObjectResult;

            response?.Value.Should().BeEquivalentTo(stubbedCaseData);
        }

        [Test]
        public void GetCaseByIdReturnsNotFoundWhenNullIsReturned()
        {
            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>()));
            var stubbedRequest = _fixture.Create<GetCaseNotesRequest>();

            var response = _caseController.GetCaseByRecordId(stubbedRequest) as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Document Not Found");
        }

        [Test]
        public void ListCasesReturns200WhenSuccessful()
        {
            var careCaseDataList = _fixture.Create<CareCaseDataList>();
            var listCasesRequest = new ListCasesRequest();

            _mockProcessDataUseCase.Setup(x => x.GetResidentCases(listCasesRequest)).Returns(careCaseDataList);
            var response = _caseController.GetCases(listCasesRequest) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(careCaseDataList);
        }

        [Test]
        public void ListCasesReturns404WhenNoCasesAreFound()
        {
            _mockProcessDataUseCase.Setup(x => x.GetResidentCases(It.IsAny<ListCasesRequest>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));

            var response = _caseController.GetCases(new ListCasesRequest()) as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Document Not Found");
        }

        [Test]
        public void GetCaseByRecordIdDoesNotCallTheCreateRequestAuditUseCaseWhenAuditingIsEnabledIsFalse()
        {
            var request = new GetCaseNotesRequest();

            _caseController.GetCaseByRecordId(request);

            _mockCreateRequestAuditUseCase.Verify(x => x.Execute(It.IsAny<CreateRequestAuditRequest>()), Times.Never);
        }

        [Test]
        public void GetCaseByRecordIdCallsTheCreateRequestAuditUseCaseWhenAuditingIsEnabledIsTrueAndUserIdAndResidentIdAreProvided()
        {
            var getCaseNotesRequest = new GetCaseNotesRequest()
            {
                AuditingEnabled = true,
                UserId = _faker.Person.Email,
                Id = "tyut67t89t876t",
                ResidentId = "4"
            };

            _caseController.GetCaseByRecordId(getCaseNotesRequest);

            _mockCreateRequestAuditUseCase.Verify(x => x.Execute(It.IsAny<CreateRequestAuditRequest>()));
        }


        [Test]
        public void GetCaseByRecordIdCallsTheCreateRequestAuditUseCaseWithCorrectValuesWhenAuditingIsEnabled()
        {
            var getCaseNotesRequest = new GetCaseNotesRequest()
            {
                AuditingEnabled = true,
                UserId = _faker.Person.Email,
                Id = "tyut67t89t876t",
                ResidentId = "4"
            };

            var auditRequest = new CreateRequestAuditRequest()
            {
                ActionName = "view_case_note",
                UserName = getCaseNotesRequest.UserId,
                Metadata = new Dictionary<string, object>() {
                        { "residentId", getCaseNotesRequest.ResidentId },
                        { "casenoteId", getCaseNotesRequest.Id }
                    }
            };

            _mockCreateRequestAuditUseCase.Setup(x => x.Execute(auditRequest)).Verifiable();           

            _caseController.GetCaseByRecordId(getCaseNotesRequest);

            _mockCreateRequestAuditUseCase.Verify(x => x.Execute(It.Is<CreateRequestAuditRequest>(
                x => x.ActionName == auditRequest.ActionName
                && x.UserName == auditRequest.UserName
                && JsonConvert.SerializeObject(x.Metadata) == JsonConvert.SerializeObject(auditRequest.Metadata)
                )), Times.Once);
        }
    }
}
