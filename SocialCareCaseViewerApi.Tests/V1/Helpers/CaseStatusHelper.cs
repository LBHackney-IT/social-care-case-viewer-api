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
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var caseStatusTypeField = TestHelpers.CreateCaseStatusTypeField(caseStatusType);
            var person = TestHelpers.CreatePerson(3);
            var caseStatus = TestHelpers.CreateCaseStatus(
                personId: 3,
                typeId: caseStatusType.Id,
                startDate: DateTime.Today,
                notes: "Testing",
                options: new List<CaseStatusTypeFieldOption>() { caseStatusTypeField.Options.First() }
            );


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
            var caseStatusTypeField = TestHelpers.CreateCaseStatusTypeField(caseStatusType);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusTypeFields.Add(caseStatusTypeField);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
        }

        public static CreateCaseStatusRequest CreateCaseStatusRequest(
          long? personId = null,
          long? typeId = null,
          List<CaseStatusRequestField>? fields = null,
          DateTime? startDate = null,
          DateTime? endDate = null,
          string? notes = null,
          string? type = "CIN",          
          string? createdBy = null
        )
        {
            return new Faker<CreateCaseStatusRequest>()
                .RuleFor(pr => pr.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(pr => pr.Type, type)
                .RuleFor(pr => pr.TypeId, f => typeId ?? f.UniqueIndex)
                .RuleFor(pr => pr.Fields, f => fields ?? new List<CaseStatusRequestField>(){ new CaseStatusRequestField() { Name="FieldName", Selected="N0"}})
                .RuleFor(pr => pr.StartDate, f => startDate ?? DateTime.Today.AddDays(- 1))
                .RuleFor(pr => pr.EndDate, f => endDate ?? DateTime.Today.AddDays(1))
                .RuleFor(pr => pr.Notes, f => notes ?? f.Random.String2(1000))
                .RuleFor(pr => pr.CreatedBy, f => createdBy ?? f.Internet.Email());
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithMultipleCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");
            var csus2 = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, startDate: DateTime.Today.AddDays(-1), notes: "Testing");


            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);
            databaseContext.CaseStatuses.Add(csus2);

            databaseContext.SaveChanges();

            return (csus, person);
        }
    }
}
