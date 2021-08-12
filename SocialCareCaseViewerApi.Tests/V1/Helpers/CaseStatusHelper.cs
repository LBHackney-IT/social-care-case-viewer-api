using System;
using System.Collections.Generic;
using Bogus;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using dbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class CaseStatusHelper
    {
        public static (CaseStatusType, CaseStatusSubtype) SaveCaseStatusTypeToDatabase(
            DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id);

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusSubtypes.Add(caseStatusSubtype);

            databaseContext.SaveChanges();

            return (caseStatusType, caseStatusSubtype);
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id, id: 20);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Now, notes: "Testing");


            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusSubtypes.Add(caseStatusSubtype);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithPastCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id, id: 20);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusSubtypes.Add(caseStatusSubtype);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
        }

        public static (CaseStatus, SocialCareCaseViewerApi.V1.Infrastructure.Person) SavePersonWithMultipleCaseStatusToDatabase(
          DatabaseContext databaseContext)
        {
            var caseStatusType = TestHelpers.CreateCaseStatusType(id: 10);
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id, id: 20);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Today.AddDays(-2), endDate: DateTime.Today.AddDays(-1), notes: "Testing");
            var csus2 = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Today.AddDays(-1), notes: "Testing");


            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusSubtypes.Add(caseStatusSubtype);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);
            databaseContext.CaseStatuses.Add(csus2);

            databaseContext.SaveChanges();

            return (csus, person);
        }
    }
}
