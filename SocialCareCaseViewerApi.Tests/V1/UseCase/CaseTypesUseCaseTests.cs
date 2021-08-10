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
        public void WhenCaseStatusIsNotFoundAndDatabaseGatewayReturnsNullThrowsGetCaseStatusExceptionWithMessage()
        {
            _mockDatabaseGateway.Setup(x => x.GetCaseStatusesByPersonId(1234));

            _caseStatusesUseCase.Invoking(x => x.ExecuteGet(1234))
                .Should().Throw<GetCaseStatusesException>()
                .WithMessage("Person not found");

        }
    }
}
