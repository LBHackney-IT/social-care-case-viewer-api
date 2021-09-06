using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Gateways;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CaseStatusFieldsExecuteGetUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private GetCaseStatusFieldsUseCase _getCaseStatusFieldsUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _getCaseStatusFieldsUseCase = new GetCaseStatusFieldsUseCase(_mockDataBaseGateway.Object);
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
            _mockDataBaseGateway.Setup(x => x.GetCaseStatusTypeWithFields(request.Type))
                .Returns(DatabaseGatewayTests.GetValidCaseStatusTypeWithFields("type"));

            _getCaseStatusFieldsUseCase.Execute(request);

            _mockDataBaseGateway.Verify(x => x.GetCaseStatusTypeWithFields("type"));
        }

        [Test]
        public void ExecuteReturnsEmptyWhenCaseStatusTypeNotFound()
        {
            GetCaseStatusFieldsRequest request = GetRequestForType("type");

            _mockDataBaseGateway.Setup(x => x.GetCaseStatusTypeWithFields(request.Type))
                .Returns((CaseStatusType) null);

            Action act = () => _getCaseStatusFieldsUseCase.Execute(request);

            act.Should().Throw<CaseStatusNotFoundException>()
                .WithMessage("Case Status Type does not exist.");
        }

        [Test]
        public void ExecuteReturnsFieldsWhenCaseStatusTypeFound()
        {
            GetCaseStatusFieldsRequest request = GetRequestForType("type");

            CaseStatusType caseStatusType = DatabaseGatewayTests.GetValidCaseStatusTypeWithFields("type");

            _mockDataBaseGateway.Setup(x => x.GetCaseStatusTypeWithFields(request.Type))
                .Returns(caseStatusType);

            GetCaseStatusFieldsResponse response = _getCaseStatusFieldsUseCase.Execute(request);

            response.Fields.First().Name.Should().Be("someThing");
            response.Fields.First().Options.First().Name.Should().Be("One");
            response.Fields.First().Options.Last().Name.Should().Be("Two");
        }
    }
}
