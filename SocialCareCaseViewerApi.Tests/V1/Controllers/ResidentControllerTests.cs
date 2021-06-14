using System.Collections.Generic;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ResidentControllerTests
    {
        private ResidentController _residentController;
        private Mock<IResidentsUseCase> _mockResidentsUseCase;
        private Mock<IWarningNoteUseCase> _mockWarningNoteUseCase;
        private Mock<IRelationshipsUseCase> _mockRelationshipsUseCase;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockResidentsUseCase = new Mock<IResidentsUseCase>();
            _mockWarningNoteUseCase = new Mock<IWarningNoteUseCase>();
            _mockRelationshipsUseCase = new Mock<IRelationshipsUseCase>();

            _residentController = new ResidentController(_mockResidentsUseCase.Object, _mockWarningNoteUseCase.Object,
                _mockRelationshipsUseCase.Object);
        }

        [Test]
        public void GetResidentsReturns200WhenSuccessful()
        {
            var residentInformationList = _fixture.Create<ResidentInformationList>();
            var residentQueryParam = new ResidentQueryParam();

            _mockResidentsUseCase.Setup(x => x.ExecuteGetAll(residentQueryParam, 2, 3)).Returns(residentInformationList);
            var response = _residentController.GetResidents(residentQueryParam, 2, 3) as OkObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(residentInformationList);
        }

        [Test]
        public void GetResidentsReturns400WhenQueryParametersAreInvalid()
        {
            _mockResidentsUseCase.Setup(x => x.ExecuteGetAll(It.IsAny<ResidentQueryParam>(), 2, 3))
                .Throws(new InvalidQueryParameterException("Invalid Parameters"));

            var response = _residentController.GetResidents(new ResidentQueryParam(), 2, 3) as BadRequestObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Invalid Parameters");
        }

        [Test]
        public void AddNewResidentReturns201WhenSuccessful()
        {
            var addNewResidentResponse = _fixture.Create<AddNewResidentResponse>();
            var addNewResidentRequest = new AddNewResidentRequest();

            _mockResidentsUseCase.Setup(x => x.ExecutePost(addNewResidentRequest)).Returns(addNewResidentResponse);
            var response = _residentController.AddNewResident(addNewResidentRequest) as CreatedAtActionResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(addNewResidentResponse);
        }

        [Test]
        public void AddNewResidentReturns500WhenResidentCouldNotBeInserted()
        {
            _mockResidentsUseCase.Setup(x => x.ExecutePost(It.IsAny<AddNewResidentRequest>()))
                .Throws(new ResidentCouldNotBeinsertedException("Resident could not be inserted"));

            var response = _residentController.AddNewResident(new AddNewResidentRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(500);
            response?.Value.Should().Be("Resident could not be inserted");
        }

        [Test]
        public void AddNewResidentReturns500WhenAddressCouldNotBeInserted()
        {
            _mockResidentsUseCase.Setup(x => x.ExecutePost(It.IsAny<AddNewResidentRequest>()))
                .Throws(new AddressCouldNotBeInsertedException("Address could not be inserted"));

            var response = _residentController.AddNewResident(new AddNewResidentRequest()) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(500);
            response?.Value.Should().Be("Address could not be inserted");
        }

        [Test]
        public void GetResidentByIdReturns200WhenSuccessful()
        {
            _mockResidentsUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Returns(new GetResidentResponse());

            var response = _residentController.GetResident(1L) as ObjectResult;

            response?.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetResidentByIdReturns404WhenResidentNotFound()
        {
            _mockResidentsUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>()));

            var result = _residentController.GetResident(1L) as NotFoundResult;

            result?.StatusCode.Should().Be(404);
        }

        [Test]
        public void UpdateResidentReturns200WhenSuccessful()
        {
            var request = new UpdateResidentRequest();

            var result = _residentController.UpdateResident(request) as StatusCodeResult;

            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(204);
        }

        [Test]
        public void UpdateResidentReturns404WhenResidentNotFound()
        {
            var request = new UpdateResidentRequest();
            _mockResidentsUseCase.Setup(x => x.ExecutePatch(It.IsAny<UpdateResidentRequest>())).Throws(new UpdateResidentException("Resident not found"));

            var result = _residentController.UpdateResident(request) as NotFoundObjectResult;

            result.Should().NotBeNull();
            result?.StatusCode.Should().Be(404);
            result?.Value.Should().Be("Resident not found");
        }

        [Test]
        public void GetWarningNoteReturns200AndGetWarningNoteResponseWhenSuccessful()
        {
            var testResidentId = _fixture.Create<long>();
            var stubbedWarningNotesResponse = _fixture.Create<ListWarningNotesResponse>();
            _mockWarningNoteUseCase
                .Setup(x => x.ExecuteGet(It.IsAny<long>()))
                .Returns(stubbedWarningNotesResponse);

            var response = _residentController.ListWarningNotes(testResidentId) as OkObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(stubbedWarningNotesResponse);
        }

        [Test]
        public void GetWarningNoteReturns200IfNoWarningNotesAreFoundForTheResident()
        {
            var testResidentId = _fixture.Create<long>();
            var emptyWarningNotesResponse = new ListWarningNotesResponse
            {
                WarningNotes = new List<WarningNote>()
            };
            _mockWarningNoteUseCase
                .Setup(x => x.ExecuteGet(It.IsAny<long>()))
                .Returns(emptyWarningNotesResponse);

            var response = _residentController.ListWarningNotes(testResidentId) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(emptyWarningNotesResponse);
        }

        [Test]
        public void ListRelationshipsReturn200WhenResidentIsFound()
        {
            var request = new ListRelationshipsRequest() { PersonId = _faker.Random.Long() };
            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsRequest>())).Returns(new ListRelationshipsResponse());

            var response = _residentController.ListRelationships(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void ListRelationshipsReturn404WithCorrectErrorMessageWhenPersonIsNotFound()
        {
            var request = new ListRelationshipsRequest() { PersonId = _faker.Random.Long() };
            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsRequest>())).Throws(new GetRelationshipsException("Person not found"));

            var response = _residentController.ListRelationships(request) as NotFoundObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListRelationshipsReturns200AndRelationshipsWhenSuccessful()
        {
            var request = new ListRelationshipsRequest() { PersonId = _faker.Random.Long() };
            var listRelationShipsResponse = _fixture.Create<ListRelationshipsResponse>();
            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsRequest>())).Returns(listRelationShipsResponse);

            var response = _residentController.ListRelationships(request) as ObjectResult;

            response?.Value.Should().BeEquivalentTo(listRelationShipsResponse);
        }
    }
}
