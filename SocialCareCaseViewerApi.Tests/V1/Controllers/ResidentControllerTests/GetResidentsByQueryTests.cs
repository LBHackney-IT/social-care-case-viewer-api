using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.ResidentControllerTests
{
    [TestFixture]
    public class GetResidentsByQueryTests
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
        public void ListContactsReturns200WhenSuccessful()
        {
            var residentInformationList = _fixture.Create<ResidentInformationList>();
            var residentQueryParam = new ResidentQueryParam();
            _mockResidentUseCase
                .Setup(x => x.GetResidentsByQuery(residentQueryParam, 2, 3))
                .Returns(residentInformationList);

            var response = _residentController.ListContacts(residentQueryParam, 2, 3) as OkObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(residentInformationList);
        }
    }
}
