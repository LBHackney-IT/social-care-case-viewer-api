using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates;
using System;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Data.PostgreSQL
{
    [TestFixture]
    public class TeamAllocationsImportTests : DatabaseTests
    {
        //imports pre-validated records. Data set must only have waiting list items i.e. team_id is set, but not the worker_id 
        private static readonly FormattableString _queryUnderTest = $@"
            INSERT INTO dbo.sccv_allocations_combined(
                allocation_start_date,
                mosaic_id,
                team_id,
                case_status,
                sccv_created_at,
                sccv_created_by
            )
            SELECT
                referral_date,
                mosaic_id,
                team_id,
                'Open',
                NOW(),
                'Review team data import'
            FROM dbo.sccv_team_allocations_import_review_team;
           ";

        [Test]
        public void AddsNewTeamAllocationRecordsFromImportTable()
        {
            var teamAllocationToImport1 = new TeamAllocationImport()
            {
                MosaicId = 123,
                ReferralDate = DateTime.Today.AddDays(-20),
                TeamId = 142
            };

            var teamAllocationToImport2 = new TeamAllocationImport()
            {
                MosaicId = 456,
                ReferralDate = DateTime.Today.AddDays(-10),
                TeamId = 142
            };

            DatabaseContext.TeamAllocationImports.Add(teamAllocationToImport1);
            DatabaseContext.TeamAllocationImports.Add(teamAllocationToImport2);
            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            DatabaseContext.Allocations.Count().Should().Be(2);

            var addedTeamAllocation1 = DatabaseContext.Allocations.FirstOrDefault(x => x.PersonId == 123);
            var addedTeamAllocation2 = DatabaseContext.Allocations.FirstOrDefault(x => x.PersonId == 456);

            var expectedAllocationRecord1 = new AllocationSet()
            {
                Id = addedTeamAllocation1.Id,
                AllocationStartDate = teamAllocationToImport1.ReferralDate,
                PersonId = teamAllocationToImport1.MosaicId,
                TeamId = teamAllocationToImport1.TeamId,

                CaseStatus = "Open",
                MarkedForDeletion = false,
                AllocationEndDate = null,
                Summary = null,
                CarePackage = null,
                CaseClosureDate = null,
                WorkerId = null,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Review team data import",
                LastModifiedAt = null,
                LastModifiedBy = null
            };

            var expectedAllocationRecord2 = new AllocationSet()
            {
                Id = addedTeamAllocation2.Id,
                AllocationStartDate = teamAllocationToImport2.ReferralDate,
                PersonId = teamAllocationToImport2.MosaicId,
                TeamId = teamAllocationToImport2.TeamId,

                CaseStatus = "Open",
                MarkedForDeletion = false,
                AllocationEndDate = null,
                Summary = null,
                CarePackage = null,
                CaseClosureDate = null,
                WorkerId = null,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Review team data import",
                LastModifiedAt = null,
                LastModifiedBy = null
            };

            addedTeamAllocation1.Should().BeEquivalentTo(expectedAllocationRecord1, options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 3000)).WhenTypeIs<DateTime>();
                    return options;
                });

            addedTeamAllocation2.Should().BeEquivalentTo(expectedAllocationRecord2, options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 3000)).WhenTypeIs<DateTime>();
                return options;
            });
        }
    }
}
