using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class SocialCareCaseViewerApiControllerTests
    {
        private SocialCareCaseViewerApiController _classUnderTest;
        private Mock<IGetAllUseCase> _mockGetAllUseCase;
        private Mock<IAddNewResidentUseCase> _mockAddNewResidentUseCase;
        private Mock<IProcessDataUseCase> _mockProcessDataUseCase;
        private Mock<IAllocationsUseCase> _mockAllocationsUseCase;
        private Mock<IGetWorkersUseCase> _mockGetWorkersUseCase;
        private Mock<ITeamsUseCase> _mockTeamsUseCase;
        private Mock<ICaseNotesUseCase> _mockCaseNotesUseCase;
        private Mock<IVisitsUseCase> _mockVisitsUseCase;
        private Mock<IWarningNotesUseCase> _mockWarningNotesUseCase;
        private Mock<IGetRecordsUseCase> _mockGetRecordsUseCase;

        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockGetAllUseCase = new Mock<IGetAllUseCase>();
            _mockAddNewResidentUseCase = new Mock<IAddNewResidentUseCase>();
            _mockProcessDataUseCase = new Mock<IProcessDataUseCase>();
            _mockAllocationsUseCase = new Mock<IAllocationsUseCase>();
            _mockGetWorkersUseCase = new Mock<IGetWorkersUseCase>();
            _mockTeamsUseCase = new Mock<ITeamsUseCase>();
            _mockCaseNotesUseCase = new Mock<ICaseNotesUseCase>();
            _mockVisitsUseCase = new Mock<IVisitsUseCase>();
            _mockWarningNotesUseCase = new Mock<IWarningNotesUseCase>();
            _mockGetRecordsUseCase = new Mock<IGetRecordsUseCase>();


            _classUnderTest = new SocialCareCaseViewerApiController(_mockGetAllUseCase.Object, _mockAddNewResidentUseCase.Object,
            _mockProcessDataUseCase.Object, _mockAllocationsUseCase.Object, _mockGetWorkersUseCase.Object, _mockTeamsUseCase.Object,
            _mockCaseNotesUseCase.Object, _mockVisitsUseCase.Object, _mockWarningNotesUseCase.Object, _mockGetRecordsUseCase.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ListContactsReturns200WhenSuccessful()
        {
            var residentInformationList = _fixture.Create<ResidentInformationList>();
            var residentQueryParam = new ResidentQueryParam();

            _mockGetAllUseCase.Setup(x => x.Execute(residentQueryParam, 2, 3)).Returns(residentInformationList);
            var response = _classUnderTest.ListContacts(residentQueryParam, 2, 3) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(residentInformationList);
        }

        [Test]
        public void ListContactsReturns400WhenQueryParametersAreInvalid()
        {
            _mockGetAllUseCase.Setup(x => x.Execute(It.IsAny<ResidentQueryParam>(), 2, 3))
                .Throws(new InvalidQueryParameterException("Invalid Parameters"));

            var response = _classUnderTest.ListContacts(new ResidentQueryParam(), 2, 3) as BadRequestObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("Invalid Parameters");
        }

        [Test]
        public void AddNewResidentReturns201WhenSuccessful()
        {
            var addNewResidentResponse = _fixture.Create<AddNewResidentResponse>();
            var addNewResidentRequest = new AddNewResidentRequest();

            _mockAddNewResidentUseCase.Setup(x => x.Execute(addNewResidentRequest)).Returns(addNewResidentResponse);
            var response = _classUnderTest.AddNewResident(addNewResidentRequest) as CreatedAtActionResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(addNewResidentResponse);
        }

        [Test]
        public void AddNewResidentReturns500WhenResidentCouldNotBeInserted()
        {
            _mockAddNewResidentUseCase.Setup(x => x.Execute(It.IsAny<AddNewResidentRequest>()))
                .Throws(new ResidentCouldNotBeinsertedException("Resident could not be inserted"));

            var response = _classUnderTest.AddNewResident(new AddNewResidentRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
            response.Value.Should().Be("Resident could not be inserted");
        }

        [Test]
        public void AddNewResidentReturns500WhenAddressCouldNotBeInserted()
        {
            _mockAddNewResidentUseCase.Setup(x => x.Execute(It.IsAny<AddNewResidentRequest>()))
                .Throws(new AddressCouldNotBeInsertedException("Address could not be inserted"));

            var response = _classUnderTest.AddNewResident(new AddNewResidentRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
            response.Value.Should().Be("Address could not be inserted");
        }

        [Test]
        public void ListCasesReturns200WhenSuccessful()
        {
            var careCaseDataList = _fixture.Create<ResidentRecords>();
            var listCasesRequest = new GetRecordsRequest();

            _mockGetRecordsUseCase.Setup(x => x.Execute(listCasesRequest)).Returns(careCaseDataList);
            var response = _classUnderTest.GetRecords(listCasesRequest) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(careCaseDataList);
        }

        [Test]
        public void ListCasesReturns404WhenNoCasesAreFound()
        {
            _mockGetRecordsUseCase.Setup(x => x.Execute(It.IsAny<GetRecordsRequest>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));

            var response = _classUnderTest.GetRecords(new GetRecordsRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
            response.Value.Should().Be("Document Not Found");
        }

        [Test]
        public void GetWorkersReturns200WhenMatchingWorker()
        {
            var request = new GetWorkersRequest() { TeamId = 5 };
            var workersList = _fixture.Create<List<WorkerResponse>>();
            _mockGetWorkersUseCase.Setup(x => x.Execute(request)).Returns(workersList);

            var response = _classUnderTest.GetWorkers(request) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseValue = response.Value as List<WorkerResponse>;

            responseValue.Should().BeOfType<List<WorkerResponse>>();
            responseValue.Count.Should().Be(workersList.Count);
        }

        [Test]
        public void GetWorkersReturns404WhenNoWorkersFound()
        {
            var workers = new List<WorkerResponse>();
            var request = new GetWorkersRequest() { TeamId = 5 };
            _mockGetWorkersUseCase.Setup(x => x.Execute(request)).Returns(workers);

            var response = _classUnderTest.GetWorkers(request) as NotFoundResult;

            response.StatusCode.Should().Be(404);
        }

        #region Teams
        [Test]
        public void GetTeamsReturns200WhenSuccessful()
        {
            var request = new ListTeamsRequest() { ContextFlag = "A" };

            var teamsList = _fixture.Create<ListTeamsResponse>();

            _mockTeamsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListTeamsRequest>())).Returns(teamsList);

            var response = _classUnderTest.ListTeams(request);

            response.Should().NotBeNull();
        }
        #endregion

        #region Allocations
        [Test]
        public void CreateAllocationReturns201WhenSuccessful()
        {
            var request = _fixture.Create<CreateAllocationRequest>();
            var responseObject = new CreateAllocationResponse();

            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request))
                .Returns(responseObject);

            var response = _classUnderTest.CreateAllocation(request) as CreatedAtActionResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(responseObject);
        }

        [Test]
        public void UpdateAllocationReturns200WhenSuccessful()
        {
            var request = new UpdateAllocationRequest() { Id = _fixture.Create<int>() };

            UpdateAllocationResponse result = new UpdateAllocationResponse() { CaseNoteId = _fixture.Create<string>() }; //TODO: test end to end with valid format

            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Returns(result);

            var response = _classUnderTest.UpdateAllocation(request);

            response.Should().NotBeNull();
        }

        [Test]
        public void UpdateAllocationReturns500WhenUpdateFails()
        {
            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Throws(new EntityUpdateException("Unable to update allocation"));

            var response = _classUnderTest.UpdateAllocation(new UpdateAllocationRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
        }

        //TODO: test other exception types

        #endregion

        #region Case notes

        [Test]
        public void ListCaseNotesByPersonIdReturns200WhenSuccessful()
        {
            var request = new ListCaseNotesRequest { Id = "1" };

            var notesList = _fixture.Create<ListCaseNotesResponse>();

            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetByPersonId(It.IsAny<string>())).Returns(notesList);

            var response = _classUnderTest.ListCaseNotes(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().NotBeNull();
        }

        [Test]
        public void GettingASingleCaseNoteByNoteIdReturns200WhenSuccessful()
        {
            var request = new GetCaseNotesRequest { Id = "1" };

            var note = _fixture.Create<CaseNoteResponse>();

            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetById(It.IsAny<string>())).Returns(note);

            var response = _classUnderTest.GetCaseNoteById(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().NotBeNull();
        }

        [Test]
        public void GetCaseNotesByNoteIdReturns404WhenNoMatchingCaseNoteId()
        {
            var request = new GetCaseNotesRequest { Id = "1" };

            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetById(It.IsAny<string>()))
                .Throws(new SocialCarePlatformApiException("404"));

            var response = _classUnderTest.GetCaseNoteById(request) as StatusCodeResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public void GetCaseNotesByNoteIdReturns500WhenSocialCarePlatformApiExceptionIs500()
        {
            var request = new GetCaseNotesRequest { Id = "1" };

            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetById(It.IsAny<string>()))
                .Throws(new SocialCarePlatformApiException("500"));

            var response = _classUnderTest.GetCaseNoteById(request) as StatusCodeResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
        }

        [Test]
        public void GivenAValidPersonIdWhenListCaseNotesIsCalledTheControllerReturnsCorrectJsonResponse()
        {
            const string personId = "123";
            var request = new ListCaseNotesRequest { Id = personId };
            var response = new ListCaseNotesResponse { CaseNotes = new List<CaseNote>() };
            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetByPersonId(personId)).Returns(response);

            var actualResponse = _classUnderTest.ListCaseNotes(request);
            var okResult = (OkObjectResult) actualResponse;
            var resultContent = (ListCaseNotesResponse) okResult.Value;

            Assert.NotNull(actualResponse);
            Assert.NotNull(okResult);
            Assert.IsInstanceOf<ListCaseNotesResponse>(resultContent);
            Assert.NotNull(resultContent);
            Assert.AreEqual(JsonConvert.SerializeObject(response), JsonConvert.SerializeObject(resultContent));
            Assert.AreEqual(200, okResult.StatusCode);
        }

        #endregion

        #region case
        [Test]
        public void GetCaseByIdReturns200WhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<ResidentRecord>();
            var testRequest = _fixture.Create<GetCaseByIdRequest>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _classUnderTest.GetCaseByRecordId(testRequest) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetCaseByIdReturnsCareCaseDataWhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<ResidentRecord>();
            var testRequest = _fixture.Create<GetCaseByIdRequest>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _classUnderTest.GetCaseByRecordId(testRequest) as OkObjectResult;

            response.Value.Should().BeEquivalentTo(stubbedCaseData);
        }

        [Test]
        public void GetCaseByIdReturns404WhenNoCaseisFound()
        {
            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));
            var testRequest = _fixture.Create<GetCaseByIdRequest>();

            var response = _classUnderTest.GetCaseByRecordId(testRequest) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
            response.Value.Should().Be("Document Not Found");
        }
        #endregion

        #region Visits

        [Test]
        public void WhenShowHistoricDataFeatureFlagIsTrueListVisitsByPersonIdReturns200WhenSuccessful()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA", "true");

            var request = new ListVisitsRequest { Id = "1" };

            var visitList = _fixture.Create<ListVisitsResponse>();

            _mockVisitsUseCase.Setup(x => x.ExecuteGetByPersonId(It.IsAny<string>())).Returns(visitList);

            var response = _classUnderTest.ListVisits(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().NotBeNull();
        }

        [Test]
        public void WhenShowHistoricDataFeatureFlagIsNullListVisitsByPersonIdReturnsAResponseWithNoVisitData()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA", null);
            var request = new ListVisitsRequest { Id = "1" };

            var response = _classUnderTest.ListVisits(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeNull();
        }


        [Test]
        public void WhenShowHistoricDataFeatureFlagIsNotEqualToTrueListVisitsByPersonIdReturnsAResponseWithNoVisitData()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA", "false");
            var request = new ListVisitsRequest { Id = "1" };

            var response = _classUnderTest.ListVisits(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeNull();
        }

        #endregion

        #region WarningNotes
        [Test]
        public void CreateWarningNoteReturns201WhenSuccessful()
        {
            var createWarningNoteResponse = _fixture.Create<CreateWarningNoteResponse>();
            var createWarningNoteRequest = new CreateWarningNoteRequest();

            _mockWarningNotesUseCase
                .Setup(x => x.ExecutePost(createWarningNoteRequest))
                .Returns(createWarningNoteResponse);
            var response = _classUnderTest.CreateWarningNote(createWarningNoteRequest) as CreatedAtActionResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(createWarningNoteResponse);
        }

        [Test]
        public void CreateWarningNoteReturns500WhenWarningNoteCouldNotBeInserted()
        {
            _mockWarningNotesUseCase
                .Setup(x => x.ExecutePost(It.IsAny<CreateWarningNoteRequest>()))
                .Throws(new CreateWarningNoteException("Warning Note could not be inserted"));

            var response = _classUnderTest.CreateWarningNote(new CreateWarningNoteRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
            response.Value.Should().Be("Warning Note could not be inserted");
        }
        #endregion
    }
}
