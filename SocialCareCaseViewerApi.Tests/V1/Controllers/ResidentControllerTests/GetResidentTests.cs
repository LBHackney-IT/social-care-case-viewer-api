using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.ResidentControllerTests
{
    [TestFixture]
    public class GetResidentTests
    {
        private ResidentController _residentController = null!;
        private Mock<IResidentUseCase> _mockResidentUseCase = null!;
        private Mock<ICreateRequestAuditUseCase> _mockCreateRequestAuditUseCase = null!;
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockResidentUseCase = new Mock<IResidentUseCase>();
            _mockCreateRequestAuditUseCase = new Mock<ICreateRequestAuditUseCase>();
            _residentController = new ResidentController(_mockResidentUseCase.Object, _mockCreateRequestAuditUseCase.Object);
        }

        [Test]
        public void GetPersonByIdReturns200WhenSuccessful()
        {
            var request = new GetPersonRequest();
            _mockResidentUseCase
                .Setup(x => x.GetResident(request))
                .Returns(new GetPersonResponse());

            var response = _residentController.GetPerson(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void GetPersonByIdDoesNotCallTheCreateRequestAuditUseCaseWhenAuditingIsEnabledIsFalse()
        {
            var request = new GetPersonRequest()
            {
                AuditingEnabled = false,
                UserId = _faker.Person.Email,
                Id = 7
            };

            _residentController.GetPerson(request);

            _mockCreateRequestAuditUseCase.Verify(x => x.Execute(It.IsAny<CreateRequestAuditRequest>()), Times.Never);
        }

        [Test]
        public void GetPersonByIdCallsTheCreateRequestAuditUseCaseWhenAuditingIsEnabledIsTrueAndUserIdIsProvided()
        {
            var getPersonRequest = new GetPersonRequest() { AuditingEnabled = true, UserId = _faker.Person.Email, Id = 1 };

            _residentController.GetPerson(getPersonRequest);

            _mockCreateRequestAuditUseCase.Verify(x => x.Execute(It.IsAny<CreateRequestAuditRequest>()), Times.Once);
        }

        [Test]
        public void GetPersonByIdCallsTheCreateRequestAuditUseCaseWithCorrectValuesWhenAuditingIsEnabledIsTrueAndUserIdIsProvided()
        {
            var getPersonRequest = new GetPersonRequest() { AuditingEnabled = true, UserId = _faker.Person.Email, Id = 1 };

            var request = new CreateRequestAuditRequest()
            {
                ActionName = "view_resident",
                UserName = getPersonRequest.UserId,
                Metadata = new Dictionary<string, object>() { { "residentId", getPersonRequest.Id } }
            };

            _mockCreateRequestAuditUseCase.Setup(x => x.Execute(request)).Verifiable();

            _residentController.GetPerson(getPersonRequest);

            _mockCreateRequestAuditUseCase.Verify(x => x.Execute(It.Is<CreateRequestAuditRequest>(
                y => y.ActionName == request.ActionName
                && y.UserName == request.UserName
                && JsonConvert.SerializeObject(y.Metadata) == JsonConvert.SerializeObject(request.Metadata)
                )), Times.Once);
        }

        [Test]
        public void GetPersonByIdCallsPersonUseCaseWhenOnlyIdIsProvidedToEnsureBackwardsCompatibility()
        {
            var getPersonRequest = new GetPersonRequest { Id = 1 };

            _residentController.GetPerson(getPersonRequest);

            _mockResidentUseCase.Verify(x => x.GetResident(getPersonRequest));
        }

        [Test]
        public void GetPersonByIdReturns404WhenPersonNotFound()
        {
            var request = new GetPersonRequest();
            _mockResidentUseCase.Setup(x => x.GetResident(request));

            var result = _residentController.GetPerson(request) as ObjectResult;

            result?.StatusCode.Should().Be(404);
        }
    }
}
