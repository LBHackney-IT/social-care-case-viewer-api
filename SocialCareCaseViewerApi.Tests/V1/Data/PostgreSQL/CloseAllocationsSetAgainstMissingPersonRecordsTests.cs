using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Data.PostgreSQL
{
    [TestFixture]
    public class CloseAllocationsSetAgainstMissingPersonRecordsTests : DatabaseTests
    {
        //TODO: use real email
        private static readonly FormattableString _queryUnderTest = $@"
           update dbo.sccv_allocations_combined 
            set 
            case_status = 'Closed',
            sccv_last_modified_by = 'first.last@hackney.gov.uk',
            sccv_last_modified_at = NOW(),
            closure_date_if_closed = NOW()
            where id in
            (select 
	            a.id
                from dbo.sccv_allocations_combined a
                left join dbo.dm_persons p
                on a.mosaic_id = p.person_id
                where 
	            p.person_id is null and a.case_status = 'Open');
        ";

        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void ClosesAllAllocationsWhereAssociatedPersonDoesNotExistInTheDatabase()
        {
            var person = TestHelpers.CreatePerson();

            var allocation1 = TestHelpers.CreateAllocation(personId: (int)person.Id, caseStatus: "Open");
            var allocation2 = TestHelpers.CreateAllocation(personId: (int) person.Id, caseStatus: "Open");

            var allocationAgainstMissingPerson = TestHelpers.CreateAllocation(personId: 555, caseStatus: "Open");
            allocationAgainstMissingPerson.LastModifiedBy = null;
            allocationAgainstMissingPerson.LastModifiedAt = null;
            allocationAgainstMissingPerson.MarkedForDeletion = true;
            allocationAgainstMissingPerson.CaseClosureDate = null;

            DatabaseContext.Allocations.AddRange(new List<AllocationSet> { allocation1, allocation2, allocationAgainstMissingPerson });
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedAllocation = DatabaseContext.Allocations.FirstOrDefault(x => x.Id == allocationAgainstMissingPerson.Id);

            updatedAllocation.CaseStatus.Should().Be("Closed");
            updatedAllocation.LastModifiedBy.Should().Be("first.last@hackney.gov.uk");
            updatedAllocation.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, precision: 1000);
            updatedAllocation.MarkedForDeletion.Should().BeTrue();
            updatedAllocation.CaseClosureDate.Should().BeCloseTo(DateTime.UtcNow, precision: 1000);
        }

        [Test]
        public void DoesNotCloseAnyAllocationsThatHavePersonAssociatedWithThem()
        {
            var person = TestHelpers.CreatePerson();

            var allocation1 = TestHelpers.CreateAllocation(personId: (int) person.Id, caseStatus: "Open");
            var allocation2 = TestHelpers.CreateAllocation(personId: (int) person.Id, caseStatus: "Open");

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var allocations = DatabaseContext.Allocations.ToList();

            allocations.All(x => x.CaseStatus == "Open").Should().BeTrue();
        }
    }
}
