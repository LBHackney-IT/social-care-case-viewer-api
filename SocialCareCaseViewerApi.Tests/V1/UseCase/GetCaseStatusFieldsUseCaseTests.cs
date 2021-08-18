using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Gateways;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class GetCaseStatusFieldsUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private GetCaseStatusFieldsUseCase _getCaseStatusFieldsUseCase;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _getCaseStatusFieldsUseCase = new GetCaseStatusFieldsUseCase(_mockDataBaseGateway.Object);
            _fixture = new Fixture();
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
            _getCaseStatusFieldsUseCase.Execute(request);

            _mockDataBaseGateway.Verify(x => x.GetCaseStatusFieldsByType("type"));
        }

        [Test]
        public void ExecuteReturnsEmptyWhenCaseStatusTypeNotFound()
        {
            GetCaseStatusFieldsRequest request = GetRequestForType("type");

            _mockDataBaseGateway.Setup(x => x.GetCaseStatusFieldsByType(request.Type))
                .Returns(Enumerable.Empty<CaseStatusTypeField>());

            GetCaseStatusFieldsResponse response = _getCaseStatusFieldsUseCase.Execute(request);

            response.Fields.Should().BeEmpty();
        }

        [Test]
        public void ExecuteReturnsFieldsWhenCaseStatusTypeFound()
        {
            GetCaseStatusFieldsRequest request = GetRequestForType("type");

            _mockDataBaseGateway.Setup(x => x.GetCaseStatusFieldsByType(request.Type))
                .Returns(DatabaseGatewayTests.GetValidCaseStatusTypeFields(new CaseStatusType()
                {
                    Name = "Test",
                    Description = "Test Type"
                }));

            GetCaseStatusFieldsResponse response = _getCaseStatusFieldsUseCase.Execute(request);

            response.Fields.First().Type.Name.Should().Be("Test");
            response.Fields.First().Name.Should().Be("someThing");
            response.Fields.First().Options.First().Name.Should().Be("One");
            response.Fields.First().Options.Last().Name.Should().Be("Two");
        }
    }
}
