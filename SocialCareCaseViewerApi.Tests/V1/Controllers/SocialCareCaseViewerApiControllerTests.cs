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
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class SocialCareCaseViewerApiControllerTests
    {
        private SocialCareCaseViewerApiController _classUnderTest;
        private Mock<IAllocationsUseCase> _mockAllocationsUseCase;
        private Mock<ICaseNotesUseCase> _mockCaseNotesUseCase;
        private Mock<IVisitsUseCase> _mockVisitsUseCase;
        private Mock<IWarningNoteUseCase> _mockWarningNoteUseCase;
        private Mock<IGetVisitByVisitIdUseCase> _mockGetVisitByVisitIdUseCase;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockAllocationsUseCase = new Mock<IAllocationsUseCase>();
            _mockCaseNotesUseCase = new Mock<ICaseNotesUseCase>();
            _mockVisitsUseCase = new Mock<IVisitsUseCase>();
            _mockWarningNoteUseCase = new Mock<IWarningNoteUseCase>();
            _mockGetVisitByVisitIdUseCase = new Mock<IGetVisitByVisitIdUseCase>();

            _classUnderTest = new SocialCareCaseViewerApiController(_mockAllocationsUseCase.Object, _mockCaseNotesUseCase.Object, _mockVisitsUseCase.Object,
                _mockWarningNoteUseCase.Object, _mockGetVisitByVisitIdUseCase.Object);
        }

        [Test]
        public void CreateAllocationReturns201WhenSuccessful()
        {
            var request = TestHelpers.CreateAllocationRequest().Item1;
            var responseObject = new CreateAllocationResponse();

            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request))
                .Returns(responseObject);

            var response = _classUnderTest.CreateAllocation(request) as CreatedAtActionResult;

            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(responseObject);
        }

        [Test]
        public void CreateAllocationReturns404WhenCreatingAllocationFails()
        {
            var request = TestHelpers.CreateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request)).Throws(new CreateAllocationException("Worker details cannot be found"));

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(404);
        }

        [Test]
        public void CreateAllocationReturns500WhenCaseNoteFails()
        {
            var request = TestHelpers.CreateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecutePost(request)).Throws(new UpdateAllocationException("Unable to create a case note. Allocation not created"));

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidMosaicId()
        {
            var request = TestHelpers.CreateAllocationRequest(mosaicId: 0).Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidAllocatedWorkerId()
        {
            var request = TestHelpers.CreateAllocationRequest(workerId: 0).Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidAllocatedTeamId()
        {
            var request = TestHelpers.CreateAllocationRequest(teamId: 0).Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateAllocationReturns400WhenInvalidCreatedBy()
        {
            var request = TestHelpers.CreateAllocationRequest(createdBy: "").Item1;

            var response = _classUnderTest.CreateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns200WhenSuccessful()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest().Item1;
            var result = new UpdateAllocationResponse() { CaseNoteId = _fixture.Create<string>() };

            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Returns(result);

            var response = _classUnderTest.UpdateAllocation(request) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(result);
        }

        [Test]
        public void UpdateAllocationReturns400WhenUpdateFails()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Throws(new EntityUpdateException("Unable to update allocation"));

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400OnUpdateAllocationException()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest().Item1;
            _mockAllocationsUseCase.Setup(x => x.ExecuteUpdate(It.IsAny<UpdateAllocationRequest>())).Throws(new UpdateAllocationException("Unable to update allocation"));

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenInvalidMosaicId()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(id: 0).Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenEmptyDeallocationReason()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(deallocationReason: "").Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenCreatedByNotValidEmail()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(createdBy: "invalidEmail").Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateAllocationReturns400WhenDeallocationDateInTheFuture()
        {
            var request = TestHelpers.CreateUpdateAllocationRequest(deallocationDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.UpdateAllocation(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void ListCaseNotesByPersonIdReturns200WhenSuccessful()
        {
            var request = new ListCaseNotesRequest { Id = "1" };
            var notesList = _fixture.Create<ListCaseNotesResponse>();
            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetByPersonId(It.IsAny<string>())).Returns(notesList);

            var response = _classUnderTest.ListCaseNotes(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(notesList);
        }

        [Test]
        public void GettingASingleCaseNoteByNoteIdReturns200WhenSuccessful()
        {
            var request = new GetCaseNotesRequest { Id = "1" };
            var note = _fixture.Create<CaseNoteResponse>();
            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetById(It.IsAny<string>())).Returns(note);

            var response = _classUnderTest.GetCaseNoteById(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(note);
        }

        [Test]
        public void GetCaseNotesByNoteIdReturns404WhenNoMatchingCaseNoteId()
        {
            var request = new GetCaseNotesRequest { Id = "1" };
            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetById(It.IsAny<string>()))
                .Throws(new SocialCarePlatformApiException("404"));

            var response = _classUnderTest.GetCaseNoteById(request) as StatusCodeResult;

            response?.StatusCode.Should().Be(404);
        }

        [Test]
        public void GetCaseNotesByNoteIdReturns500WhenSocialCarePlatformApiExceptionIs500()
        {
            var request = new GetCaseNotesRequest { Id = "1" };
            _mockCaseNotesUseCase.Setup(x => x.ExecuteGetById(It.IsAny<string>()))
                .Throws(new SocialCarePlatformApiException("500"));

            var response = _classUnderTest.GetCaseNoteById(request) as StatusCodeResult;

            response?.StatusCode.Should().Be(500);
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

        [Test]
        public void ListVisitsByPersonIdReturns200WhenSuccessful()
        {
            var request = new ListVisitsRequest { Id = "1" };
            var visitList = new List<Visit>();
            _mockVisitsUseCase.Setup(x => x.ExecuteGetByPersonId(It.IsAny<string>())).Returns(visitList);

            var response = _classUnderTest.ListVisits(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(visitList);
        }

        [Test]
        public void GetVisitByVisitIdReturns200StatusAndVisitWhenSuccessful()
        {
            var visit = TestHelpers.CreateVisit();
            _mockGetVisitByVisitIdUseCase.Setup(x => x.Execute(visit.VisitId)).Returns(visit);

            var response = _classUnderTest.GetVisitByVisitId(visit.VisitId) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(visit);
        }

        [Test]
        public void GetVisitByVisitIdReturns404StatusAndNullWhenUnsuccessful()
        {
            var response = _classUnderTest.GetVisitByVisitId(1L) as NotFoundResult;

            response?.StatusCode.Should().Be(404);
        }

        [Test]
        public void PostWarningNoteReturns201WhenSuccessful()
        {
            var postWarningNoteResponse = _fixture.Create<PostWarningNoteResponse>();
            var postWarningNoteRequest = new PostWarningNoteRequest();
            _mockWarningNoteUseCase
                .Setup(x => x.ExecutePost(postWarningNoteRequest))
                .Returns(postWarningNoteResponse);

            var response = _classUnderTest.PostWarningNote(postWarningNoteRequest) as CreatedAtActionResult;

            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(postWarningNoteResponse);
        }

        [Test]
        public void PostWarningNoteReturns400WhenPersonNotFoundExceptionThrown()
        {
            _mockWarningNoteUseCase
                .Setup(x => x.ExecutePost(It.IsAny<PostWarningNoteRequest>()))
                .Throws(new PersonNotFoundException("Warning Note could not be inserted"));

            var response = _classUnderTest.PostWarningNote(new PostWarningNoteRequest()) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Warning Note could not be inserted");
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

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(stubbedWarningNotesResponse);
        }

        [Test]
        public void GetWarningNoteWouldReturns200IfNoWarningNotesAreFoundForThePerson()
        {
            var testPersonId = _fixture.Create<long>();
            var emptyWarningNotesResponse = new ListWarningNotesResponse
            {
                WarningNotes = new List<WarningNote>()
            };
            _mockWarningNoteUseCase
                .Setup(x => x.ExecuteGet(It.IsAny<long>()))
                .Returns(emptyWarningNotesResponse);

            var response = _classUnderTest.ListWarningNotes(testPersonId) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(emptyWarningNotesResponse);
        }

        [Test]
        public void GetWarningNoteByIdReturns200WhenSuccessful()
        {
            var warningNoteId = _fixture.Create<long>();
            var stubbedResponse = _fixture.Create<WarningNoteResponse>();
            _mockWarningNoteUseCase
                .Setup(x => x.ExecuteGetWarningNoteById(It.IsAny<long>()))
                .Returns(stubbedResponse);

            var response = _classUnderTest.GetWarningNoteById(warningNoteId) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(stubbedResponse);
        }

        [Test]
        public void GetWarningNoteByIdReturns404IfNoWarningNoteIsFound()
        {
            var warningNoteId = _fixture.Create<long>();
            _mockWarningNoteUseCase.Setup(x => x.ExecuteGetWarningNoteById(It.IsAny<long>()));

            var response = _classUnderTest.GetWarningNoteById(warningNoteId) as ObjectResult;

            response.Should().BeNull();
            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be($"No warning note found for the specified ID: {warningNoteId}");
        }

        [Test]
        public void PatchWarningNoteCallsTheUseCaseAndReturns204WhenSuccessful()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest().Item1;
            var response = _classUnderTest.PatchWarningNote(request) as NoContentResult;

            _mockWarningNoteUseCase.Verify(x => x.ExecutePatch(request), Times.Once);
            response?.StatusCode.Should().Be(204);
        }

        [Test]
        public void PatchWarningNoteReturnsA400ResponseWhenPatchWarningNoteExceptionThrown()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest().Item1;

            _mockWarningNoteUseCase
                .Setup(x => x.ExecutePatch(It.IsAny<PatchWarningNoteRequest>()))
                .Throws(new PatchWarningNoteException("exception encountered"));

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("exception encountered");
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidWarningNoteId()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(warningNoteId: 0).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidReviewDate()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(reviewDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidNextReviewDateId()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(nextReviewDate: DateTime.Now).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidStatus()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "invalid").Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidReviewNotes()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(reviewNotes: "").Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidManagerName()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(managerName: _faker.Random.String2(101)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void PatchWarningNoteReturns400WhenInvalidDiscussedWithManagerDate()
        {
            var request = TestHelpers.CreatePatchWarningNoteRequest(discussedWithManagerDate: DateTime.Now.AddDays(1)).Item1;

            var response = _classUnderTest.PatchWarningNote(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }
    }
}
