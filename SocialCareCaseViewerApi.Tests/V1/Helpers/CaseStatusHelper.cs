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
            var caseStatusType = TestHelpers.CreateCaseStatusType();
            var caseStatusSubtype = TestHelpers.CreateCaseStatusSubtype(typeId: caseStatusType.Id);
            var person = TestHelpers.CreatePerson(3);
            var csus = TestHelpers.CreateCaseStatus(personId: 3, typeId: caseStatusType.Id, subtypeId: caseStatusSubtype.Id, startDate: DateTime.Now, notes: "Testing");


            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.CaseStatusSubtypes.Add(caseStatusSubtype);
            databaseContext.Persons.Add(person);
            databaseContext.CaseStatuses.Add(csus);

            databaseContext.SaveChanges();

            return (csus, person);
        }
    }
}
