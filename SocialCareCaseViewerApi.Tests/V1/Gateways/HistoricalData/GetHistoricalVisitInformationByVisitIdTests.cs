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
    public class GetHistoricalVisitInformationByVisitIdTests : HistoricalDataDatabaseTests

    {
        private HistoricalDataGateway _historicalDataGateway = null!;

        [SetUp]
        public void Setup()
        {
            _historicalDataGateway = new HistoricalDataGateway(HistoricalSocialCareContext);
        }

        [Test]
        public void WhenThereIsNoMatchingVisitReturnsNull()
        {
            const long realVisitId = 123L;
            const long fakeVisitId = 456L;
            AddVisitToDatabase(realVisitId);

            var response = _historicalDataGateway.GetVisitById(fakeVisitId);

            response.Should().BeNull();
        }

        [Test]
        public void WhenThereAreMultipleVisitsReturnsVisitsWithMatchingId()
        {
            var visit = AddVisitToDatabase();
            AddVisitToDatabase();

            var response = _historicalDataGateway.GetVisitById(visit.VisitId);

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

            var response = _historicalDataGateway.GetVisitById(visit.VisitId);

            if (response == null)
            {
                throw new ArgumentNullException(null, nameof(response));
            }

            response.VisitId.Should().Be(visit.VisitId);
            response.PersonId.Should().Be(visit.PersonId);
            response.VisitType.Should().Be(visit.VisitType);
            response.PlannedDateTime.Should().BeCloseTo(visit.PlannedDateTime!.Value);
            response.ActualDateTime.Should().BeCloseTo(visit.ActualDateTime!.Value);
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
