using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class CaseRecordsUseCaseTests
    {
        private Mock<IProcessDataGateway> _mockProcessDataGateway = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateWay = null!;
        private Mock<IMongoGateway> _mockMongoGateway = null!;
        private CaseRecordsUseCase _caseRecordsUseCase = null!;

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockDatabaseGateWay = new Mock<IDatabaseGateway>();
            _mockMongoGateway = new Mock<IMongoGateway>();

            _caseRecordsUseCase = new CaseRecordsUseCase(_mockProcessDataGateway.Object, _mockDatabaseGateWay.Object, _mockMongoGateway.Object);
        }

                [Test]
        public void GetResidentCasesReturnsResidentsSubmittedCasesWithTotalCount()
        {
            var request = TestHelpers.CreateListCasesRequest(1L);

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""))
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(() => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));
            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 2));

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.TotalCount.Should().Be(3);
        }

        [Test]
        public void GetResidentCasesReturnsResidentsSubmittedCasesExcludingAuditTrailEventsWhenExcludeAuditTrailEventsIsSetToTrue()
        {
            var request = TestHelpers.CreateListCasesRequest(1L);
            request.ExcludeAuditTrailEvents = true;

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), formId: "Person updated"),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), formId: "Person created"),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), formId: "Warning note added"),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), formId: "Warning note expired"),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), formId: "Worker allocated"),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), formId: "Worker deallocated")
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(() => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));
            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 2));

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.Cases.Count.Should().Be(1);
            response.Cases.First().Should().BeEquivalentTo(expectedResponse.First().ToCareCaseData(request));
        }

        [Test]
        public void GetResidentCasesCallMongoGatewayAndReturnsResidentsSubmittedCases()
        {
            var request = TestHelpers.CreateListCasesRequest(1L);

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""))
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(
                () => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));
            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 2));

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.Cases.Count.Should().Be(2);
            response.Cases.Should().BeEquivalentTo(expectedResponse.Take(2).Select(x => x.ToCareCaseData(request)).ToList());
        }

        [Test]
        public void GetResidentCasesCallMongoGatewayAndReturnsDeletedCasesCountWhenRequested()
        {
            var request = TestHelpers.CreateListCasesRequest(1L, includeDeletedRecordsCount: true);

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), deleted: true)
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(
                () => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));

            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 2));

            _mockMongoGateway.Setup(x =>
                x.GetRecordsCountByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>()))
                .Returns(1);

            var response = _caseRecordsUseCase.GetResidentCases(request);

            response.DeletedRecordsCount.Should().Be(1);
        }

        [Test]
        public void GetResidentCasesDoesNotCallMongoGatewayToGetDeletedCasesCountIfNotRequested()
        {
            var request = TestHelpers.CreateListCasesRequest(1L, includeDeletedRecordsCount: false);

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""), deleted: true)
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");

            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(() => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));

            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 2));

            _mockMongoGateway.Setup(x =>
                x.GetRecordsCountByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>()))
                .Returns(1);

            _caseRecordsUseCase.GetResidentCases(request);

            _mockMongoGateway.Verify(x => x.GetRecordsCountByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.IsAny<FilterDefinition<CaseSubmission>>()), Times.Never);

        }

        [Test]
        public void GetResidentsCasesCallsMongoGatewayToGetDeletedRecordsCountForTheGivenFilterWhenRequested()
        {
            var request = TestHelpers.CreateListCasesRequest(mosaicId: 1L, includeDeletedRecordsCount: true);

            var expectedJsonFilter = "{ \"Residents._id\" : 1, \"SubmissionState\" : 1, \"Deleted\" : true }";

            var expectedResponse = new List<CaseSubmission>
            {
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? "")),
                TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, residentId: int.Parse(request.MosaicId ?? ""))
            };

            _mockDatabaseGateWay.Setup(x => x.GetNCReferenceByPersonId(request.MosaicId)).Returns(request.MosaicId ?? "");
            _mockDatabaseGateWay.Setup(x => x.GetPersonIdByNCReference(request.MosaicId)).Returns(request.MosaicId ?? "");

            _mockProcessDataGateway.Setup(x => x.GetProcessData(request, request.MosaicId)).Returns(() => new Tuple<IEnumerable<CareCaseData>, int>(new List<CareCaseData>(), 0));

            _mockMongoGateway
                .Setup(x => x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                    It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((expectedResponse, 2));

            _mockMongoGateway.Setup(x => x.GetRecordsCountByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.IsAny<FilterDefinition<CaseSubmission>>())).Returns(1);

            _caseRecordsUseCase.GetResidentCases(request);

            _mockMongoGateway.Verify(
                x => x.GetRecordsCountByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions],
                        It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter))), Times.Once);
        }


        [Test]
        public void GenerateFilterDefinitionForDefaultCase()
        {
            const string expectedJsonQuery = "{ \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            var emptyRequest = TestHelpers.CreateListCasesRequest();

            var response = CaseRecordsUseCase.GenerateFilterDefinition(emptyRequest);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithProvidedMosaicId()
        {
            const string expectedJsonQuery = "{ \"Residents._id\" : 1, \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            const long mosaicId = 1L;
            var requestWithMosaicId = TestHelpers.CreateListCasesRequest(mosaicId: mosaicId);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithMosaicId);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithProvidedFirstName()
        {
            const string expectedJsonQuery = "{ \"Residents.FirstName\" : /^testington$/i, \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            const string firstName = "testington";
            var requestWithFirstName = TestHelpers.CreateListCasesRequest(firstName: firstName);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithFirstName);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithProvidedLastName()
        {
            const string expectedJsonQuery = "{ \"Residents.LastName\" : /^toastington$/i, \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            const string lastName = "toastington";
            var requestWithLastName = TestHelpers.CreateListCasesRequest(lastName: lastName);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithLastName);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithProvidedWorkerEmail()
        {
            const string expectedJsonQuery = "{ \"CreatedBy.Email\" : \"foo@hackney.gov.uk\", \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            const string workerEmail = "foo@hackney.gov.uk";
            var requestWithLastName = TestHelpers.CreateListCasesRequest(workerEmail: workerEmail);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithLastName);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithProvidedIncludeDeletedRecordsFlagSetToTrue()
        {
            const string expectedJsonQuery = "{ \"SubmissionState\" : 1 }";
            const bool includeDeletedRecordsFlag = true;
            var requestWithIncludeDeletedRecordsFlag = TestHelpers.CreateListCasesRequest(includeDeletedRecords: includeDeletedRecordsFlag);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithIncludeDeletedRecordsFlag);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithProvidedIncludeDeletedRecordsFlagSetToFalse()
        {
            const string expectedJsonQuery = "{ \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            const bool includeDeletedRecordsFlag = false;
            var requestWithIncludeDeletedRecordsFlag = TestHelpers.CreateListCasesRequest(includeDeletedRecords: includeDeletedRecordsFlag);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithIncludeDeletedRecordsFlag);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithAddDeletedRecordsFilterTrueReturnsFilterWithDeletedRecordsExcludedWhenTheyAreNotRequested()
        {
            const string expectedJsonQuery = "{ \"SubmissionState\" : 1, \"Deleted\" : { \"$ne\" : true } }";
            const bool includeDeletedRecordsFlag = false;
            var requestWithIncludeDeletedRecordsFlag = TestHelpers.CreateListCasesRequest(includeDeletedRecords: includeDeletedRecordsFlag);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithIncludeDeletedRecordsFlag, addDeletedRecordsFilter: true);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }

        [Test]
        public void GenerateFilterDefinitionWithAddDeletedRecordsFilterFalseReturnsFilterWithDeletedRecordsIncludedWhetherTheyAreRequestedOrNot()
        {
            const string expectedJsonQuery = "{ \"SubmissionState\" : 1 }";
            const bool includeDeletedRecordsFlag = false;
            var requestWithIncludeDeletedRecordsFlag = TestHelpers.CreateListCasesRequest(includeDeletedRecords: includeDeletedRecordsFlag);

            var response = CaseRecordsUseCase.GenerateFilterDefinition(requestWithIncludeDeletedRecordsFlag, addDeletedRecordsFilter: false);

            response.RenderToJson().Should().Be(expectedJsonQuery);
        }
    }
}
