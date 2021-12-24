using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.HistoricalData
{
    [NonParallelizable]
    [TestFixture]
    public class HistoricalGetVisitInformationByPersonIdTests : DatabaseTests
    {
        private HistoricalSocialCareGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new HistoricalSocialCareGateway(DatabaseContext);
        }

        [Test]
        public void WhenThereAreNoVisitsWithPersonIdReturnsEmptyList()
        {
            const long realPersonId = 123L;
            const long fakePersonId = 456L;
            AddVisitToDatabase(personId: realPersonId);

            var response = _classUnderTest.GetVisitInformationByPersonId(fakePersonId);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenThereIsOneMatchReturnsAListContainingTheMatchingVisit()
        {
            var visit = AddVisitToDatabase();

            var response = _classUnderTest.GetVisitInformationByPersonId(visit.PersonId);

            response.ToList().Count.Should().Be(1);
            response.FirstOrDefault().VisitId.Should().Be(visit.VisitId);
        }

        [Test]
        public void WhenThereAreMultipleMatchesReturnsAListContainingAllMatchingVisits()
        {
            const long realPersonId = 123L;
            const long visitIdOne = 1L;
            const long visitIdTwo = 2L;

            AddVisitToDatabase(visitIdOne, personId: realPersonId);
            AddVisitToDatabase(visitIdTwo, personId: realPersonId);

            var response = _classUnderTest.GetVisitInformationByPersonId(realPersonId);

            response.ToList().Count.Should().Be(2);
        }

        [Test]
        public void OnlyReturnVisitsForCorrectPersonId()
        {
            const long realPersonId = 123L;
            const long otherPersonId = 456L;
            const long visitIdOne = 1L;
            const long visitIdTwo = 2L;

            AddVisitToDatabase(visitIdOne, personId: realPersonId);
            AddVisitToDatabase(visitIdTwo, personId: otherPersonId);

            var response = _classUnderTest.GetVisitInformationByPersonId(realPersonId);

            response.ToList().Count.Should().Be(1);
        }

        [Test]
        public void WhenThereIsAMatchingRecordReturnsDetailsFromVisit()
        {
            var visit = AddVisitToDatabase();

            var response = _classUnderTest.GetVisitInformationByPersonId(visit.PersonId).First();

            if (response == null)
            {
                throw new ArgumentNullException(null, nameof(response));
            }

            response.VisitId.Should().Be(visit.VisitId);
            response.VisitType.Should().Be(visit.VisitType);
            response.PlannedDateTime.Should().Be(visit.PlannedDateTime);
            response.ActualDateTime.Should().Be(visit.ActualDateTime);
            response.ReasonVisitNotMade.Should().Be(visit.ReasonVisitNotMade);
            response.SeenAloneFlag.Should().Be(!string.IsNullOrEmpty(visit.SeenAloneFlag) && visit.SeenAloneFlag.Equals("Y"));
            response.CompletedFlag.Should().Be(!string.IsNullOrEmpty(visit.CompletedFlag) && visit.CompletedFlag.Equals("Y"));
            response.CreatedByEmail.Should().Be(visit.Worker.EmailAddress);
            response.CreatedByName.Should().Be($"{visit.Worker.FirstNames} {visit.Worker.LastNames}");
        }

        private HistoricalVisit AddVisitToDatabase(long? visitId = null, long? workerId = null, long? personId = null)
        {
            var visit = HistoricalTestHelper.CreateDatabaseVisit(visitId, personId, workerId: workerId);

            DatabaseContext.HistoricalVisits.Add(visit);
            DatabaseContext.HistoricalWorkers.Add(visit.Worker);
            DatabaseContext.SaveChanges();

            return visit;
        }
    }
}
