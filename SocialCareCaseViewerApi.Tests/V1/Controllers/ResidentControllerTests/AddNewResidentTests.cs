using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.ResidentControllerTests
{
    [TestFixture]
    public class AddNewResidentTests
    {
        private ResidentController _residentController = null!;
        private Mock<IResidentUseCase> _mockResidentUseCase = null!;
        private Mock<ICreateRequestAuditUseCase> _mockCreateRequestAuditUseCase = null!;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockResidentUseCase = new Mock<IResidentUseCase>();
            _mockCreateRequestAuditUseCase = new Mock<ICreateRequestAuditUseCase>();
            _residentController = new ResidentController(_mockResidentUseCase.Object, _mockCreateRequestAuditUseCase.Object);
        }

        [Test]
        public void AddNewResidentReturns201WhenSuccessful()
        {
            var addNewResidentResponse = _fixture.Create<AddNewResidentResponse>();
            var addNewResidentRequest = new AddNewResidentRequest();
            _mockResidentUseCase
                .Setup(x => x.AddNewResident(addNewResidentRequest))
                .Returns(addNewResidentResponse);

            var response = _residentController.AddNewResident(addNewResidentRequest) as CreatedAtActionResult;

            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(addNewResidentResponse);
        }

        [Test]
        public void AddNewResidentReturns500WhenResidentCouldNotBeInserted()
        {
            var addNewResidentRequest = _fixture.Create<AddNewResidentRequest>();
            _mockResidentUseCase
                .Setup(x => x.AddNewResident(addNewResidentRequest))
                .Throws(new ResidentCouldNotBeInsertedException("Resident could not be inserted"));

            var response = _residentController.AddNewResident(addNewResidentRequest) as ObjectResult;

            response?.StatusCode.Should().Be(500);
            response?.Value.Should().Be("Resident could not be inserted");
        }

        [Test]
        public void AddNewResidentReturns500WhenAddressCouldNotBeInserted()
        {
            var addNewResidentRequest = _fixture.Create<AddNewResidentRequest>();
            _mockResidentUseCase
                .Setup(x => x.AddNewResident(addNewResidentRequest))
                .Throws(new AddressCouldNotBeInsertedException("Address could not be inserted"));

            var response = _residentController.AddNewResident(addNewResidentRequest) as ObjectResult;

            response?.StatusCode.Should().Be(500);
            response?.Value.Should().Be("Address could not be inserted");
        }
    }
}
