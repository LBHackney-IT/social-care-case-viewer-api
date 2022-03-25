using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetAllocationsSortingTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessdataGateway;
        private readonly Mock<ISystemTime> _mockIsystemTime = new Mock<ISystemTime>();

        [SetUp]
        public void SetUp()
        {
            _mockProcessdataGateway = new Mock<IProcessDataGateway>();
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessdataGateway.Object, _mockIsystemTime.Object);

            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void GetAllocationsReturnResultsInRagRatingOrderWhenRequested()
        {
            var person = TestHelpers.CreatePerson();
            var personId = (int) person.Id;

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var MediumAllocation = TestHelpers.CreateAllocation(ragRating: "Medium", personId: personId);
            var UrgentAllocation = TestHelpers.CreateAllocation(ragRating: "Urgent", personId: personId);
            var LowAllocation = TestHelpers.CreateAllocation(ragRating: "Low", personId: personId);
            var HighAllocation = TestHelpers.CreateAllocation(ragRating: "High", personId: personId);
            var allocationWithoutRagRating = TestHelpers.CreateAllocation(ragRating: null, personId: personId);

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { HighAllocation, LowAllocation, allocationWithoutRagRating, UrgentAllocation, MediumAllocation });
            DatabaseContext.SaveChanges();

            var (result, _) = _databaseGateway.SelectAllocations(mosaicId: personId, 0, "", 0, sortBy: "rag_rating", 0);

            result[0].Id.Should().Be(allocationWithoutRagRating.Id);
            result[1].Id.Should().Be(UrgentAllocation.Id);
            result[2].Id.Should().Be(HighAllocation.Id);
            result[3].Id.Should().Be(MediumAllocation.Id);
            result[4].Id.Should().Be(LowAllocation.Id);
        }

        [Test]
        [TestCase("random_string")]
        [TestCase("")]
        public void GetAllocationsReturnResultsInRagRatingOrderByDefaultOrWhenUnknownSortByStringIsProvided(string sortBy)
        {
            var person = TestHelpers.CreatePerson();
            var personId = (int) person.Id;

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var MediumAllocation = TestHelpers.CreateAllocation(ragRating: "Medium", personId: personId);
            var UrgentAllocation = TestHelpers.CreateAllocation(ragRating: "Urgent", personId: personId);
            var LowAllocation = TestHelpers.CreateAllocation(ragRating: "Low", personId: personId);
            var HighAllocation = TestHelpers.CreateAllocation(ragRating: "High", personId: personId);
            var allocationWithoutRagRating = TestHelpers.CreateAllocation(ragRating: null, personId: personId);

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { HighAllocation, LowAllocation, allocationWithoutRagRating, UrgentAllocation, MediumAllocation });
            DatabaseContext.SaveChanges();

            var (result, _) = _databaseGateway.SelectAllocations(mosaicId: personId, 0, workerEmail: "", 0, sortBy: sortBy, 0);

            result[0].Id.Should().Be(allocationWithoutRagRating.Id);
            result[1].Id.Should().Be(UrgentAllocation.Id);
            result[2].Id.Should().Be(HighAllocation.Id);
            result[3].Id.Should().Be(MediumAllocation.Id);
            result[4].Id.Should().Be(LowAllocation.Id);
        }

        [Test]
        public void GetAllocationsReturnResultsInDateAddedOrderWhenRequested()
        {
            var person = TestHelpers.CreatePerson();
            var personId = (int) person.Id;

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var allocation1 = TestHelpers.CreateAllocation(personId: personId, dateAdded: DateTime.Today.AddDays(-20));
            var allocation2 = TestHelpers.CreateAllocation(personId: personId, dateAdded: DateTime.Today.AddDays(-40));
            var allocation3 = TestHelpers.CreateAllocation(personId: personId, dateAdded: DateTime.Today.AddDays(-10));
            var allocation4 = TestHelpers.CreateAllocation(personId: personId, dateAdded: DateTime.Today.AddDays(-30));

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { allocation1, allocation2, allocation3, allocation4 });
            DatabaseContext.SaveChanges();

            var (result, _) = _databaseGateway.SelectAllocations(mosaicId: personId, workerId: 0, workerEmail: "", 0, "date_added", 0);

            result[0].Id.Should().Be(allocation3.Id);
            result[1].Id.Should().Be(allocation1.Id);
            result[2].Id.Should().Be(allocation4.Id);
            result[3].Id.Should().Be(allocation2.Id);
        }

        [Test]
        public void GetAllocationsReturnResultsInPersonReviewDateOrderWhenRequested()
        {
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);

            var person1 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(30));
            var allocation1 = TestHelpers.CreateAllocation(personId: (int) person1.Id, workerId: worker.Id);

            var person2 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(10));
            var allocation2 = TestHelpers.CreateAllocation(personId: (int) person2.Id, workerId: worker.Id);

            var person3 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(40));
            var allocation3 = TestHelpers.CreateAllocation(personId: (int) person3.Id, workerId: worker.Id);

            var person4 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(20));
            var allocation4 = TestHelpers.CreateAllocation(personId: (int) person4.Id, workerId: worker.Id);

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3, person4 });
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { allocation1, allocation2, allocation3, allocation4 });
            DatabaseContext.SaveChanges();

            var (result, _) = _databaseGateway.SelectAllocations(0, workerId: worker.Id, workerEmail: "", 0, "review_date");

            result[0].Id.Should().Be(allocation2.Id);
            result[1].Id.Should().Be(allocation4.Id);
            result[2].Id.Should().Be(allocation1.Id);
            result[3].Id.Should().Be(allocation3.Id);
        }
    }
}
