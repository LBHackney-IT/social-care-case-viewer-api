using System;
using System.Collections.Generic;
using Bogus;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class CaseStatusHelper
    {
        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person, List<CaseStatusAnswer>) SavePersonWithCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var person = TestHelpers.CreatePerson();
            var caseStatus = TestHelpers.CreateCaseStatus(person.Id, resident: person);

            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(caseStatus);

            databaseContext.SaveChanges();

            var caseStatusAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id);
            databaseContext.CaseStatusAnswers.AddRange(caseStatusAnswers);

            databaseContext.SaveChanges();

            return (caseStatus, person, caseStatusAnswers);
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithPastCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");

            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
        }

        public static CaseStatus CreateCaseStatus(
          long? personId = null,
          DateTime? startDate = null,
          DateTime? endDate = null,
          string? notes = null,
          string? createdBy = null,
          string? type = null
        )
        {
            return new Faker<CaseStatus>()
              .RuleFor(pr => pr.PersonId, f => personId ?? f.UniqueIndex + 1)
              .RuleFor(pr => pr.StartDate, f => startDate ?? DateTime.Today.AddDays(-1))
              .RuleFor(pr => pr.EndDate, f => endDate ?? DateTime.Today.AddDays(1))
              .RuleFor(pr => pr.Notes, f => notes ?? f.Random.String2(1000))
              .RuleFor(pr => pr.CreatedBy, f => createdBy ?? f.Internet.Email())
              .RuleFor(pr => pr.Answers, new List<CaseStatusAnswer>())
              .RuleFor(pr => pr.Type, f => type ?? f.Random.String2(3));
        }

        public static CreateCaseStatusRequest CreateCaseStatusRequest(
          long? personId = null,
          string? type = null,
          List<CaseStatusRequestAnswers>? answers = null,
          DateTime? startDate = null,
          string? notes = null,
          string? createdBy = null
        )
        {
            return new Faker<CreateCaseStatusRequest>()
                .RuleFor(pr => pr.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(pr => pr.Type, f => type ?? "CIN")
                .RuleFor(pr => pr.Answers, f => answers ?? new List<CaseStatusRequestAnswers>() { new CaseStatusRequestAnswers() { Option = "placementType", Value = "C2" } })
                .RuleFor(pr => pr.StartDate, f => startDate ?? DateTime.Today.AddDays(-1))
                .RuleFor(pr => pr.Notes, f => notes ?? f.Random.String2(1000))
                .RuleFor(pr => pr.CreatedBy, f => createdBy ?? f.Internet.Email());
        }

        public static CreateCaseStatusAnswerRequest CreateCaseStatusAnswerRequest(
            List<CaseStatusRequestAnswers>? answers = null,
            long? caseStatusId = null,
            string? createdBy = null,
            DateTime? startDate = null
           )
        {
            return new Faker<CreateCaseStatusAnswerRequest>()
                .RuleFor(a => a.Answers, f => answers ?? new List<CaseStatusRequestAnswers>() { new CaseStatusRequestAnswers() { Option = "placementType", Value = "C2" } })
                .RuleFor(a => a.CaseStatusId, f => caseStatusId ?? f.UniqueIndex)
                .RuleFor(a => a.CreatedBy, f => createdBy ?? f.Internet.Email())
                .RuleFor(a => a.StartDate, f => startDate ?? DateTime.Today.AddDays(-5));
        }

        public static List<CaseStatusRequestAnswers> CreateCaseStatusRequestAnswers(int? min = null, int? max = null)
        {
            var caseStatusRequestAnswers = new List<CaseStatusRequestAnswers>();

            for (var i = 0; i < new Random().Next(min ?? 1, max ?? 5); i++)
            {
                var answer = new Faker<CaseStatusRequestAnswers>()
                    .RuleFor(a => a.Option, f => f.Random.String2(100))
                    .RuleFor(a => a.Value, f => f.Random.String2(100));

                caseStatusRequestAnswers.Add(answer);
            }

            return caseStatusRequestAnswers;
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithMultipleCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var person = TestHelpers.CreatePerson();
            var caseStatus = TestHelpers.CreateCaseStatus(person.Id, resident: person);
            var caseStatus2 = TestHelpers.CreateCaseStatus(person.Id, resident: person);

            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(caseStatus);
            databaseContext.CaseStatuses.Add(caseStatus2);

            databaseContext.SaveChanges();

            return (caseStatus, person);
        }

        public static DateTime? TrimMilliseconds(DateTime? dt)
        {
            if (dt == null) return null;
            return new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, dt.Value.Hour, dt.Value.Minute, dt.Value.Second, 0, dt.Value.Kind);
        }
    }
}
