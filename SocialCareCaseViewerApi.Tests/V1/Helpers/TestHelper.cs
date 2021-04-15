using Bogus;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class TestHelper
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

        public static CaseNote CreateCaseNote()
        {
            return new Faker<CaseNote>()
                .RuleFor(c => c.CaseNoteId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.MosaicId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.CreatedOn, f => f.Date.Past())
                .RuleFor(c => c.NoteType, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteContent, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(c => c.CreatedByEmail, f => f.Person.Email)
                .RuleFor(c => c.CreatedByName, f => f.Person.FirstName);
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
    }
}
