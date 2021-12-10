using AutoFixture;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Residents
{
    [TestFixture]
    public class GetResidentsByQueryTests
    {
        private ResidentUseCase _residentUseCase = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        private readonly Fixture _fixture = new Fixture();
        private const int MinimumLimit = 10;
        private const int MaximumLimit = 100;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _residentUseCase = new ResidentUseCase(_mockDatabaseGateway.Object);

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null, null, null))
               .Returns((new List<ResidentInformation>(), 0));
        }

        [Test]
        public void ReturnsEmptyListWhenMosaicIdIsProvidedInLettersOnlyFormat()
        {
            var request = new ResidentQueryParam() { MosaicId = "nonNumeric" };

            _residentUseCase
                .GetResidentsByQuery(request, cursor: 0, limit: 20)
                .Should()
                .BeEquivalentTo(new ResidentInformationList() { Residents = new List<ResidentInformation>() });
        }

        [Test]
        public void IfLimitLessThanTheMinimumWillUseTheMinimumLimit()
        {
            _residentUseCase.GetResidentsByQuery(new ResidentQueryParam(), 0, 4);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MinimumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void IfLimitIsOnTheMinimumBoundaryWillUseTheMinimumLimit()
        {
            _residentUseCase.GetResidentsByQuery(new ResidentQueryParam(), 0, 10);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MinimumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void IfLimitMoreThanTheMaximumWillUseTheMaximumLimit()
        {
            _residentUseCase.GetResidentsByQuery(new ResidentQueryParam(), 0, 400);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MaximumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void IfLimitIsOnTheMaximumBoundaryWillUseTheMaximumLimit()
        {
            _residentUseCase.GetResidentsByQuery(new ResidentQueryParam(), 0, 100);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MaximumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void ReturnsTheNextCursor()
        {
            var residents = _fixture.CreateMany<ResidentInformation>(10).ToList();
            var idCount = 10;
            foreach (ResidentInformation resident in residents)
            {
                idCount++;
                resident.MosaicId = idCount.ToString();
            }
            var expectedNextCursor = residents.Max(r => r.MosaicId);

            _mockDatabaseGateway.Setup(x =>
                    x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
                .Returns((residents.ToList(), 0));

            _residentUseCase.GetResidentsByQuery(new ResidentQueryParam(), cursor: 0, limit: 10).NextCursor.Should().Be(expectedNextCursor);
        }

        [Test]
        public void WhenAtTheEndOfTheResidentListReturnsTheNextCursorAsEmptyString()
        {
            var residents = _fixture.CreateMany<ResidentInformation>(7);

            _mockDatabaseGateway.Setup(x =>
                    x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
                .Returns((residents.ToList(), 0));

            _residentUseCase.GetResidentsByQuery(new ResidentQueryParam(), cursor: 0, limit: 10).NextCursor.Should().Be("");
        }

        [Test]
        public void CallsGetResidentsBySearchCriteriaMethodWhenMosaicIdIsNotProvided()
        {
            var request = new ResidentQueryParam();

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
              .Returns((new List<ResidentInformation>(), 0)).Verifiable();

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify();
        }

        [Test]
        public void SetsTotalCountWhenMosaicIdIsProvided()
        {
            var request = new ResidentQueryParam() { MosaicId = "123" };
            var totalCount = 3;
            var listOfIds = new List<long> { 1, 2, 3 };

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
              .Returns((new List<ResidentInformation>(), totalCount));

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(listOfIds);
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(listOfIds)).Returns(new List<Person>() { new Person(), new Person(), new Person() });

            var result = _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 10);

            result.TotalCount.Should().Be(totalCount);
        }

        [Test]
        public void DoesNotCallGetResidentsBySearchCriteriaMethodWhenMosaicIdIsProvided()
        {
            var request = new ResidentQueryParam() { MosaicId = "43" };

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
              .Returns((new List<ResidentInformation>(), 0));

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(new List<long>());

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        [TestCase("42")]
        [TestCase("NC42")]
        [TestCase("ASC42")]
        [TestCase("TMP42")]
        [TestCase("42NC")]
        [TestCase("A42SC")]
        [TestCase("TM42P")]
        public void CallsGetPersonDetailsByIdWhenMosaicIdIsProvidedWithoutZeroPrefix(string mosaicId)
        {
            var request = new ResidentQueryParam() { MosaicId = mosaicId };

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(new List<long> { 1, 2 });
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonDetailsById(Convert.ToInt64(request.MosaicId)));
        }

        [Test]
        [TestCase("042")]
        [TestCase("0042")]
        public void DoesNotCallGetPersonDetailsByIdWhenMosaicIdContainsLeadingZeros(string mosaicId)
        {
            var request = new ResidentQueryParam() { MosaicId = mosaicId };

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(new List<long> { 1, 2 });
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonDetailsById(It.IsAny<long>()), Times.Never);
        }

        [Test]
        [TestCase("42")]
        [TestCase("NC42")]
        [TestCase("ASC42")]
        [TestCase("TMP42")]
        [TestCase("42NC")]
        [TestCase("A42SC")]
        [TestCase("TM42P")]
        public void CallsGetPersonIdsByEmergencyIdWhenMosaicIdIsProvided(string mosaicId)
        {
            var request = new ResidentQueryParam() { MosaicId = mosaicId };

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(new List<long>() { 1, 2 });
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonIdsByEmergencyId(request.MosaicId));
        }

        [Test]
        [TestCase("NC042")]
        [TestCase("NC0042")]
        [TestCase("NC00042")]
        public void CallsGetPersonIdsByEmergencyIdWithLettersRemovedLeavingLeadingZerosWhenMosaicIdIsProvided(string mosaicId)
        {
            var request = new ResidentQueryParam() { MosaicId = mosaicId };

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(new List<long>() { 1, 2 });
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            var expectedIdParameter = Regex.Replace(mosaicId, "[^0-9.]", "");

            _mockDatabaseGateway.Verify(x => x.GetPersonIdsByEmergencyId(expectedIdParameter));
        }

        [Test]
        [TestCase("42")]
        [TestCase("NC42")]
        [TestCase("ASC42")]
        [TestCase("TMP42")]
        [TestCase("42NC")]
        [TestCase("A42SC")]
        [TestCase("TM42P")]
        public void CallsGetPersonsByListOfIdsWhenMatchingEmergencyIdsFound(string mosaicId)
        {
            var request = new ResidentQueryParam() { MosaicId = mosaicId };

            var listOfMatchingIds = new List<long> { 1, 2 };

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<string>())).Returns(listOfMatchingIds);
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _residentUseCase.GetResidentsByQuery(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(listOfMatchingIds));
        }

        [Test]
        public void ReturnsTotalCountOfResultsInReturnObject()
        {
            var request = new ResidentQueryParam() { FirstName = "foo" };
            var totalCount = 3;

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(0, 10, null, request.FirstName, null, null, null, null, null)).Returns((new List<ResidentInformation>(), totalCount));

            var response = _residentUseCase.GetResidentsByQuery(request, 0, 1);

            response.TotalCount.Should().Be(totalCount);
        }
    }
}
