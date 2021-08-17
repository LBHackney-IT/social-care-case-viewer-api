using System;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class CaseStatusHelper
    {
        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var person = TestHelpers.CreatePerson(3);
            var caseStatus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, startDate: DateTime.Today, notes: "Testing");


            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(caseStatus);

            databaseContext.SaveChanges();

            return (caseStatus, person);
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithPastCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
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
