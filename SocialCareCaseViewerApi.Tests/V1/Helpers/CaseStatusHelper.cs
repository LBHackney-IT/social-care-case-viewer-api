using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class CaseStatusHelper
    {
        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusTypeField = TestHelpers.CreateCaseStatusTypeField(caseStatusType.Id);
            var person = TestHelpers.CreatePerson();
            var caseStatus = TestHelpers.CreateCaseStatus(person.Id, caseStatusType.Id, options: new List<CaseStatusTypeFieldOption> { caseStatusTypeField.Options.First() }, resident: person);

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusTypeFields.Add(caseStatusTypeField);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(caseStatus);

            databaseContext.SaveChanges();

            return (caseStatus, person);
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithPastCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var caseStatusTypeField = TestHelpers.CreateCaseStatusTypeField(caseStatusType.Id);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusTypeFields.Add(caseStatusTypeField);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
        }

        public static CaseStatus CreateCaseStatus(
          long? personId = null,
          long? typeId = null,
          DateTime? startDate = null,
          DateTime? endDate = null,
          string? notes = null,
          string? createdBy = null
        )
        {
            return new Faker<CaseStatus>()
              .RuleFor(pr => pr.PersonId, f => personId ?? f.UniqueIndex + 1)
              .RuleFor(pr => pr.TypeId, f => typeId ?? f.UniqueIndex + 1)
              .RuleFor(pr => pr.StartDate, f => startDate ?? DateTime.Today.AddDays(-1))
              .RuleFor(pr => pr.EndDate, f => endDate ?? DateTime.Today.AddDays(1))
              .RuleFor(pr => pr.Notes, f => notes ?? f.Random.String2(1000))
              .RuleFor(pr => pr.CreatedBy, f => createdBy ?? f.Internet.Email());
        }

        public static CreateCaseStatusRequest CreateCaseStatusRequest(
          long? personId = null,
          string? type = null,
          List<CaseStatusRequestField>? fields = null,
          DateTime? startDate = null,
          string? notes = null,
          string? createdBy = null
        )
        {
            return new Faker<CreateCaseStatusRequest>()
                .RuleFor(pr => pr.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(pr => pr.Type, f => type ?? "CIN")
                .RuleFor(pr => pr.Fields, f => fields ?? new List<CaseStatusRequestField>() { new CaseStatusRequestField() { Name = "placementReason", Selected = "N0" } })
                .RuleFor(pr => pr.StartDate, f => startDate ?? DateTime.Today.AddDays(-1))
                .RuleFor(pr => pr.Notes, f => notes ?? f.Random.String2(1000))
                .RuleFor(pr => pr.CreatedBy, f => createdBy ?? f.Internet.Email());
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithMultipleCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var person = TestHelpers.CreatePerson();
            var caseStatus = TestHelpers.CreateCaseStatus(person.Id, caseStatusType.Id, resident: person);
            var caseStatus2 = TestHelpers.CreateCaseStatus(person.Id, caseStatusType.Id, resident: person);

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(caseStatus);
            databaseContext.CaseStatuses.Add(caseStatus2);

            databaseContext.SaveChanges();

            return (caseStatus, person);
        }

        public static (CaseStatusType, CaseStatusTypeField, CaseStatusTypeFieldOption) SaveCaseStatusFieldsToDatabase(DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 999, name: "CSTYPE");
            var caseStatusTypeField = TestHelpers.CreateCaseStatusTypeField(caseStatusType.Id, name: "reason", description: "What's the reason?");
            var caseOptions = TestHelpers.CreateCaseStatusTypeFieldOptions(caseStatusTypeField.Id, name: "N0", description: "value of the reason");

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusTypeFields.Add(caseStatusTypeField);
            databaseContext.CaseStatusTypeFieldOptions.Add(caseOptions);

            databaseContext.SaveChanges();

            return (caseStatusType, caseStatusTypeField, caseOptions);
        }

        public static DateTime? TrimMilliseconds(DateTime? dt)
        {
            if (dt == null) return null;
            return new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, dt.Value.Hour, dt.Value.Minute, dt.Value.Second, 0, dt.Value.Kind);
        }
    }
}
