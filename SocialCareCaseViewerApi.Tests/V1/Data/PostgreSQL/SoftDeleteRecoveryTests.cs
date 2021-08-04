using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Data.PostgreSQL
{
    public class SoftDeleteRecoveryTests : DatabaseTests
    {
        [SetUp]
        public void SetUp()
        {
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void SetsTheMarkedForDeletionFlagToFalseForPersonAndRelatedEntitiesAndRemovesTheRecordFromDeletedPersonRecordTable()
        {
            //person to be restored, new master record and relationship
            var (deletedPerson, newMasterPersonRecord, personalRelationship, personalRelationshipType, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);

            //address
            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: deletedPerson.Id);
            address.MarkedForDeletion = true;
            DatabaseContext.Addresses.Add(address);

            //telephone numbers
            var telephoneNumber = DatabaseGatewayHelper.CreatePhoneNumberEntity(personId: deletedPerson.Id);
            address.MarkedForDeletion = true;
            DatabaseContext.PhoneNumbers.Add(telephoneNumber);

            //other names
            var otherName = DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(personId: deletedPerson.Id);
            otherName.MarkedForDeletion = true;
            DatabaseContext.PersonOtherNames.Add(otherName);

            //allocations
            var allocation = TestHelpers.CreateAllocation(personId: (int) deletedPerson.Id);
            allocation.MarkedForDeletion = true;
            DatabaseContext.Allocations.Add(allocation);

            //warning notes
            var warningNote = TestHelpers.CreateWarningNote(personId: deletedPerson.Id);
            warningNote.MarkedForDeletion = true;
            DatabaseContext.WarningNotes.Add(warningNote);

            //inverse relationship (type not handled for simplicity)
            var inversePersonalRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(newMasterPersonRecord, deletedPerson, personalRelationshipType);
            inversePersonalRelationship.MarkedForDeletion = true;
            DatabaseContext.PersonalRelationships.Add(inversePersonalRelationship);

            //record marked as soft deleted
            DatabaseContext.DeletedPersonRecords.Add(new DeletedPersonRecord()
            {
                DeletedId = deletedPerson.Id,
                MasterId = newMasterPersonRecord.Id
            });

            DatabaseContext.SaveChanges();

            //TODO: wrap into transaction when running manually (BEGIN; <SCRIPT> END;)
            //TODO: this is designed to be run against individual IDs. Replace the {deletedPerson.Id} with appropriate MosaicId (long)
            DatabaseContext.Database.ExecuteSqlInterpolated($@"
                   UPDATE dbo.dm_persons SET marked_for_deletion = false WHERE person_id = {deletedPerson.Id};
                   UPDATE dbo.dm_addresses SET marked_for_deletion = false WHERE person_id = {deletedPerson.Id};
                   UPDATE dbo.dm_telephone_numbers SET marked_for_deletion = false WHERE person_id = {deletedPerson.Id};
                   UPDATE dbo.sccv_person_other_name SET marked_for_deletion = false WHERE person_id = {deletedPerson.Id};
                   UPDATE dbo.sccv_allocations_combined SET marked_for_deletion = false WHERE mosaic_id = {deletedPerson.Id};
                   UPDATE dbo.sccv_warning_note SET marked_for_deletion = false WHERE person_id = {deletedPerson.Id};
                   UPDATE dbo.sccv_personal_relationship SET marked_for_deletion = false WHERE fk_person_id = {deletedPerson.Id};
                   UPDATE dbo.sccv_personal_relationship SET marked_for_deletion = false WHERE fk_other_person_id = {deletedPerson.Id};
                   DELETE from dbo.sccv_deleted_person_record where deleted_person_id = {deletedPerson.Id};
            ");

            DatabaseContext.Persons.First(x => x.Id == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.Addresses.First(x => x.PersonId == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.PhoneNumbers.First(x => x.PersonId == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.PersonOtherNames.First(x => x.PersonId == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.Allocations.First(x => x.PersonId == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.WarningNotes.First(x => x.PersonId == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.PersonalRelationships.First(x => x.PersonId == deletedPerson.Id).MarkedForDeletion.Should().BeFalse();
            DatabaseContext.PersonalRelationships.First(x => x.OtherPersonId == newMasterPersonRecord.Id).MarkedForDeletion.Should().BeFalse();

            DatabaseContext.DeletedPersonRecords.Any(x => x.DeletedId == deletedPerson.Id).Should().BeFalse();
        }
    }
}
