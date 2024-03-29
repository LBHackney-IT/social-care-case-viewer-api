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

            var MediumAllocation = TestHelpers.CreateAllocation(ragRating: "Medium", personId: personId, caseStatus: "open");
            var UrgentAllocation = TestHelpers.CreateAllocation(ragRating: "Urgent", personId: personId, caseStatus: "open");
            var LowAllocation = TestHelpers.CreateAllocation(ragRating: "Low", personId: personId, caseStatus: "open");
            var HighAllocation = TestHelpers.CreateAllocation(ragRating: "High", personId: personId, caseStatus: "open");
            var allocationWithoutRagRating = TestHelpers.CreateAllocation(ragRating: null, personId: personId, caseStatus: "open");

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { HighAllocation, LowAllocation, allocationWithoutRagRating, UrgentAllocation, MediumAllocation });
            DatabaseContext.SaveChanges();

            var (result, _, _) = _databaseGateway.SelectAllocations(mosaicId: personId, 0, "", 0, 0, sortBy: "rag_rating", 0);

            result[0].Id.Should().Be(UrgentAllocation.Id);
            result[1].Id.Should().Be(HighAllocation.Id);
            result[2].Id.Should().Be(MediumAllocation.Id);
            result[3].Id.Should().Be(LowAllocation.Id);
            result[4].Id.Should().Be(allocationWithoutRagRating.Id);

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

            var MediumAllocation = TestHelpers.CreateAllocation(ragRating: "Medium", personId: personId, caseStatus: "open");
            var UrgentAllocation = TestHelpers.CreateAllocation(ragRating: "Urgent", personId: personId, caseStatus: "open");
            var LowAllocation = TestHelpers.CreateAllocation(ragRating: "Low", personId: personId, caseStatus: "open");
            var HighAllocation = TestHelpers.CreateAllocation(ragRating: "High", personId: personId, caseStatus: "open");
            var allocationWithoutRagRating = TestHelpers.CreateAllocation(ragRating: null, personId: personId, caseStatus: "open");

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { HighAllocation, LowAllocation, allocationWithoutRagRating, UrgentAllocation, MediumAllocation });
            DatabaseContext.SaveChanges();

            var (result, _, _) = _databaseGateway.SelectAllocations(mosaicId: personId, 0, workerEmail: "", 0, 0, sortBy: sortBy, 0);

            result[0].Id.Should().Be(UrgentAllocation.Id);
            result[1].Id.Should().Be(HighAllocation.Id);
            result[2].Id.Should().Be(MediumAllocation.Id);
            result[3].Id.Should().Be(LowAllocation.Id);
            result[4].Id.Should().Be(allocationWithoutRagRating.Id);

        }

        [Test]
        public void GetAllocationsReturnResultsInDateAddedOrderWhenRequested()
        {
            var person = TestHelpers.CreatePerson();
            var personId = (int) person.Id;

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var allocation1 = TestHelpers.CreateAllocation(personId: personId, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-20));
            var allocation2 = TestHelpers.CreateAllocation(personId: personId, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-40));
            var allocation3 = TestHelpers.CreateAllocation(personId: personId, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-10));
            var allocation4 = TestHelpers.CreateAllocation(personId: personId, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-30));

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { allocation1, allocation2, allocation3, allocation4 });
            DatabaseContext.SaveChanges();

            var (result, _, _) = _databaseGateway.SelectAllocations(mosaicId: personId, workerId: 0, workerEmail: "", 0, 0, "date_added", 0);

            result[0].Id.Should().Be(allocation2.Id);
            result[1].Id.Should().Be(allocation4.Id);
            result[2].Id.Should().Be(allocation1.Id);
            result[3].Id.Should().Be(allocation3.Id);
        }

        [Test]
        public void GetAllocationsReturnResultsInPersonReviewDateOrderWithNullLastWhenRequested()
        {
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);

            var person1 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(10));
            var allocation1 = TestHelpers.CreateAllocation(personId: (int) person1.Id, workerId: worker.Id, caseStatus: "open");

            var person2 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(20));
            var allocation2 = TestHelpers.CreateAllocation(personId: (int) person2.Id, workerId: worker.Id, caseStatus: "open");

            var person3 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(30));
            var allocation3 = TestHelpers.CreateAllocation(personId: (int) person3.Id, workerId: worker.Id, caseStatus: "open");

            var person4 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(40));
            var allocation4 = TestHelpers.CreateAllocation(personId: (int) person4.Id, workerId: worker.Id, caseStatus: "open");

            var person5 = TestHelpers.CreatePerson();
            person5.ReviewDate = null;
            var allocation5 = TestHelpers.CreateAllocation(personId: (int) person5.Id, workerId: worker.Id, caseStatus: "open");

            var person6 = TestHelpers.CreatePerson(reviewDate: null);
            person6.ReviewDate = null;
            var allocation6 = TestHelpers.CreateAllocation(personId: (int) person6.Id, workerId: worker.Id, caseStatus: "open");

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3, person4, person5, person6 });
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { allocation6, allocation5, allocation4, allocation3, allocation2, allocation1 });
            DatabaseContext.SaveChanges();

            var (result, _, _) = _databaseGateway.SelectAllocations(0, workerId: worker.Id, workerEmail: "", 0, 0, "review_date");

            result[0].Id.Should().Be(allocation1.Id);
            result[1].Id.Should().Be(allocation2.Id);
            result[2].Id.Should().Be(allocation3.Id);
            result[3].Id.Should().Be(allocation4.Id);
            result[4].Id.Should().BeOneOf(allocation5.Id, allocation6.Id);
            result[5].Id.Should().BeOneOf(allocation5.Id, allocation6.Id);
        }

        [Test]
        public void GetAllocationsReturnResultsInPersonReviewDateOrderWhenRequested()
        {
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);

            var person1 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(10));
            var allocation1 = TestHelpers.CreateAllocation(personId: (int) person1.Id, workerId: worker.Id, caseStatus: "open");

            var person2 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(20));
            var allocation2 = TestHelpers.CreateAllocation(personId: (int) person2.Id, workerId: worker.Id, caseStatus: "open");

            var person3 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(30));
            var allocation3 = TestHelpers.CreateAllocation(personId: (int) person3.Id, workerId: worker.Id, caseStatus: "open");

            var person4 = TestHelpers.CreatePerson(reviewDate: DateTime.Today.AddDays(40));
            var allocation4 = TestHelpers.CreateAllocation(personId: (int) person4.Id, workerId: worker.Id, caseStatus: "open");

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3, person4 });
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { allocation1, allocation2, allocation3, allocation4 });
            DatabaseContext.SaveChanges();

            var (result, _, _) = _databaseGateway.SelectAllocations(0, workerId: worker.Id, workerEmail: "", 0, 0, "review_date");

            result[0].Id.Should().Be(allocation1.Id);
            result[1].Id.Should().Be(allocation2.Id);
            result[2].Id.Should().Be(allocation3.Id);
            result[3].Id.Should().Be(allocation4.Id);
        }

        [Test]
        public void GetAllocationsReturnsResultsInSortByDateAddedOrderWhenFilteredByWorkerIdAndWhenSortByIsSetToDateAdded()
        {
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);

            var person1 = TestHelpers.CreatePerson();
            var allocation1 = TestHelpers.CreateAllocation(personId: (int) person1.Id, workerId: worker.Id, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-50));

            var person2 = TestHelpers.CreatePerson();
            var allocation2 = TestHelpers.CreateAllocation(personId: (int) person2.Id, workerId: worker.Id, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-10));

            var person3 = TestHelpers.CreatePerson();
            var allocation3 = TestHelpers.CreateAllocation(personId: (int) person3.Id, workerId: worker.Id, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-60));

            var person4 = TestHelpers.CreatePerson();
            var allocation4 = TestHelpers.CreateAllocation(personId: (int) person4.Id, workerId: worker.Id, caseStatus: "open", dateAdded: DateTime.Today.AddDays(-30));

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3, person4 });
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Allocations.AddRange(new List<AllocationSet>() { allocation1, allocation2, allocation3, allocation4 });
            DatabaseContext.SaveChanges();

            var (result, _, _) = _databaseGateway.SelectAllocations(mosaicId: 0, workerId: worker.Id, workerEmail: "", teamId: 0, 0, sortBy: "date_added");

            result[0].Id.Should().Be(allocation3.Id);
            result[1].Id.Should().Be(allocation1.Id);
            result[2].Id.Should().Be(allocation4.Id);
            result[3].Id.Should().Be(allocation2.Id);
        }
    }
}
