using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using FluentAssertions;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    public class CaseStatusExecuteGetUseCaseTests
    {
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private CaseStatusesUseCase _caseStatusesUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);
        }

        [Test]
        public void WhenPersonIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetCaseStatusExceptionWithMessage()
        {
            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusesByPersonId(1234));

            _caseStatusesUseCase.Invoking(x => x.ExecuteGet(1234))
                .Should().Throw<GetCaseStatusesException>()
                .WithMessage("Person not found");
        }


        [Test]
        public void WhenPersonIsFoundWithNoCaseStatusesReturnsEmptyList()
        {
            var person = TestHelpers.CreatePerson();

            _mockDatabaseGateway
                .Setup(x => x.GetPersonByMosaicId(person.Id))
                .Returns(person);
            _mockCaseStatusGateway
                .Setup(x => x.GetCaseStatusesByPersonId(person.Id))
                .Returns(new List<SocialCareCaseViewerApi.V1.Domain.CaseStatus>());

            var result = _caseStatusesUseCase.ExecuteGet(person.Id);

            result.Should().BeEquivalentTo(new List<CaseStatusResponse>());
        }

        [Test]
        public void WhenPersonIsFoundWithCaseStatusesReturnsCaseStatusesList()
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusTypeField = TestHelpers.CreateCaseStatusTypeField(caseStatusType.Id);
            var person = TestHelpers.CreatePerson();
            var caseStatus = TestHelpers.CreateCaseStatus(
                person.Id,
                caseStatusType.Id,
                options: new List<CaseStatusTypeFieldOption> { caseStatusTypeField.Options.First() }
            );

            var response = new List<SocialCareCaseViewerApi.V1.Domain.CaseStatus> { caseStatus.ToDomain() };

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(person.Id)).Returns(person);
            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusesByPersonId(person.Id)).Returns(response);

            var result = _caseStatusesUseCase.ExecuteGet(person.Id);

            result.Count.Should().Be(1);
            result.First().Fields.First().Name.Should().Be(caseStatusTypeField.Name);
            result.First().Fields.First().Description.Should().Be(caseStatusTypeField.Description);
            result.First().Fields.First().SelectedOption.Name.Should().Be("One");
            result.First().Fields.First().SelectedOption.Description.Should().Be("First option");
        }
    }
}
