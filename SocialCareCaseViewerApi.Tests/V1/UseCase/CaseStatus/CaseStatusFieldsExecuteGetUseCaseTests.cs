using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Gateways;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
{
    [TestFixture]
    public class CaseStatusFieldsExecuteGetUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway;
        private CaseStatusesUseCase _getCaseStatusFieldsUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _getCaseStatusFieldsUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDataBaseGateway.Object);
        }

        private GetCaseStatusFieldsRequest GetRequestForType(string type)
        {
            return _fixture.Build<GetCaseStatusFieldsRequest>()
                .With(x => x.Type, type)
                .Create();
        }

        [Test]
        public void ExecuteCallsDatabaseGateway()
        {
            GetCaseStatusFieldsRequest request = GetRequestForType("type");
            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusTypeWithFields(request.Type))
                .Returns(DatabaseGatewayTests.GetValidCaseStatusTypeWithFields("type"));

            _getCaseStatusFieldsUseCase.ExecuteGetFields(request);

            _mockCaseStatusGateway.Verify(x => x.GetCaseStatusTypeWithFields("type"));
        }

        [Test]
        public void ExecuteReturnsEmptyWhenCaseStatusTypeNotFound()
        {
            var request = GetRequestForType("type");

            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusTypeWithFields(request.Type))
                .Returns((CaseStatusType) null);

            Action act = () => _getCaseStatusFieldsUseCase.ExecuteGetFields(request);

            act.Should().Throw<CaseStatusNotFoundException>()
                .WithMessage("Case Status Type does not exist.");
        }

        [Test]
        public void ExecuteReturnsFieldsWhenCaseStatusTypeFound()
        {
            var request = GetRequestForType("type");

            var caseStatusType = DatabaseGatewayTests.GetValidCaseStatusTypeWithFields("type");

            _mockCaseStatusGateway.Setup(x => x.GetCaseStatusTypeWithFields(request.Type))
                .Returns(caseStatusType);

            var response = _getCaseStatusFieldsUseCase.ExecuteGetFields(request);

            response.Fields.First().Name.Should().Be("someThing");
            response.Fields.First().Options?.First().Name.Should().Be("One");
            response.Fields.First().Options?.Last().Name.Should().Be("Two");
        }
    }
}
