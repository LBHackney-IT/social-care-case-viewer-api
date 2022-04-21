using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates;
using System;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Data.PostgreSQL
{
    [TestFixture]
    public class PersonReviewDateImportTests : DatabaseTests
    {
        //import pre-validated records. Data set must have only records with review date set
        private static readonly FormattableString _queryUnderTest = $@"
            UPDATE dbo.dm_persons p
            SET
                review_date = i.review_date,
                sccv_last_modified_by = 'person_review_date_review_team_update_script',
                sccv_last_modified_at = NOW()
            FROM dbo.sccv_person_review_date_import_review_team i
            WHERE p.person_id = i.mosaic_id and p.review_date is null;
           ";

        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void UpdatesPersonReviewDateBasedOnTheImportDataWithoutTouchingOtherRecords()
        {
            var person1 = TestHelpers.CreatePerson();
            person1.ReviewDate = null;

            var person2 = TestHelpers.CreatePerson();
            person2.ReviewDate = null;

            var person3 = TestHelpers.CreatePerson();
            var person3ReviewDate = person3.ReviewDate;

            var reviewDate1 = new PersonReviewDateImport()
            {
                MosaicId = person1.Id,
                Reviewdate = DateTime.Today.AddDays(20)
            };

            var reviewDate2 = new PersonReviewDateImport()
            {
                MosaicId = person2.Id,
                Reviewdate = DateTime.Today.AddDays(10)
            };

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.Persons.Add(person3);
            DatabaseContext.PersonReviewDateImports.Add(reviewDate1);
            DatabaseContext.PersonReviewDateImports.Add(reviewDate2);
            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedPerson1 = DatabaseContext.Persons.FirstOrDefault(x => x.Id == person1.Id);
            var updatedPerson2 = DatabaseContext.Persons.FirstOrDefault(x => x.Id == person2.Id);

            updatedPerson1.ReviewDate.Should().Be(reviewDate1.Reviewdate);
            updatedPerson1.LastModifiedBy.Should().Be("person_review_date_review_team_update_script");
            updatedPerson1.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, 3000);

            updatedPerson2.ReviewDate.Should().Be(reviewDate2.Reviewdate);
            updatedPerson2.LastModifiedBy.Should().Be("person_review_date_review_team_update_script");
            updatedPerson1.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, 3000);

            var unmodifiedPerson3 = DatabaseContext.Persons.FirstOrDefault(x => x.Id == person3.Id);
            unmodifiedPerson3.ReviewDate.Should().BeCloseTo(person3ReviewDate.Value, 1000);
        }

        [Test]
        public void DoesNotUpdatePersonsReviewDateIfAlreadySet()
        {
            var person = TestHelpers.CreatePerson(reviewDate: DateTime.Now.AddDays(30));

            var personReviewDate = person.ReviewDate;

            var reviewDate = new PersonReviewDateImport()
            {
                MosaicId = person.Id,
                Reviewdate = DateTime.Today.AddDays(10)
            };

            DatabaseContext.Persons.Add(person);
            DatabaseContext.PersonReviewDateImports.Add(reviewDate);
            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var updatedPerson1 = DatabaseContext.Persons.FirstOrDefault(x => x.Id == person.Id);

            updatedPerson1.ReviewDate.Should().BeCloseTo(personReviewDate.Value, 1000);
        }
    }
}
