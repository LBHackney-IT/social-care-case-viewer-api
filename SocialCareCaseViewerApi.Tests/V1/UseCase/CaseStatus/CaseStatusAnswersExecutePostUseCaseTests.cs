using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using CaseStatusDomain = SocialCareCaseViewerApi.V1.Domain.CaseStatus;
using CaseStatusInfrastructure = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    [TestFixture]
    public class CaseStatusAnswersExecutePostUseCaseTests
    {
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        private CaseStatusesUseCase _caseStatusesUseCase = null!;
        private CreateCaseStatusAnswerRequest _request = null!;
        private CaseStatusDomain _caseStatus = null!;
        private Worker _worker = null!;
        private readonly string _answerGroupId1 = Guid.NewGuid().ToString();
        private CaseStatusInfrastructure _lacCaseStatus = null!;

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);
            _request = CaseStatusHelper.CreateCaseStatusAnswerRequest();
            _caseStatus = CaseStatusHelper.CreateCaseStatus(type: "LAC").ToDomain();
            _worker = TestHelpers.CreateWorker();
            _lacCaseStatus = TestHelpers.CreateCaseStatus(type: "LAC");

        }

        [Test]
        public void UseCaseCallsCaseStatusGateway()
        {
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_request.CaseStatusId)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.ReplaceCaseStatusAnswers(_request)).Returns(_caseStatus);

            _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            _mockCaseStatusGateway.Verify(gw => gw.ReplaceCaseStatusAnswers(_request));
        }

        [Test]
        public void UseCaseCallsDataBaseGatewayToCheckWorkerExists()
        {
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_request.CaseStatusId)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.ReplaceCaseStatusAnswers(_request)).Returns(_caseStatus);

            _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(_request.CreatedBy));
        }

        [Test]
        public void UseCaseCallsCaseStatusGatewayToGetCaseStatus()
        {
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>()));
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_request.CaseStatusId)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.ReplaceCaseStatusAnswers(_request)).Returns(_caseStatus);

            _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            _mockCaseStatusGateway.Verify(x => x.GetCasesStatusByCaseStatusId(_request.CaseStatusId));
        }

        [Test]
        public void WhenCaseStatusDoesNotExistThrowsCaseStatusDoesNotExistException()
        {
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(default(CaseStatusDomain));

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<CaseStatusDoesNotExistException>().WithMessage($"Case status with {_request.CaseStatusId} not found");
        }

        [Test]
        public void WhenCreatedByEmailDoesNotExistThrowsWorkerNotFoundException()
        {
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(new CaseStatusDomain() { Type = "LAC" });

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<WorkerNotFoundException>().WithMessage($"Worker with email `{_request.CreatedBy}` was not found");
        }

        [Test]
        [TestCase("CP")]
        [TestCase("CIN")]
        public void ThrowsAnExceptionWhenCaseStatusTypeIsNotLAC(string caseStatusType)
        {
            var caseStatus = TestHelpers.CreateCaseStatus(type: caseStatusType);

            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<InvalidCaseStatusTypeException>().WithMessage($"Answers can only be added to LAC statuses");
        }

        [Test]
        public void WhenStartDateIsBeforeTheCurrentAnswersStartDateItThrowsAnInvalidCaseStatusAnswersStartDateException()
        {
            var caseStatusAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: _lacCaseStatus.Id, groupId: _answerGroupId1, startDate: DateTime.Today.AddDays(-10), min: 2, max: 2);
            _lacCaseStatus.Answers.AddRange(caseStatusAnswers);

            _request.StartDate = DateTime.Today.AddDays(-20);

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_lacCaseStatus.ToDomain());
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);

            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_caseStatus);

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<InvalidCaseStatusAnswersStartDateException>().WithMessage($"Start date cannot be before the current active date");
        }


        [Test]
        public void WhenTypeIsLACandTheProvidedStarDateIsValidAndThereAreScheduledAnswersItCallsTheGateway()
        {
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: new DateTime(2000, 01, 11), endDate: new DateTime(2040, 02, 01));
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: new DateTime(2040, 02, 01));

            _lacCaseStatus.Answers.AddRange(activeAnswers);
            _lacCaseStatus.Answers.AddRange(scheduledAnswers);

            _request.StartDate = new DateTime(2000, 01, 11);

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_lacCaseStatus.ToDomain());
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.ReplaceCaseStatusAnswers(_request)).Returns(_caseStatus);
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_caseStatus);
            _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            _mockCaseStatusGateway.Verify(x => x.GetCasesStatusByCaseStatusId(_request.CaseStatusId));
        }

        [Test]
        public void WhenStartDateIsOnTheCurrentAnswersStartDateItThrowsAnInvalidCaseStatusAnswersStartDateException()
        {
            var date = DateTime.Today.AddDays(-10);

            var caseStatusAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: _lacCaseStatus.Id, groupId: _answerGroupId1, startDate: date, min: 2, max: 2);
            _lacCaseStatus.Answers.AddRange(caseStatusAnswers);

            _request.StartDate = date;

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_lacCaseStatus.ToDomain());
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_caseStatus);

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<InvalidCaseStatusAnswersStartDateException>().WithMessage($"Start date cannot be before the current active date");
        }
    }
}
