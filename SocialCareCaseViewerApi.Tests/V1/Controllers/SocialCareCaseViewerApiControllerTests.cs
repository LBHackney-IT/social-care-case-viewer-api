using System;
using System.Collections.Generic;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;

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
        private Mock<IWarningNoteUseCase> _mockWarningNoteUseCase;
        private Mock<IGetVisitByVisitIdUseCase> _mockGetVisitByVisitIdUseCase;
        private Mock<IPersonUseCase> _mockPersonUseCase;

        private Fixture _fixture;
        private Faker _faker;

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
            _mockWarningNoteUseCase = new Mock<IWarningNoteUseCase>();
            _mockGetVisitByVisitIdUseCase = new Mock<IGetVisitByVisitIdUseCase>();
            _mockPersonUseCase = new Mock<IPersonUseCase>();

            _classUnderTest = new SocialCareCaseViewerApiController(_mockGetAllUseCase.Object, _mockAddNewResidentUseCase.Object,
            _mockProcessDataUseCase.Object, _mockAllocationsUseCase.Object, _mockGetWorkersUseCase.Object, _mockTeamsUseCase.Object,
            _mockCaseNotesUseCase.Object, _mockVisitsUseCase.Object, _mockWarningNoteUseCase.Object, _mockGetVisitByVisitIdUseCase.Object, _mockPersonUseCase.Object);
            _fixture = new Fixture();
            _faker = new Faker();
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
        public void GetPersonByIdReturns200WhenSuccessfull()
        {
            GetPersonRequest request = new GetPersonRequest();

            _mockPersonUseCase.Setup(x => x.ExecuteGet(It.IsAny<GetPersonRequest>())).Returns(new GetPersonResponse());

            var response = _classUnderTest.GetPerson(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetPersonByIdReturns404WhenPersonNotFound()
        {
            GetPersonRequest request = new GetPersonRequest();
            GetPersonResponse response = null;

            _mockPersonUseCase.Setup(x => x.ExecuteGet(It.IsAny<GetPersonRequest>())).Returns(response);

            var result = _classUnderTest.GetPerson(request) as NotFoundResult;

            result.StatusCode.Should().Be(404);
        }

        [Test]
        public void UpdatePersonReturns200WhenSuccessful()
        {
            UpdatePersonRequest request = new UpdatePersonRequest();

            var result = _classUnderTest.UpdatePerson(request) as StatusCodeResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }

        [Test]
        public void UpdatePersonReturns404WhenPersonNotFound()
        {
            UpdatePersonRequest request = new UpdatePersonRequest();

            _mockPersonUseCase.Setup(x => x.ExecutePatch(It.IsAny<UpdatePersonRequest>())).Throws(new UpdatePersonException("Person not found"));

            var result = _classUnderTest.UpdatePerson(request) as NotFoundObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(404);
            result.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListCasesReturns200WhenSuccessful()
        {
            var careCaseDataList = _fixture.Create<CareCaseDataList>();
            var listCasesRequest = new ListCasesRequest();

            _mockProcessDataUseCase.Setup(x => x.Execute(listCasesRequest)).Returns(careCaseDataList);
            var response = _classUnderTest.ListCases(listCasesRequest) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(careCaseDataList);
        }

        [Test]
        public void ListCasesReturns404WhenNoCasesAreFound()
        {
            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<ListCasesRequest>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));

            var response = _classUnderTest.ListCases(new ListCasesRequest()) as ObjectResult;

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
            var request = TestHelpers.CreateAllocationRequest().Item1;
            var responseObject = new CreateAllocationResponse();

            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request))
                .Returns(responseObject);

            var response = _classUnderTest.CreateAllocation(request) as CreatedAtActionResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(responseObject);
        }

        [Test]
        public void CreateAllocationReturns404WhenCreatingAllocationFails()
        {
            var request = TestHelpers.CreateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request)).Throws(new CreateAllocationException("Worker details cannot be found"));

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public void CreateAllocationReturns500WhenCaseNoteFails()
        {
            var request = TestHelpers.CreateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request)).Throws(new UpdateAllocationException("Unable to create a case note. Allocation not created"));

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidMosaicId()
        {
            var request = TestHelpers.CreateAllocationRequest(mosaicId: 0).Item1;
            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidAllocatedWorkerId()
        {
            var request = TestHelpers.CreateAllocationRequest(workerId: 0).Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidAllocatedTeamId()
        {
            var request = TestHelpers.CreateAllocationRequest(teamId: 0).Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidCreatedBy()
        {
            var request = TestHelpers.CreateAllocationRequest(createdBy: "").Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns200WhenSuccessful()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest().Item1;
            var result = new UpdateAllocationResponse() { CaseNoteId = _fixture.Create<string>() };

            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Returns(result);

            var response = _classUnderTest.UpdateAllocation(request) as OkObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(result);
        }

        [Test]
        public void UpdateAllocationReturns500WhenUpdateFails()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Throws(new EntityUpdateException("Unable to update allocation"));

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(500);
        }

        [Test]
        public void UpdateAllocationReturns500OnUpdateAllocationException()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Throws(new UpdateAllocationException("Unable to update allocation"));

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(500);
        }

        [Test]
        public void UpdateAllocationReturns400WhenInvalidMosaicId()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(id: 0).Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenEmptyDeallocationReason()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(deallocationReason: "").Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenCreatedByNotValidEmail()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(createdBy: "invalidEmail").Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenDeallocationDateInTheFuture()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(deallocationDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(400);
        }

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
            var stubbedCaseData = _fixture.Create<CareCaseData>();
            var testRequest = _fixture.Create<GetCaseByIdRequest>();

            _mockProcessDataUseCase.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedCaseData);
            var response = _classUnderTest.GetCaseByRecordId(testRequest) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetCaseByIdReturnsCareCaseDataWhenSuccessful()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();
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
        public void ListVisitsByPersonIdReturns200WhenSuccessful()
        {
            var request = new ListVisitsRequest { Id = "1" };
            var visit = TestHelpers.CreateVisit();
            var visitList = new List<Visit>();

            _mockVisitsUseCase.Setup(x => x.ExecuteGetByPersonId(It.IsAny<string>())).Returns(visitList);

            var response = _classUnderTest.ListVisits(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().NotBeNull();
        }


        [Test]
        public void GetVisitByVisitIdReturns200StatusAndVisitWhenSuccessful()
        {
            var visit = TestHelpers.CreateVisit();
            _mockGetVisitByVisitIdUseCase.Setup(x => x.Execute(visit.VisitId)).Returns(visit);

            var response = _classUnderTest.GetVisitByVisitId(visit.VisitId) as OkObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(visit);
        }

        [Test]
        public void GetVisitByVisitIdReturns404StatusAndNullWhenUnsuccessful()
        {
            var response = _classUnderTest.GetVisitByVisitId(1L) as NotFoundResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(404);
        }
        #endregion

        #region WarningNotes
        [Test]
        public void PostWarningNoteReturns201WhenSuccessful()
        {
            var postWarningNoteResponse = _fixture.Create<PostWarningNoteResponse>();
            var postWarningNoteRequest = new PostWarningNoteRequest();

            _mockWarningNoteUseCase
                .Setup(x => x.ExecutePost(postWarningNoteRequest))
                .Returns(postWarningNoteResponse);
            var response = _classUnderTest.PostWarningNote(postWarningNoteRequest) as CreatedAtActionResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(postWarningNoteResponse);
        }

        [Test]
        public void PostWarningNoteReturns500WhenWarningNoteCouldNotBeInserted()
        {
            _mockWarningNoteUseCase
                .Setup(x => x.ExecutePost(It.IsAny<PostWarningNoteRequest>()))
                .Throws(new PostWarningNoteException("Warning Note could not be inserted"));

            var response = _classUnderTest.PostWarningNote(new PostWarningNoteRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(500);
            response.Value.Should().Be("Warning Note could not be inserted");
        }

        [Test]
        public void GetWarningNoteReturns200AndGetWarningNoteResponseWhenSuccessful()
        {
            var testPersonId = _fixture.Create<long>();
            var stubbedWarningNotesResponse = _fixture.Create<ListWarningNotesResponse>();

            _mockWarningNoteUseCase
                .Setup(x => x.ExecuteGet(It.IsAny<long>()))
                .Returns(stubbedWarningNotesResponse);

            var response = _classUnderTest.ListWarningNotes(testPersonId) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(stubbedWarningNotesResponse);
        }

        [Test]
        public void GetWarningNoteReturns404IfDocumentNotFound()
        {
            var testPersonId = _fixture.Create<long>();

            _mockWarningNoteUseCase
                .Setup(x => x.ExecuteGet(It.IsAny<long>()))
                .Throws(new DocumentNotFoundException("Document Not Found"));

            var response = _classUnderTest.ListWarningNotes(testPersonId) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
            response.Value.Should().Be("Document Not Found");
        }

        [Test]
        public void PatchWarningNoteCallsTheUseCaseAndReturns204WhenSuccessful()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest().Item1;
            var response = _classUnderTest.PatchWarningNote(request) as NoContentResult;

            _mockWarningNoteUseCase.Verify(x => x.ExecutePatch(request), Times.Once);
            response.StatusCode.Should().Be(204);
        }

        [Test]
        public void PatchWarningNoteReturnsA404ResponseIfItEncountersAnException()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest().Item1;

            _mockWarningNoteUseCase
                .Setup(x => x.ExecutePatch(It.IsAny<PatchWarningNoteRequest>()))
                .Throws(new PatchWarningNoteException("exception encountered"));

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
            response.Value.Should().Be("exception encountered");
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidWarningNoteId()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(warningNoteId: 0).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidReviewDate()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(reviewDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidNextReviewDateId()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(nextReviewDate: DateTime.Now).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidStatus()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "invalid").Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidEndedDate()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(endedDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidReviewNotes()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(reviewNotes: "").Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidManagerName()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(managerName: _faker.Random.String2(101)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidDiscussedWithManagerDate()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(discussedWithManagerDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }
        #endregion
    }
}
