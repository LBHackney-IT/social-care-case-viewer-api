using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.RequestAudit
{
    [TestFixture]
    public class CreateRequestAuditTests
    {
        private CreateRequestAuditUseCase _createRequestUseCase;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _createRequestUseCase = new CreateRequestAuditUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void CallsDatabaseGateway()
        {
            var request = new CreateRequestAuditRequest();

            _createRequestUseCase.Execute(request);

            _mockDatabaseGateway.Verify(x => x.CreateRequestAudit(request));
        }

        [Test]
        public void DoesNotThrowAnExceptionWhenDatabaseGatewayThrowsAnException()
        {
            var request = new CreateRequestAuditRequest();

            _mockDatabaseGateway.Setup(x => x.CreateRequestAudit(request)).Throws(new Exception());

            _createRequestUseCase.Invoking(x => x.Execute(request)).Should().NotThrow<Exception>();
        }
    }
}
