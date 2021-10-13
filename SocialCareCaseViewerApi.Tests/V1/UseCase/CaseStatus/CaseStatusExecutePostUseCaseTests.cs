using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using System;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    [TestFixture]
    public class CaseStatusExecutePostUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway;
        private CaseStatusesUseCase _caseStatusesUseCase;
        private CreateCaseStatusRequest _request;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);

            _request = CaseStatusHelper.CreateCaseStatusRequest();

            _mockDatabaseGateway
                .Setup(x => x.GetPersonByMosaicId(It.IsAny<int>()))
                .Returns(TestHelpers.CreatePerson(_request.PersonId));

            var caseStatus = CaseStatusHelper.CreateCaseStatus().ToDomain();

            _mockCaseStatusGateway
                .Setup(x => x.CreateCaseStatus(It.IsAny<CreateCaseStatusRequest>()))
                .Returns(caseStatus);
        }

        [Test]
        public void CallsGateways()
        {
            var resident = TestHelpers.CreatePerson(_request.PersonId, ageContext: "c");
            var worker = TestHelpers.CreateWorker(createdBy: _request.CreatedBy);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_request.PersonId)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(worker);

            _caseStatusesUseCase.ExecutePost(_request);

            _mockDatabaseGateway.Verify(gateway => gateway.GetPersonByMosaicId(_request.PersonId));
            _mockDatabaseGateway.Verify(gateway => gateway.GetWorkerByEmail(_request.CreatedBy));
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
        public void WhenCreatedByEmailDoesNotExistThrowsWorkerNotFoundException()
        {
            var resident = TestHelpers.CreatePerson(_request.PersonId, ageContext: "c");
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_request.PersonId)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy));

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"'createdBy' with '{_request.CreatedBy}' was not found as a worker.");
        }

        [TestCase("a")]
        [TestCase("z")]
        [Test]
        public void WhenPersonIsNotInChildAgeContextItShouldThrowInvalidAgeContextException(string ageContext)
        {
            var resident = TestHelpers.CreatePerson(_request.PersonId, ageContext: ageContext);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_request.PersonId)).Returns(resident);

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<InvalidAgeContextException>()
            .WithMessage($"Person with the id {resident.Id} belongs to the wrong AgeContext for this operation");
        }

        [Test]
        public void WhenPersonIsAdultAgeContextItShouldThrowInvalidAgeContextException()
        {
            var resident = TestHelpers.CreatePerson(_request.PersonId, ageContext: "a");
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_request.PersonId)).Returns(resident);

            Action act = () => _caseStatusesUseCase.ExecutePost(_request);

            act.Should().Throw<InvalidAgeContextException>()
            .WithMessage($"Person with the id {resident.Id} belongs to the wrong AgeContext for this operation");
        }
    }
}
