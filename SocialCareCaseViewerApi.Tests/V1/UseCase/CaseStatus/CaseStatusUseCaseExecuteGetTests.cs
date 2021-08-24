using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.Tests.V1.Helpers.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    public class CaseStatusUseCaseExecuteGetTests
    {
        private readonly Mock<IDatabaseGateway> _mockDatabaseGateway = new Mock<IDatabaseGateway>();
        private readonly Mock<ICaseStatusGateway> _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
        private CaseStatusUseCase _caseStatusUseCase;

        [SetUp]
        public void SetUp()
        {
            _caseStatusUseCase = new CaseStatusUseCase(_mockDatabaseGateway.Object, _mockCaseStatusGateway.Object);
        }

        [Test]
        public void WhenAPersonDoesNotExist()
        {
            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(1234));

            _caseStatusUseCase.Invoking(x => x.ExecuteGet(1234))
                .Should().Throw<GetCaseStatusesException>()
                .WithMessage("Person not found");
        }


        [Test]
        public void WhenAPersonHasNoCaseStatuses()
        {
            var person = TestHelpers.CreatePerson();

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(person.Id)).Returns(person);
            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusesByPersonId(person.Id))
                .Returns(new List<SocialCareCaseViewerApi.V1.Domain.CaseStatus>());

            var result = _caseStatusUseCase.ExecuteGet(person.Id);

            result.PersonId.Should().Be(person.Id);
            result.CaseStatuses.Should().BeEmpty();
        }

        [Test]
        public void WhenAPersonHasACurrentCaseStatus()
        {
            var person = TestHelpers.CreatePerson();
            var caseStatusType = CaseStatusDomainHelper.NewCaseStatusType();
            var caseStatus = CaseStatusDomainHelper.NewCaseStatusForPerson(caseStatusType);

            var response = new List<SocialCareCaseViewerApi.V1.Domain.CaseStatus> { caseStatus };

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(person.Id)).Returns(person);
            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusesByPersonId(person.Id)).Returns(response);

            var result = _caseStatusUseCase.ExecuteGet(person.Id);

            result.PersonId.Should().Be(person.Id);
            result.CaseStatuses.Count.Should().Be(1);
            result.CaseStatuses.Single().Notes.Should().Be(caseStatus.Notes);
            result.CaseStatuses.Single().StartDate.Should().Be(caseStatus.StartDate);
            result.CaseStatuses.Single().Fields.First().SelectedOption.Name.Should()
                .Be(caseStatusType.Fields.First().Options.First().Name);
            result.CaseStatuses.Single().Fields.First().SelectedOption.Description.Should()
                .Be(caseStatusType.Fields.First().Options.First().Description);
        }
    }
}
