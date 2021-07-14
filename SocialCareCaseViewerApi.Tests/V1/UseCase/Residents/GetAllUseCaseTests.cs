using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.Residents
{
    [TestFixture]
    public class GetAllUseCaseTests
    {
        private GetAllUseCase _getAllUseCase;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private Fixture _fixture = new Fixture();
        private const int MinimumLimit = 10;
        private const int MaximumLimit = 100;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _getAllUseCase = new GetAllUseCase(_mockDatabaseGateway.Object);

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null, null, null))
               .Returns(new List<ResidentInformation>());
        }

        [Test]
        public void ReturnsEmptyListWhenMosaicIdIsProvidedInLettersOnlyFormat()
        {
            var request = new ResidentQueryParam() { MosaicId = "nonNumeric" };

            _getAllUseCase.Execute(request, cursor: 0, limit: 20)
                .Should()
                .BeEquivalentTo(new ResidentInformationList() { Residents = new List<ResidentInformation>() });
        }

        [Test]
        public void IfLimitLessThanTheMinimumWillUseTheMinimumLimit()
        {
            _getAllUseCase.Execute(new ResidentQueryParam(), 0, 4);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MinimumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void IfLimitIsOnTheMinimumBoundaryWillUseTheMinimumLimit()
        {
            _getAllUseCase.Execute(new ResidentQueryParam(), 0, 10);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MinimumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void IfLimitMoreThanTheMaximumWillUseTheMaximumLimit()
        {
            _getAllUseCase.Execute(new ResidentQueryParam(), 0, 400);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MaximumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void IfLimitIsOnTheMaximumBoundaryWillUseTheMaxmumLimit()
        {
            _getAllUseCase.Execute(new ResidentQueryParam(), 0, 100);
            _mockDatabaseGateway.Verify(x => x.GetResidentsBySearchCriteria(It.IsAny<int>(), MaximumLimit, null, null, null, null, null, null, null));
        }

        [Test]
        public void ReturnsTheNextCursor()
        {
            var residents = _fixture.CreateMany<ResidentInformation>(10);
            int idCount = 10;
            foreach (ResidentInformation resident in residents)
            {
                idCount++;
                resident.MosaicId = idCount.ToString();
            }
            var expectedNextCursor = residents.Max(r => r.MosaicId);

            _mockDatabaseGateway.Setup(x =>
                    x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
                .Returns(residents.ToList());

            _getAllUseCase.Execute(new ResidentQueryParam(), cursor: 0, limit: 10).NextCursor.Should().Be(expectedNextCursor);
        }

        [Test]
        public void WhenAtTheEndOfTheResidentListReturnsTheNextCursorAsEmptyString()
        {
            var residents = _fixture.CreateMany<ResidentInformation>(7);

            _mockDatabaseGateway.Setup(x =>
                    x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
                .Returns(residents.ToList());

            _getAllUseCase.Execute(new ResidentQueryParam(), cursor: 0, limit: 10).NextCursor.Should().Be("");
        }

        [Test]
        public void CallsGetResidentsBySearchCriteriaMethodWhenMosaicIdIsNotProvided()
        {
            var request = new ResidentQueryParam();

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
              .Returns(new List<ResidentInformation>()).Verifiable();

            _getAllUseCase.Execute(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify();
        }

        [Test]
        public void DoesNotCallGetResidentsBySearchCriteriaMethodWhenMosaicIdIsProvided()
        {
            var request = new ResidentQueryParam() { MosaicId = "43" };

            _mockDatabaseGateway.Setup(x => x.GetResidentsBySearchCriteria(0, 10, null, null, null, null, null, null, null))
              .Returns(new List<ResidentInformation>());

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<long>())).Returns(new List<long>());

            _getAllUseCase.Execute(request, cursor: 0, limit: 4);

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
        public void CallsGetPersonByMosaicIdWhenMosaicIdIsProvided(string mosaicId)
        {
            var request = new ResidentQueryParam() { MosaicId = mosaicId };

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<long>())).Returns(new List<long> { 1, 2 });
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _getAllUseCase.Execute(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(Convert.ToInt64(request.MosaicId)));
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

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<long>())).Returns(new List<long>() { 1, 2 });
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _getAllUseCase.Execute(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonIdsByEmergencyId(Convert.ToInt64(request.MosaicId)));
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

            _mockDatabaseGateway.Setup(x => x.GetPersonIdsByEmergencyId(It.IsAny<long>())).Returns(listOfMatchingIds);
            _mockDatabaseGateway.Setup(x => x.GetPersonsByListOfIds(It.IsAny<List<long>>())).Returns(new List<Person>() { TestHelpers.CreatePerson() });

            _getAllUseCase.Execute(request, cursor: 0, limit: 4);

            _mockDatabaseGateway.Verify(x => x.GetPersonsByListOfIds(listOfMatchingIds));
        }
    }
}
