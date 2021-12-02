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
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus.CP
{
    [TestFixture]
    public class CaseStatusAnswersExecutePostUseCaseTests
    {
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        private CaseStatusesUseCase _caseStatusesUseCase = null!;
        private CreateCaseStatusAnswerRequest _request = null!;
        private CaseStatusDomain _cpCaseStatusDomain = null!;
        private Worker _worker = null!;
        private readonly string _answerGroupId1 = Guid.NewGuid().ToString();
        private CaseStatusInfrastructure _cpCaseStatus = null!;
        private List<CaseStatusRequestAnswers> _answers = null!;

        [SetUp]
        public void SetUp()
        {
            _answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 1, max: 1);
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);
            _request = CaseStatusHelper.CreateCaseStatusAnswerRequest(answers: _answers);
            _cpCaseStatusDomain = CaseStatusHelper.CreateCaseStatus(type: "CP").ToDomain();
            _worker = TestHelpers.CreateWorker();
            _cpCaseStatus = TestHelpers.CreateCaseStatus(type: "CP");
        }

        [Test]
        public void WhenTypeIsCPAndStartDateIsBeforeTheCurrentAnswersStartDateItThrowsAnInvalidCaseStatusAnswersStartDateException()
        {
            var caseStatusAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: _cpCaseStatus.Id, groupId: _answerGroupId1, startDate: DateTime.Today.AddDays(-10), min: 1, max: 1);
            _cpCaseStatus.Answers.AddRange(caseStatusAnswers);

            _request.StartDate = DateTime.Today.AddDays(-20);

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_cpCaseStatus.ToDomain());
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);

            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_cpCaseStatusDomain);

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<InvalidCaseStatusAnswersStartDateException>().WithMessage($"Start date cannot be before the current active date for CP");
        }

        [Test]
        public void WhenTypeIsCPandTheProvidedStarDateIsValidAndThereAreScheduledAnswersItCallsTheGateway()
        {
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: new DateTime(2000, 01, 11), endDate: new DateTime(2040, 02, 01));
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: new DateTime(2040, 02, 01));

            _cpCaseStatus.Answers.AddRange(activeAnswers);
            _cpCaseStatus.Answers.AddRange(scheduledAnswers);

            _request.StartDate = new DateTime(2000, 01, 11);

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_cpCaseStatus.ToDomain());
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.ReplaceCaseStatusAnswers(_request)).Returns(_cpCaseStatusDomain);
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_cpCaseStatusDomain);
            _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            _mockCaseStatusGateway.Verify(x => x.GetCasesStatusByCaseStatusId(_request.CaseStatusId));
        }

        [Test]
        public void WhenTypeIsCPAndStartDateIsOnTheCurrentAnswersStartDateItThrowsAnInvalidCaseStatusAnswersStartDateException()
        {
            var date = DateTime.Today.AddDays(-10);

            var caseStatusAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: _cpCaseStatus.Id, groupId: _answerGroupId1, startDate: date, min: 1, max: 1);
            _cpCaseStatus.Answers.AddRange(caseStatusAnswers);

            _request.StartDate = date;

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_cpCaseStatus.ToDomain());
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_request.CreatedBy)).Returns(_worker);
            _mockCaseStatusGateway.Setup(x => x.CreateCaseStatusAnswer(_request)).Returns(_cpCaseStatusDomain);

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<InvalidCaseStatusAnswersStartDateException>().WithMessage($"Start date cannot be before the current active date for CP");
        }

        [Test]
        public void WhenTypeIsCPAndProvidedAnswersCountIsNotOneItThrowsInvalidCaseStatusAnswersRequestException()
        {
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_cpCaseStatus.ToDomain());

            _request.Answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 3, max: 5);

            Action act = () => _caseStatusesUseCase.ExecutePostCaseStatusAnswer(_request);

            act.Should().Throw<InvalidCaseStatusAnswersRequestException>().WithMessage("CP must have only one answer");
        }
    }
}
