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
    public class DeletePersonRecordsTests : DatabaseTests
    {
        //TODO: wrap into transaction when running manually (BEGIN; <SCRIPT> END;)
        private static readonly FormattableString _queryUnderTest = $@"
                   UPDATE dbo.dm_persons p
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = p.person_id;
                  
                   UPDATE dbo.dm_addresses a
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = a.person_id;

                   UPDATE dbo.dm_telephone_numbers t
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = t.person_id;

                   UPDATE dbo.sccv_person_other_name o
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = o.person_id;

                   UPDATE dbo.sccv_allocations_combined al
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = al.mosaic_id;

                   UPDATE dbo.sccv_warning_note w
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = w.person_id;

                   UPDATE dbo.sccv_personal_relationship r
                   SET marked_for_deletion = true     
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = r.fk_person_id;

                   UPDATE dbo.sccv_personal_relationship ri
                   SET marked_for_deletion = true
                   FROM dbo.sccv_person_record_to_be_deleted d
                   WHERE d.fk_person_id_to_delete = ri.fk_other_person_id;

                   INSERT INTO dbo.sccv_deleted_person_record(deleted_person_id, master_person_id)
                   SELECT fk_person_id_to_delete, fk_master_person_id
                   FROM dbo.sccv_person_record_to_be_deleted;
                   ";

        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void SetsTheMarkedForDeletionFlagForPersonAndRelatedEntitiesAndUpdatesTheDeletedPersonRecordsTableWithCorrectRecords()
        {
            //person to be deleted, new master record and relationship
            var (personToBeDeleted, newMasterPersonRecord, personalRelationship, personalRelationshipType, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);

            //address
            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: personToBeDeleted.Id);
            DatabaseContext.Addresses.Add(address);

            //telephone numbers
            var telephoneNumber = DatabaseGatewayHelper.CreatePhoneNumberEntity(personId: personToBeDeleted.Id);
            DatabaseContext.PhoneNumbers.Add(telephoneNumber);

            //other names
            var otherName = DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(personId: personToBeDeleted.Id);
            DatabaseContext.PersonOtherNames.Add(otherName);

            //allocations
            var allocation = TestHelpers.CreateAllocation(personId: (int) personToBeDeleted.Id);
            DatabaseContext.Allocations.Add(allocation);

            //warning notes
            var warningNote = TestHelpers.CreateWarningNote(personId: personToBeDeleted.Id);
            DatabaseContext.WarningNotes.Add(warningNote);

            //inverse relationship (type not handled for simplicity)
            var inversePersonalRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(newMasterPersonRecord, personToBeDeleted, personalRelationshipType);
            DatabaseContext.PersonalRelationships.Add(inversePersonalRelationship);

            //record marked for deletion
            DatabaseContext.PersonRecordsToBeDeleted.Add(new PersonRecordToBeDeleted()
            {
                IdToDelete = personToBeDeleted.Id,
                MasterId = newMasterPersonRecord.Id
            });

            DatabaseContext.SaveChanges();

            DatabaseContext.Database.ExecuteSqlInterpolated(_queryUnderTest);

            //flags
            DatabaseContext.Persons.First(x => x.Id == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.Addresses.First(x => x.PersonId == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.PhoneNumbers.First(x => x.PersonId == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.PersonOtherNames.First(x => x.PersonId == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.Allocations.First(x => x.PersonId == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.WarningNotes.First(x => x.PersonId == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.PersonalRelationships.First(x => x.PersonId == personToBeDeleted.Id).MarkedForDeletion.Should().BeTrue();
            DatabaseContext.PersonalRelationships.First(x => x.OtherPersonId == newMasterPersonRecord.Id).MarkedForDeletion.Should().BeTrue();

            //record in deleted person record table
            var expectedDeletedPersonRecordEntry = new DeletedPersonRecord()
            {
                DeletedId = personToBeDeleted.Id,
                MasterId = newMasterPersonRecord.Id,
                Timestamp = DateTime.Now.ToUniversalTime()
            };

            DatabaseContext.DeletedPersonRecords.First(x => x.DeletedId == personToBeDeleted.Id).MasterId.Should().Be(expectedDeletedPersonRecordEntry.MasterId);
            DatabaseContext.DeletedPersonRecords.First(x => x.DeletedId == personToBeDeleted.Id).DeletedId.Should().Be(expectedDeletedPersonRecordEntry.DeletedId);
            DatabaseContext.DeletedPersonRecords.First(x => x.DeletedId == personToBeDeleted.Id).Timestamp.Should().BeCloseTo(expectedDeletedPersonRecordEntry.Timestamp, precision: 5000);
        }
    }
}

