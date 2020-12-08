using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class ProcessDataUseCaseTests
    {
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private ProcessDataUseCase _classUnderTest;
        private Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _classUnderTest = new ProcessDataUseCase(_mockProcessDataGateway.Object);
        }

        [Test]
        public void ExecuteIfLimitLessThanTheMinimumWillUseTheMinimumLimit()
        {
            var stubbedRequest = new ListCasesRequest();
            _mockProcessDataGateway.Setup(x =>
                x.GetProcessData(0, 10, stubbedRequest))
                .Returns(new List<CareCaseData>()).Verifiable();

            _classUnderTest.Execute(stubbedRequest, 0, 4);

            _mockProcessDataGateway.Verify();
        }

        [Test]
        public void ExecuteIfLimitMoreThanTheMaximumWillUseTheMaximumLimit()
        {
            var stubbedRequest = new ListCasesRequest();
            _mockProcessDataGateway.Setup(x =>
                x.GetProcessData(0, 100, stubbedRequest))
                .Returns(new List<CareCaseData>()).Verifiable();

            _classUnderTest.Execute(stubbedRequest, 0, 400);

            _mockProcessDataGateway.Verify();
        }

        [Test]
        public void ExecuteReturnsCursor()
        {
            var stubbedRequest = new ListCasesRequest();
            var stubbedCases = _fixture.CreateMany<CareCaseData>(10);
            int idCount = 10;
            foreach (CareCaseData careCase in stubbedCases)
            {
                idCount++;
                careCase.RecordId = idCount.ToString();
            }
            var expectedNextCursor = stubbedCases.Max(r => r.RecordId);

            _mockProcessDataGateway.Setup(x =>
                x.GetProcessData(0, 10, stubbedRequest))
                .Returns(stubbedCases.ToList());

            _classUnderTest.Execute(stubbedRequest, 0, 10).NextCursor.Should().Be(expectedNextCursor);
        }

        [Test]
        public void WhenAtTheEndOfTheCaseListReturnsTheNextCursorAsEmptyString()
        {
            var stubbedRequest = new ListCasesRequest();
            var stubbedCases = _fixture.CreateMany<CareCaseData>(7);

            _mockProcessDataGateway.Setup(x =>
                x.GetProcessData(0, 10, stubbedRequest))
                .Returns(stubbedCases.ToList());

            _classUnderTest.Execute(stubbedRequest, 0, 10).NextCursor.Should().Be("");
        }
    }
}
