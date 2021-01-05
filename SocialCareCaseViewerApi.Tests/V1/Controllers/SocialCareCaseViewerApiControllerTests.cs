using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Domain;
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
        private Mock<IGetChildrenAllocationUseCase> _mockGetChildrenAllocationUseCase;
        private Mock<IGetAdultsAllocationsUseCase> _mockGetAdultsAllocationsUseCase;

        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockGetAllUseCase = new Mock<IGetAllUseCase>();
            _mockAddNewResidentUseCase = new Mock<IAddNewResidentUseCase>();
            _mockProcessDataUseCase = new Mock<IProcessDataUseCase>();
            _mockGetChildrenAllocationUseCase = new Mock<IGetChildrenAllocationUseCase>();
            _mockGetAdultsAllocationsUseCase = new Mock<IGetAdultsAllocationsUseCase>();
            _classUnderTest = new SocialCareCaseViewerApiController(_mockGetAllUseCase.Object, _mockAddNewResidentUseCase.Object,
                _mockProcessDataUseCase.Object, _mockGetChildrenAllocationUseCase.Object, _mockGetAdultsAllocationsUseCase.Object);
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

        // To Do: Add 400 response for invalid or missing parameters

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

        // [Test]
        // public void ListCasesReturns400WhenStartDateIsInvalid()
        // {
        //     var badDateRequest = new ListCasesRequest
        //     {
        //         StartDate = "Bad Date"
        //     };

        //     var response = _classUnderTest.ListCases(new ListCasesRequest());

        //     response.Should().NotBeNull();
        // response.StatusCode.Should().Be(400);
        // response.Value.Should().Be("Invalid start date");
        // }

        // [Test]
        // public void ListCasesReturns400WhenEndDateIsInvalid()
        // {
        //     var badDateRequest = new ListCasesRequest
        //     {
        //         EndDate = "Bad Date"
        //     };

        //     var response = _classUnderTest.ListCases(new ListCasesRequest()) as BadRequestObjectResult;

        //     response.Should().NotBeNull();
        //     response.StatusCode.Should().Be(400);
        //     response.Value.Should().Be("Invalid end date");
        // }

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
        public void GetChildrensAllocatedWorkerReturns200WhenSuccessful()
        {
            var childAllocationList = _fixture.Create<CfsAllocationList>();
            var listAllocationsRequest = new ListAllocationsRequest();

            _mockGetChildrenAllocationUseCase.Setup(x => x.Execute(listAllocationsRequest)).Returns(childAllocationList);
            var response = _classUnderTest.GetChildrensAllocatedWorker(listAllocationsRequest) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(childAllocationList);
        }

        //To Do: add 404 response for no allocations found

        [Test]
        public void GetAdultsAllocatedWorkerReturns200WhenSuccessful()
        {
            var adultAllocationList = _fixture.Create<AscAllocationList>();
            var listAcsAllocationRequest = new ListAscAllocationsRequest();

            _mockGetAdultsAllocationsUseCase.Setup(x => x.Execute(listAcsAllocationRequest)).Returns(adultAllocationList);
            var response = _classUnderTest.GetAdultsAllocatedWorker(listAcsAllocationRequest) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(adultAllocationList);
        }

        //To Do: add 404 response for no allocations found

        // [Test]
        // public async Task CreateCaseNoteReturns201WhenSuccessful()
        // {
        //     var taskResponse = _fixture.Create<string>();
        //     var caseNotesDocument = _fixture.Create<CaseNotesDocument>();

        //     _mockProcessDataUseCase.Setup(x => x.Execute(caseNotesDocument)).Returns(Task.FromResult(taskResponse));
        //     var response = await _classUnderTest.CreateCaseNote(caseNotesDocument).ConfigureAwait(true) as CreatedAtActionResult;

        //     response.Should().NotBeNull();
        //     response.StatusCode.Should().Be(201);
        //     response.Value.Should().BeEquivalentTo(taskResponse);
        // }

        // To Do: add 400 response for invalid or missing parameters
        // To Do: Add 500 response for problem generating token
    }
}
