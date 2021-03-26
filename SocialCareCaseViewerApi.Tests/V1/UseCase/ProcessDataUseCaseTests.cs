using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class ProcessDataUseCaseTests
    {
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private ProcessDataUseCase _classUnderTest;
        private Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _classUnderTest = new ProcessDataUseCase(_mockProcessDataGateway.Object, _mockDatabaseGateway.Object);
        }

        // [Test]
        // public void ExecuteReturnsCareCaseDataListWhenProvidedListCasesRequest()
        // {
        //     var stubbedCaseData = _fixture.CreateMany<CareCaseData>();

        //     _mockProcessDataGateway.Setup(x => x.GetProcessData(It.IsAny<ListCasesRequest>()))
        //         .Returns(stubbedCaseData.ToList());

        //     var response = _classUnderTest.Execute(new ListCasesRequest());

        //     response.Should().NotBeNull();
        //     response.Cases.Should().BeEquivalentTo(stubbedCaseData);
        // }

        //[Test]
        //public async Task ExecuteReturnsStringWhenProvidedCaseNoteDocument()
        //{
        //    _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
        //        .Returns(Task.FromResult("response"));

        //    var response = await _classUnderTest.Execute(new CaseNotesDocument()).ConfigureAwait(true);

        //    response.Should().NotBeNull();
        //    response.Should().BeOfType<string>();
        //}

        // [Test]
        // public void ExecuteIfLimitLessThanTheMinimumWillUseTheMinimumLimit()
        // {
        //     var stubbedRequest = new ListCasesRequest();
        //     stubbedRequest.Limit = 10;
        //     _mockProcessDataGateway.Setup(x =>
        //         x.GetProcessData(stubbedRequest))
        //         .Returns(new List<CareCaseData>()).Verifiable();
        // }

        //[Test]
        //public void ExecuteIfLimitLessThanTheMinimumWillUseTheMinimumLimit()
        //{
        //    var stubbedRequest = new ListCasesRequest();
        //    stubbedRequest.Limit = 10;
        //    _mockProcessDataGateway.Setup(x =>
        //        x.GetProcessData(stubbedRequest))
        //        .Returns(new List<CareCaseData>()).Verifiable();

        //    stubbedRequest.Limit = 4;
        //    _classUnderTest.Execute(stubbedRequest);

        //    _mockProcessDataGateway.Verify();
        //}

        //[Test]
        //public void ExecuteIfLimitMoreThanTheMaximumWillUseTheMaximumLimit()
        //{
        //    var stubbedRequest = new ListCasesRequest();
        //    stubbedRequest.Limit = 100;
        //    _mockProcessDataGateway.Setup(x =>
        //        x.GetProcessData(stubbedRequest))
        //        .Returns(new List<CareCaseData>()).Verifiable();

        //    stubbedRequest.Limit = 400;
        //    _classUnderTest.Execute(stubbedRequest);

        //    _mockProcessDataGateway.Verify();
        //}

        //[Test]
        //public void ExecuteReturnsCursor()
        //{
        //    var stubbedRequest = new ListCasesRequest();
        //    var stubbedCases = _fixture.CreateMany<CareCaseData>(20);
        //    int idCount = 10;
        //    foreach (CareCaseData careCase in stubbedCases)
        //    {
        //        idCount++;
        //        careCase.RecordId = idCount.ToString();
        //    }
        //    var expectedNextCursor = stubbedCases.Max(r => r.RecordId);

        //    stubbedRequest.Cursor = 0;
        //    _mockProcessDataGateway.Setup(x =>
        //        x.GetProcessData(stubbedRequest))
        //        .Returns(stubbedCases.ToList());

        //    _classUnderTest.Execute(stubbedRequest).NextCursor.Should().Be(expectedNextCursor);
        //}

        //[Test]
        //public void WhenAtTheEndOfTheCaseListReturnsTheNextCursorAsEmptyString()
        //{
        //    var stubbedRequest = new ListCasesRequest();
        //    var stubbedCases = _fixture.CreateMany<CareCaseData>(7);

        //    _mockProcessDataGateway.Setup(x =>
        //        x.GetProcessData(stubbedRequest))
        //        .Returns(stubbedCases.ToList());

        //    _classUnderTest.Execute(stubbedRequest).NextCursor.Should().Be("");
        //}

        [Test]
        public void ExecuteReturnsCareCaseDataWhenProvidedRecordId()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();

            _mockProcessDataGateway.Setup(x => x.GetCaseById(It.IsAny<string>()))
                .Returns(stubbedCaseData);

            var response = _classUnderTest.Execute("test record id");

            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(stubbedCaseData);
        }
    }
}
