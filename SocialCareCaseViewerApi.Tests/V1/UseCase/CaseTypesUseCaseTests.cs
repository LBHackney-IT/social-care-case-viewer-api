using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using FluentAssertions;
using SocialCareCaseViewerApi.V1.Exceptions;

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
        public void CallsDatabaseGateway()
        {
            var person = TestHelpers.CreatePerson();
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id);
            var csus = TestHelpers.CreateCaseStatus(personId: person.Id, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Now, endDate: DateTime.Now);

            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(It.IsAny<long>())).Returns(new List<CaseStatus>() { csus });

            var result = _caseStatusesUseCase.ExecuteGet(It.IsAny<long>());

            _mockDatabaseGateway.Verify(x => x.GetCaseStatusesByPersonId(It.IsAny<long>()));
        }

        [Test]
        public void WhenCaseStatusIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetRelationshipsExceptionWithMessage()
        {
            var person = TestHelpers.CreatePerson();

            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(person.Id));

            _caseStatusesUseCase.Invoking(x => x.ExecuteGet(person.Id))
                .Should().Throw<GetCaseStatusesException>()
                .WithMessage("Case status for person not found");

        }

        [Test]
        public void WhenPersonHasCaseStatusReturnsCaseStatuses()
        {
            var person = TestHelpers.CreatePerson();
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id);
            var csus = TestHelpers.CreateCaseStatus(personId: person.Id, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Now, endDate: DateTime.Now);

            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(It.IsAny<long>())).Returns(new List<CaseStatus>() { csus });

            var result = _caseStatusesUseCase.ExecuteGet(It.IsAny<long>());

            result.CaseStatuses.Should().HaveCount(1);
        }
    }
}
