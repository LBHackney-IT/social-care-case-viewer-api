using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
        private Mock<IWorkersUseCase> _mockWorkersUseCase;
        private Mock<ITeamsUseCase> _mockTeamsUseCase;

        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockGetAllUseCase = new Mock<IGetAllUseCase>();
            _mockAddNewResidentUseCase = new Mock<IAddNewResidentUseCase>();
            _mockProcessDataUseCase = new Mock<IProcessDataUseCase>();
            _mockAllocationsUseCase = new Mock<IAllocationsUseCase>();
            _mockWorkersUseCase = new Mock<IWorkersUseCase>();
            _mockTeamsUseCase = new Mock<ITeamsUseCase>();

            _classUnderTest = new SocialCareCaseViewerApiController(_mockGetAllUseCase.Object, _mockAddNewResidentUseCase.Object,
            _mockProcessDataUseCase.Object, _mockAllocationsUseCase.Object, _mockWorkersUseCase.Object, _mockTeamsUseCase.Object);
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

        #region Workers
        [Test]
        public void GetWorkersReturns200WhenSuccessful()
        {
            var request = new ListWorkersRequest() { TeamId = 5 };

            var workersList = _fixture.Create<ListWorkersResponse>();

            _mockWorkersUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListWorkersRequest>())).Returns(workersList);

            var response = _classUnderTest.ListWorkers(request);

            response.Should().NotBeNull();
        }
        #endregion

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
    }
}
