using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using FluentAssertions;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using System;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class CaseTypesUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private CaseStatusesUseCase _caseStatusesUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void WhenPersonIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetCaseStatusExceptionWithMessage()
        {
            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(1234, DateTime.Today, DateTime.Now));

            _caseStatusesUseCase.Invoking(x => x.ExecuteGet(1234, DateTime.Today.ToString("dd-MM-yyyy"), DateTime.Now.ToString("dd-MM-yyyy")))
                .Should().Throw<GetCaseStatusesException>()
                .WithMessage("Person not found");

        }


        [Test]
        public void WhenPersonIsFoundWithNoCaseStatusesReturnsEmptyList()
        {
            var person = TestHelpers.CreatePerson();
            var emptyResponse = new List<CaseStatus>() { };

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(person.Id)).Returns(person);
            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(person.Id, DateTime.Today, DateTime.Now)).Returns(emptyResponse);

            var result = _caseStatusesUseCase.ExecuteGet(person.Id, DateTime.Today.ToString("dd-MM-yyyy"), DateTime.Now.ToString("dd-MM-yyyy"));

            result.Should().BeEquivalentTo(new ListCaseStatusesResponse() { PersonId = person.Id, CaseStatuses = new List<SocialCareCaseViewerApi.V1.Domain.CaseStatus>() { } });
        }

        [Test]
        public void WhenPersonIsFoundWithCaseStatusesReturnsCaseStatusesList()
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id);
            var person = TestHelpers.CreatePerson();
            var csus = TestHelpers.CreateCaseStatus(personId: person.Id, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Today, notes: "Testing");

            var response = new List<CaseStatus>() { csus };

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(person.Id)).Returns(person);
            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(person.Id, DateTime.Today, DateTime.Today)).Returns(response);

            var result = _caseStatusesUseCase.ExecuteGet(person.Id, DateTime.Today.ToString("dd-MM-yyyy"), DateTime.Now.ToString("dd-MM-yyyy"));

            result.CaseStatuses.Count.Should().Be(1);
        }
    }
}
