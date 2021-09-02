using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Collections.Generic;
using System;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Relationships
{
    [TestFixture]
    public class CaseStatusExecutePostUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private CaseStatusesUseCase _caseStatusesUseCase;
        private CreateCaseStatusRequest _request;
        private readonly CaseStatusType _typeInRequest = TestHelpers.CreateCaseStatusType();

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockDatabaseGateway.Object);

            _mockDatabaseGateway.Setup(x => x.GetCaseStatusTypeWithFields(_typeInRequest.Name))
                .Returns(_typeInRequest);

            _request = CaseStatusHelper.CreateCaseStatusRequest(type: _typeInRequest.Name);

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<int>()))
                .Returns(TestHelpers.CreatePerson((int) _request.PersonId));

            CaseStatus _caseStatus = CaseStatusHelper.CreateCaseStatus();

            _mockDatabaseGateway.Setup(x => x.CreateCaseStatus(It.IsAny<CreateCaseStatusRequest>()))
                .Returns(_caseStatus);

            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>()))
                .Returns(TestHelpers.CreateWorker(email: _request.CreatedBy));
        }

        [Test]
        public void CallsDatabaseGatewayToCheckPersonExists()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(TestHelpers.CreatePerson((int) _request.PersonId));

            _caseStatusesUseCase.ExecutePost(_request);
            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonByMosaicId(_request.PersonId));
        }

        [Test]
        public void WhenPersonDoesNotExistThrowsPersonNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>()));

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"'personId' with '{_request.PersonId}' was not found.");
        }
        [Test]
        public void WhenTypeDoesNotExistThrowsTypeNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(TestHelpers.CreatePerson((int) _request.PersonId));
            _mockDatabaseGateway.Setup(x => x.GetCaseStatusTypeWithFields(It.IsAny<string>()));

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<CaseStatusTypeNotFoundException>();
        }

        [Test]
        public void WhenCreatedByEmailDoesNotExistThrowsWorkerNotFoundException()
        {
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(TestHelpers.CreatePerson((int) _request.PersonId));
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>()))
                .Returns((Worker) null);

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"'createdBy' with '{_request.CreatedBy}' was not found as a worker.");
        }
    }
}
