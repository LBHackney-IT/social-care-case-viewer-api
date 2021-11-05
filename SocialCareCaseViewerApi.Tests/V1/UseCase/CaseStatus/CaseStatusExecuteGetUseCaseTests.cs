using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System.Collections.Generic;
using DomainCaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;
using InfrastructureCasStatus = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    public class CaseStatusExecuteGetUseCaseTests
    {
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private CaseStatusesUseCase _caseStatusesUseCase;
        private ListCaseStatusesRequest _request;
        private Person _person;
        private InfrastructureCasStatus _caseStatus;

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);

            _request = new ListCaseStatusesRequest() { PersonId = 123, IncludeClosedCases = false };
            _person = TestHelpers.CreatePerson();
            _caseStatus = TestHelpers.CreateCaseStatus(_person.Id);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(_person);
        }

        [Test]
        public void WhenPersonIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetCaseStatusExceptionWithMessage()
        {
            _mockCaseStatusGateway.Setup(x => x.GetActiveCaseStatusesByPersonId(1234));
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(It.IsAny<long>())).Returns(default(Person));

            _caseStatusesUseCase.Invoking(x => x.ExecuteGet(_request))
                .Should().Throw<GetCaseStatusesException>()
                .WithMessage("Person not found");
        }

        [Test]
        public void WhenPersonIsFoundWithNoCaseStatusesReturnsEmptyList()
        {
            _mockCaseStatusGateway.Setup(x => x.GetActiveCaseStatusesByPersonId(It.IsAny<long>())).Returns(new List<DomainCaseStatus>());

            var result = _caseStatusesUseCase.ExecuteGet(_request);

            result.Should().BeEquivalentTo(new List<CaseStatusResponse>());
        }

        [Test]
        public void WhenPersonIsFoundWithCaseStatusesReturnsCaseStatusesList()
        {
            var response = new List<DomainCaseStatus> { _caseStatus.ToDomain() };

            _mockCaseStatusGateway.Setup(x => x.GetActiveCaseStatusesByPersonId(It.IsAny<long>())).Returns(response);

            var result = _caseStatusesUseCase.ExecuteGet(_request);

            result.Count.Should().Be(1);
        }

        [Test]
        public void WhenIncludeClosedCasesFlagIsFalseItCallsTheGetActiveCaseStatusesByPersonIdGatewayMethdod()
        {
            var response = new List<DomainCaseStatus> { _caseStatus.ToDomain() };

            _mockCaseStatusGateway.Setup(x => x.GetActiveCaseStatusesByPersonId(It.IsAny<long>())).Returns(response);

            _caseStatusesUseCase.ExecuteGet(_request);

            _mockCaseStatusGateway.Verify(x => x.GetActiveCaseStatusesByPersonId(It.IsAny<long>()));
        }

        [Test]
        public void WhenIncludeClosedCasesFlagIsTrueItCallsGetCaseStatusesByPersonIdGatewayMethod()
        {
            var response = new List<DomainCaseStatus> { _caseStatus.ToDomain() };

            _request.IncludeClosedCases = true;

            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusesByPersonId(It.IsAny<long>())).Returns(response);

            _caseStatusesUseCase.ExecuteGet(_request);

            _mockCaseStatusGateway.Verify(x => x.GetCaseStatusesByPersonId(It.IsAny<long>()));
        }
    }
}
