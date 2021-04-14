using Bogus;
using SocialCareCaseViewerApi.V1.Domain;

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
                .RuleFor(v => v.PlannedDateTime, f => f.Date.Past(1).ToString("s"))
                .RuleFor(v => v.ActualDateTime, f => f.Date.Past(1).ToString("s"))
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
                .RuleFor(c => c.CreatedOn, f => f.Date.Past(1))
                .RuleFor(c => c.NoteType, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteContent, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(c => c.CreatedByEmail, f => f.Person.Email)
                .RuleFor(c => c.CreatedByName, f => f.Person.FirstName);
        }
    }
}
