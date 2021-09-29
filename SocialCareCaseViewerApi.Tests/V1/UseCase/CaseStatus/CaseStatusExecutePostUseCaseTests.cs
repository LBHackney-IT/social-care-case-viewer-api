using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    [TestFixture]
    public class CaseStatusExecutePostUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway;
        private CaseStatusesUseCase _caseStatusesUseCase;
        private CreateCaseStatusRequest _request;
        private readonly CaseStatusType _typeInRequest = TestHelpers.CreateCaseStatusType();

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);

            _mockCaseStatusGateway
                .Setup(x => x.GetCaseStatusTypeWithFields(_typeInRequest.Name))
                .Returns(_typeInRequest);

            _request = CaseStatusHelper.CreateCaseStatusRequest(type: _typeInRequest.Name);

            _mockDatabaseGateway
                .Setup(x => x.GetPersonByMosaicId(It.IsAny<int>()))
                .Returns(TestHelpers.CreatePerson(_request.PersonId));

            var caseStatus = CaseStatusHelper.CreateCaseStatus().ToDomain();

            _mockCaseStatusGateway
                .Setup(x => x.CreateCaseStatus(It.IsAny<CreateCaseStatusRequest>()))
                .Returns(caseStatus);

            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>()))
                .Returns(TestHelpers.CreateWorker(email: _request.CreatedBy));
        }

        [Test]
        public void CallsGatewaysToCheckCaseStatusExists()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(TestHelpers.CreatePerson(_request.PersonId));

            _caseStatusesUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonByMosaicId(_request.PersonId));
            _mockDatabaseGateway.Verify(gateway => gateway.GetWorkerByEmail(_request.CreatedBy));
            _mockCaseStatusGateway.Verify(gateway => gateway.GetCaseStatusTypeWithFields(_request.Type));
            _mockCaseStatusGateway.Verify(gateway => gateway.CreateCaseStatus(_request));
        }

        [Test]
        public void WhenPersonDoesNotExistThrowsPersonNotFoundException()
        {
            const long nonExistingPersonId = 1L;
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(nonExistingPersonId));

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"'personId' with '{_request.PersonId}' was not found.");
        }
        [Test]
        public void WhenTypeDoesNotExistThrowsTypeNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(TestHelpers.CreatePerson(_request.PersonId));
            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusTypeWithFields(It.IsAny<string>()));

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<CaseStatusTypeNotFoundException>();
        }

        [Test]
        public void WhenCreatedByEmailDoesNotExistThrowsWorkerNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(TestHelpers.CreatePerson(_request.PersonId));
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>()));

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"'createdBy' with '{_request.CreatedBy}' was not found as a worker.");
        }
    }
}
