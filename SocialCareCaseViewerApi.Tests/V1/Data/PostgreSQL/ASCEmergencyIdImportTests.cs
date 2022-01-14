using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates;
using System;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Data
{
    [TestFixture]
    public class ASCEmergencyIdImportTests : DatabaseTests
    {
        private readonly Fixture _fixture = new Fixture();

        /// <summary>
        /// Imports emergency ID person records from sccv_persons_import to dm_persons table and adds lookup records to sccv_persons_lookup table
        /// Must be run manually on the db server after sccv_persons_import table has been populated with the data from the service
        /// </summary>
        private static readonly FormattableString _queryUnderTest = $@"
                   do
                    $$
                            declare
                                    new_person record;
                                    new_person_mosaic_id bigint;
                            begin
                                    for new_person
                                    in SELECT
                                            imports.first_name,
                                            imports.last_name,
                                            imports.full_name,
                                            imports.date_of_birth,
                                            imports.person_id,
                                            imports.gender,
                                            imports.restricted,
                                            imports.context_flag

                            FROM
                                    dbo.SCCV_PERSONS_LOOKUP as lookup RIGHT OUTER JOIN
                                    dbo.SCCV_PERSONS_IMPORT as imports ON lookup.NC_ID = imports.PERSON_ID
                            WHERE lookup.PERSON_ID is null AND LOWER(imports.PERSON_ID) like 'tmp%'

                            loop
                                    new_person_mosaic_id = null;

                                    insert into dbo.dm_persons(
                                            first_name,
                                            last_name,
                                            full_name,
                                            date_of_birth,
                                            gender,
                                            restricted,
                                            context_flag,
                                            from_dm_person
                                    )
                                    values (
                                            new_person.first_name,
                                            new_person.last_name,
                                            new_person.full_name,
                                            new_person.date_of_birth,
                                            new_person.gender,
                                            new_person.restricted,
                                            new_person.context_flag,
                                            'N'
                                    )
                                    returning person_id into new_person_mosaic_id;

                                    insert into dbo.sccv_persons_lookup(person_id, nc_id)
                                            values (new_person_mosaic_id, new_person.person_id);

                                    raise notice '% - %', new_person.person_id, new_person_mosaic_id;
                            end loop;
                    end;
                    $$
                   ";

        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void ImportsNewPersonWithASCEmergencyIdFromPersonsImportTableToPersonTable()
        {
            var newPersonRecordToImport = _fixture.Build<PersonImport>().With(x => x.Id, "tmp123").Create();
            DatabaseContext.PersonImport.Add(newPersonRecordToImport);

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            DatabaseContext.Persons.Count().Should().Be(1);
        }

        [Test]
        public void CreatesNewLookupRecordInThePersonLookupTable()
        {
            var newPersonRecordToImport = _fixture.Build<PersonImport>().With(x => x.Id, "tmp123").Create();
            DatabaseContext.PersonImport.Add(newPersonRecordToImport);

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var importedPerson = DatabaseContext.Persons.First();

            var lookup = DatabaseContext.PersonLookups.First();

            lookup.NCId.Should().Be(newPersonRecordToImport.Id);
            lookup.MosaicId.Should().Be(importedPerson.Id.ToString());
        }

        [Test]
        public void CreatesNewPersonRecordWithCorrectValuesForSupportedColumns()
        {
            var dateOfBirth = new DateTime(2000, 1, 1, 15, 30, 0);

            var newPersonRecordToImport = _fixture.Build<PersonImport>()
                .With(x => x.Id, "tmp123")
                .With(x => x.DateOfBirth, dateOfBirth).Create();

            DatabaseContext.PersonImport.Add(newPersonRecordToImport);

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            var importedPerson = DatabaseContext.Persons.First();

            importedPerson.FirstName.Should().Be(newPersonRecordToImport.FirstName);
            importedPerson.LastName.Should().Be(newPersonRecordToImport.LastName);
            importedPerson.FullName.Should().Be(newPersonRecordToImport.FullName);
            importedPerson.DateOfBirth.Should().Be(newPersonRecordToImport.DateOfBirth);
            importedPerson.Gender.Should().Be(newPersonRecordToImport.Gender);
            importedPerson.Restricted.Should().Be(newPersonRecordToImport.Restricted);
            importedPerson.AgeContext.Should().Be(newPersonRecordToImport.AgeContext);
            importedPerson.DataIsFromDmPersonsBackup.Should().Be("N");
        }

        [Test]
        [TestCase("tmp123")]
        [TestCase("tmp003344")]

        public void OnlyImportsRecordsThatArePrefixedWithTmpAsRequestedByASC(string emergencyId)
        {
            var newPersonRecordToImport = _fixture.Build<PersonImport>().With(x => x.Id, emergencyId).Create();
            DatabaseContext.PersonImport.Add(newPersonRecordToImport);

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            DatabaseContext.Persons.Count().Should().Be(1);
        }

        [Test]
        [TestCase("nc123")]
        [TestCase("asc003344")]
        [TestCase("123tmp")]
        [TestCase("123")]

        public void DoesNotImportRecordsThatDoNotHaveTmpPrefix(string emergencyId)
        {
            var newPersonRecordToImport = _fixture.Build<PersonImport>().With(x => x.Id, emergencyId).Create();
            DatabaseContext.PersonImport.Add(newPersonRecordToImport);

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            DatabaseContext.Persons.Count().Should().Be(0);
        }
    }
}

