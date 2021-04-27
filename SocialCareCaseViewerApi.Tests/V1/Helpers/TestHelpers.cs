using System;
using System.Collections.Generic;
using Bogus;
using Bogus.DataSets;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using InfrastructurePerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class TestHelpers
    {
        public static Visit CreateVisit()
        {
            return new Faker<Visit>()
                .RuleFor(v => v.VisitId, f => f.UniqueIndex)
                .RuleFor(v => v.PersonId, f => f.UniqueIndex)
                .RuleFor(v => v.VisitType, f => f.Random.String2(1, 20))
                .RuleFor(v => v.PlannedDateTime, f => f.Date.Past())
                .RuleFor(v => v.ActualDateTime, f => f.Date.Past())
                .RuleFor(v => v.ReasonNotPlanned, f => f.Random.String2(1, 16))
                .RuleFor(v => v.ReasonVisitNotMade, f => f.Random.String2(1, 16))
                .RuleFor(v => v.SeenAloneFlag, f => f.Random.Bool())
                .RuleFor(v => v.CompletedFlag, f => f.Random.Bool())
                .RuleFor(v => v.CpRegistrationId, f => f.UniqueIndex)
                .RuleFor(v => v.CpVisitScheduleStepId, f => f.UniqueIndex)
                .RuleFor(v => v.CpVisitScheduleDays, f => f.Random.Number(999))
                .RuleFor(v => v.CpVisitOnTime, f => f.Random.Bool())
                .RuleFor(v => v.CreatedByEmail, f => f.Person.Email)
                .RuleFor(v => v.CreatedByName, f => f.Person.FullName);
        }

        public static CaseNote CreateCaseNote(string? noteType = null, DateTime? createdOn = null)
        {
            return new Faker<CaseNote>()
                .RuleFor(c => c.CaseNoteId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.MosaicId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.CreatedOn, f => createdOn ?? f.Date.Past())
                .RuleFor(c => c.NoteType, f => noteType ?? f.Random.String2(50))
                .RuleFor(c => c.CaseNoteContent, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(c => c.CreatedByEmail, f => f.Person.Email)
                .RuleFor(c => c.CreatedByName, f => f.Person.FirstName);
        }

        public static CaseNoteResponse CreateCaseNoteResponse(CaseNote? caseNote)
        {
            return new Faker<CaseNoteResponse>()
                .RuleFor(c => c.RecordId, f => caseNote?.CaseNoteId ?? f.UniqueIndex.ToString())
                .RuleFor(c => c.PersonId, f => caseNote?.MosaicId ?? f.UniqueIndex.ToString())
                .RuleFor(c => c.Title, f => caseNote?.CaseNoteTitle ?? f.Lorem.Lines(1))
                .RuleFor(c => c.Content, f => caseNote?.CaseNoteContent ?? f.Lorem.Paragraph())
                .RuleFor(c => c.DateOfEvent, f => caseNote?.CreatedOn.ToString("s") ?? f.Date.Recent().ToString("s"))
                .RuleFor(c => c.OfficerEmail, f => caseNote?.CreatedByEmail ?? f.Person.Email)
                .RuleFor(c => c.OfficerName, f => caseNote?.CreatedByName ?? f.Person.FullName)
                .RuleFor(c => c.FormName, f => caseNote?.NoteType ?? f.Random.String2(10));
        }

        public static ResidentHistoricRecord CreateResidentHistoricRecord(long? personId = null)
        {
            return new Faker<ResidentHistoricRecord>()
                .RuleFor(r => r.RecordId, f => f.UniqueIndex)
                .RuleFor(r => r.FormName, f => f.Random.String2(50))
                .RuleFor(r => r.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(r => r.FirstName, f => f.Person.FirstName)
                .RuleFor(r => r.LastName, f => f.Person.LastName)
                .RuleFor(r => r.DateOfBirth, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.OfficerEmail, f => f.Person.Email)
                .RuleFor(r => r.CaseFormUrl, f => f.Internet.Url())
                .RuleFor(r => r.CaseFormTimeStamp, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.DateOfEvent, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(r => r.RecordType, f => f.PickRandom<RecordType>())
                .RuleFor(r => r.IsHistoric, true);
        }

        public static ResidentHistoricRecordCaseNote CreateResidentHistoricRecordCaseNote(long? personId = null)
        {
            var caseNote = CreateCaseNote();

            return new Faker<ResidentHistoricRecordCaseNote>()
                .RuleFor(r => r.RecordId, f => f.UniqueIndex)
                .RuleFor(r => r.FormName, f => f.Random.String2(50))
                .RuleFor(r => r.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(r => r.FirstName, f => f.Person.FirstName)
                .RuleFor(r => r.LastName, f => f.Person.LastName)
                .RuleFor(r => r.DateOfBirth, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.OfficerEmail, f => f.Person.Email)
                .RuleFor(r => r.CaseFormUrl, f => f.Internet.Url())
                .RuleFor(r => r.CaseFormTimeStamp, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.DateOfEvent, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.CaseNoteTitle, "Historical Case Note Title")
                .RuleFor(r => r.RecordType, f => f.PickRandom<RecordType>())
                .RuleFor(r => r.IsHistoric, true)
                .RuleFor(r => r.CaseNote, caseNote);
        }

        public static ResidentHistoricRecordVisit CreateResidentHistoricRecordVisit(long? personId = null)
        {
            var visit = CreateVisit();

            return new Faker<ResidentHistoricRecordVisit>()
                .RuleFor(r => r.RecordId, f => f.UniqueIndex)
                .RuleFor(r => r.FormName, f => f.Random.String2(50))
                .RuleFor(r => r.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(r => r.FirstName, f => f.Person.FirstName)
                .RuleFor(r => r.LastName, f => f.Person.LastName)
                .RuleFor(r => r.DateOfBirth, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.OfficerEmail, f => f.Person.Email)
                .RuleFor(r => r.CaseFormUrl, f => f.Internet.Url())
                .RuleFor(r => r.CaseFormTimeStamp, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.DateOfEvent, f => f.Date.Past().ToString("s"))
                .RuleFor(r => r.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(r => r.RecordType, f => f.PickRandom<RecordType>())
                .RuleFor(r => r.IsHistoric, true)
                .RuleFor(r => r.Visit, visit);
        }

        public static (CreateAllocationRequest, Worker, Worker, InfrastructurePerson, Team) CreateAllocationRequest(
            int? mosaicId = null,
            int? teamId = null,
            int? workerId = null,
            string? createdBy = null
            )
        {
            var worker = CreateWorker();
            var createdByWorker = CreateWorker();
            var person = CreatePerson();
            var team = CreateTeam();

            var createAllocationRequest = new Faker<CreateAllocationRequest>()
                .RuleFor(c => c.MosaicId, f => mosaicId ?? person.Id)
                .RuleFor(c => c.AllocatedTeamId, f => teamId ?? team.Id)
                .RuleFor(c => c.AllocatedWorkerId, f => workerId ?? worker.Id)
                .RuleFor(c => c.CreatedBy, f => createdBy ?? createdByWorker.Email)
                .RuleFor(c => c.AllocationStartDate, DateTime.Now);

            return (createAllocationRequest, worker, createdByWorker, person, team);
        }

        public static (UpdateAllocationRequest, Worker, Worker, InfrastructurePerson, Team) CreateUpdateAllocationRequest(
            int? id = null,
            string? deallocationReason = null,
            string? createdBy = null,
            DateTime? deallocationDate = null
            )
        {
            var worker = CreateWorker();
            var updatedByWorker = CreateWorker();
            var person = CreatePerson();
            var team = CreateTeam();

            var updateAllocationRequest = new Faker<UpdateAllocationRequest>()
                .RuleFor(u => u.Id, f => id ?? f.UniqueIndex + 1)
                .RuleFor(u => u.DeallocationReason, f => deallocationReason ?? f.Random.String2(200))
                .RuleFor(u => u.CreatedBy, createdBy ?? updatedByWorker.Email)
                .RuleFor(u => u.DeallocationDate, f => deallocationDate ?? f.Date.Recent());

            return (updateAllocationRequest, worker, updatedByWorker, person, team);
        }

        private static Worker CreateWorker(int? workerId = null)
        {
            return new Faker<Worker>()
                .RuleFor(w => w.Id, f => workerId ?? f.UniqueIndex + 1)
                .RuleFor(w => w.Email, f => f.Person.Email)
                .RuleFor(w => w.FirstName, f => f.Person.FirstName)
                .RuleFor(w => w.LastName, f => f.Person.LastName);
        }

        public static InfrastructurePerson CreatePerson(int? personId = null)
        {
            return new Faker<InfrastructurePerson>()
                .RuleFor(p => p.Id, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(p => p.Title, f => f.Name.Prefix())
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.FirstName)
                .RuleFor(p => p.FullName, f => f.Person.FullName)
                .RuleFor(p => p.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(p => p.DateOfDeath, f => f.Date.Recent())
                .RuleFor(p => p.Ethnicity, f => f.Random.String2(0, 30))
                .RuleFor(p => p.FirstLanguage, f => f.Random.String2(10, 100))
                .RuleFor(p => p.Religion, f => f.Random.String2(10, 80))
                .RuleFor(p => p.EmailAddress, f => f.Person.Email)
                .RuleFor(p => p.Gender, f => f.Random.String2(1, "MF"))
                .RuleFor(p => p.Nationality, f => f.Address.Country())
                .RuleFor(p => p.NhsNumber, f => f.Random.Number(int.MaxValue))
                .RuleFor(p => p.PersonIdLegacy, f => f.Random.String2(16))
                .RuleFor(p => p.AgeContext, f => f.Random.String2(1))
                .RuleFor(p => p.DataIsFromDmPersonsBackup, f => f.Random.String2(1))
                .RuleFor(p => p.SexualOrientation, f => f.Random.String2(100))
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.String2(100))
                .RuleFor(p => p.Restricted, f => f.Random.String2(1));
        }

        public static Address CreateAddress(long? personId = null)
        {
            var person = CreatePerson();

            return new Faker<Address>()
                .RuleFor(a => a.Person, person)
                .RuleFor(a => a.PersonAddressId, f => f.Random.Number(int.MaxValue))
                .RuleFor(a => a.AddressId, f => f.Random.Number(999999999))
                .RuleFor(a => a.PersonId, personId ?? person.Id)
                .RuleFor(a => a.EndDate, f => f.Date.Recent())
                .RuleFor(a => a.AddressLines, f => f.Address.FullAddress())
                .RuleFor(a => a.PostCode, f => f.Address.ZipCode())
                .RuleFor(a => a.Uprn, f => f.Random.Number(int.MaxValue))
                .RuleFor(a => a.DataIsFromDmPersonsBackup, f => f.Random.String2(1, "YN"))
                .RuleFor(a => a.IsDisplayAddress, f => f.Random.String2(1, "YN"));
        }

        public static PhoneNumber CreatePhoneNumber(long? personId = null)
        {
            var person = CreatePerson();

            return new Faker<PhoneNumber>()
                .RuleFor(p => p.Person, person)
                .RuleFor(p => p.Id, f => f.UniqueIndex)
                .RuleFor(p => p.PersonId, personId ?? person.Id)
                .RuleFor(p => p.Number, f => f.Random.Number(int.MaxValue).ToString())
                .RuleFor(p => p.Type, f => f.Random.String2(1, 80));
        }

        public static PersonOtherName CreatePersonOtherName(long? personId = null)
        {
            var person = CreatePerson();

            return new Faker<PersonOtherName>()
                .RuleFor(p => p.Person, person)
                .RuleFor(p => p.Id, f => f.UniqueIndex)
                .RuleFor(p => p.PersonId, personId ?? person.Id)
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.LastName);
        }

        public static AddNewResidentResponse CreateAddNewResidentResponse(
            long? personId = null,
            long? addressId = null,
            string? caseNoteId = null,
            List<int>? otherNameIds = null,
            List<int>? phoneNumberIds = null
            )
        {
            var person = CreatePerson();
            var address = CreateAddress();
            var caseNote = CreateCaseNote();

            return new Faker<AddNewResidentResponse>()
                .RuleFor(a => a.Id, personId ?? person.Id)
                .RuleFor(a => a.AddressId, addressId ?? address.AddressId)
                .RuleFor(a => a.OtherNameIds, otherNameIds ?? new Faker<List<int>>())
                .RuleFor(a => a.PhoneNumberIds, phoneNumberIds ?? new Faker<List<int>>())
                .RuleFor(a => a.CaseNoteId, caseNoteId ?? caseNote.CaseNoteId)
                .RuleFor(a => a.CaseNoteErrorMessage, f => f.Random.String2(100));
        }

        private static Team CreateTeam(int? teamId = null)
        {
            return new Faker<Team>()
                .RuleFor(t => t.Id, f => teamId ?? f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => f.Random.String2(1))
                .RuleFor(t => t.Name, f => f.Random.String2(1, 200));
        }

        public static WarningNote CreateWarningNote(long? personId = null, string? status = null)
        {
            return new Faker<WarningNote>()
                .RuleFor(w => w.Id, f => f.UniqueIndex + 1)
                .RuleFor(w => w.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(w => w.StartDate, f => f.Date.Recent())
                .RuleFor(w => w.ReviewDate, f => f.Date.Recent())
                .RuleFor(w => w.EndDate, f => f.Date.Recent())
                .RuleFor(w => w.LastReviewDate, f => f.Date.Recent())
                .RuleFor(w => w.NextReviewDate, f => f.Date.Recent())
                .RuleFor(w => w.DisclosedWithIndividual, f => f.Random.Bool())
                .RuleFor(w => w.DisclosedDetails, f => f.Random.String2(1, 1000))
                .RuleFor(w => w.Notes, f => f.Random.String2(1, 1000))
                .RuleFor(w => w.NoteType, f => f.Random.String2(1, 50))
                .RuleFor(w => w.Status, f => status ?? "open")
                .RuleFor(w => w.DisclosedDate, f => f.Date.Recent())
                .RuleFor(w => w.DisclosedHow, f => f.Random.String2(1, 50))
                .RuleFor(w => w.WarningNarrative, f => f.Random.String2(1, 1000))
                .RuleFor(w => w.ManagerName, f => f.Random.String2(1, 100))
                .RuleFor(w => w.DiscussedWithManagerDate, f => f.Date.Recent());
        }

        public static (PatchWarningNoteRequest, InfrastructurePerson, Worker, WarningNote) CreatePatchWarningNoteRequest(
            long? warningNoteId = null,
            DateTime? reviewDate = null,
            DateTime? nextReviewDate = null,
            string? startingStatus = "open",
            string? requestStatus = "open",
            DateTime? endedDate = null,
            string? reviewNotes = null,
            string? managerName = null,
            DateTime? discussedWithManagerDate = null
        )
        {
            var person = CreatePerson();
            var worker = CreateWorker();
            WarningNote warningNote = CreateWarningNote(personId: person.Id, status: startingStatus);

            var patchWarningNoteRequest = new Faker<PatchWarningNoteRequest>()
                .RuleFor(p => p.WarningNoteId, f => warningNoteId ?? warningNote.Id)
                .RuleFor(p => p.ReviewDate, f => reviewDate ?? f.Date.Recent())
                .RuleFor(p => p.ReviewedBy, f => worker.Email ?? f.Person.Email)
                .RuleFor(p => p.NextReviewDate, f => nextReviewDate ?? f.Date.Future())
                .RuleFor(p => p.Status, f => requestStatus)
                .RuleFor(p => p.EndedDate, f => endedDate ?? f.Date.Recent())
                .RuleFor(p => p.EndedBy, f => worker.Email ?? f.Person.Email)
                .RuleFor(p => p.ReviewNotes, f => reviewNotes ?? f.Random.String2(1, 1000))
                .RuleFor(p => p.ManagerName, f => managerName ?? f.Random.String2(1, 100))
                .RuleFor(p => p.DiscussedWithManagerDate, f => discussedWithManagerDate ?? f.Date.Recent());

            return (patchWarningNoteRequest, person, worker, warningNote);
        }
    }
}
