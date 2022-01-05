using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.HistoricalData
{
    [NonParallelizable]
    [TestFixture]
    public class HistoricalGetVisitInformationByVisitIdTests : HistoricalDataDatabaseTests

    {
        private HistoricalSocialCareGateway _classUnderTest = null!;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new HistoricalSocialCareGateway(HistoricalSocialCareContext);
        }

        [Test]
        public void WhenThereIsNoMatchingVisitReturnsNull()
        {
            const long realVisitId = 123L;
            const long fakeVisitId = 456L;
            AddVisitToDatabase(realVisitId);

            var response = _classUnderTest.GetVisitInformationByVisitId(fakeVisitId);

            response.Should().BeNull();
        }

        [Test]
        public void WhenThereAreMultipleVisitsReturnsVisitsWithMatchingId()
        {
            var visit = AddVisitToDatabase();
            AddVisitToDatabase();

            var response = _classUnderTest.GetVisitInformationByVisitId(visit.VisitId);

            if (response == null)
            {
                throw new ArgumentNullException(null, nameof(response));
            }

            response.VisitId.Should().Be(visit.VisitId);
        }

        [Test]
        public void WhenThereIsAMatchingVisitReturnsVisitDetails()
        {
            var visit = AddVisitToDatabase();

            var response = _classUnderTest.GetVisitInformationByVisitId(visit.VisitId);

            if (response == null)
            {
                throw new ArgumentNullException(null, nameof(response));
            }

            response.VisitId.Should().Be(visit.VisitId);
            response.PersonId.Should().Be(visit.PersonId);
            response.VisitType.Should().Be(visit.VisitType);
            response.PlannedDateTime.Should().Be(visit.PlannedDateTime);
            response.ActualDateTime.Should().Be(visit.ActualDateTime);
            response.ReasonNotPlanned.Should().Be(visit.ReasonNotPlanned);
            response.ReasonVisitNotMade.Should().Be(visit.ReasonVisitNotMade);
            response.SeenAloneFlag.Should().Be(!string.IsNullOrEmpty(visit.SeenAloneFlag) && visit.SeenAloneFlag.Equals("Y"));
            response.CompletedFlag.Should().Be(!string.IsNullOrEmpty(visit.CompletedFlag) && visit.CompletedFlag.Equals("Y"));
            response.CreatedByEmail.Should().Be(visit.Worker.EmailAddress);
            response.CreatedByName.Should().Be($"{visit.Worker.FirstNames} {visit.Worker.LastNames}");
        }

        private HistoricalVisit AddVisitToDatabase(long? visitId = null, long? workerId = null)
        {
            var visit = HistoricalTestHelper.CreateDatabaseVisit(visitId, workerId: workerId);

            HistoricalSocialCareContext.HistoricalVisits.Add(visit);
            HistoricalSocialCareContext.HistoricalWorkers.Add(visit.Worker);
            HistoricalSocialCareContext.SaveChanges();

            return visit;
        }
    }
}
