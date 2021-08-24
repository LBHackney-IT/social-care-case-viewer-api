using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers.Gateway
{
    public static class CaseStatusGatewayHelper
    {
        public static CaseStatusType NewCaseStatusType()
        {
            return new Faker<CaseStatusType>()
                .RuleFor(cst => cst.Name, f => f.Lorem.Word())
                .RuleFor(cst => cst.Description, f => f.Lorem.Sentence())
                .RuleFor(cst => cst.Fields, f => new List<CaseStatusTypeField>
                {
                    new Faker<CaseStatusTypeField>()
                        .RuleFor(cstf => cstf.Name, f => f.Lorem.Word())
                        .RuleFor(cstf => cstf.Description, f => f.Lorem.Sentence())
                        .RuleFor(cstf => cstf.Options, f => new List<CaseStatusTypeFieldOption>
                        {
                            new Faker<CaseStatusTypeFieldOption>()
                                .RuleFor(cstfo => cstfo.Name, f => f.Lorem.Word())
                                .RuleFor(cstfo => cstfo.Description, f => f.Lorem.Sentence()),
                            new Faker<CaseStatusTypeFieldOption>()
                                .RuleFor(cstfo => cstfo.Name, f => f.Lorem.Word())
                                .RuleFor(cstfo => cstfo.Description, f => f.Lorem.Sentence())
                        })
                });
        }

        public static CaseStatusType StoreNewCaseStatusType(DatabaseContext databaseContext)
        {
            var caseStatusType = NewCaseStatusType();

            databaseContext.CaseStatusTypes.Add(caseStatusType);
            databaseContext.SaveChanges();

            return caseStatusType;
        }

        public static CaseStatus NewCaseStatusForPerson(
            CaseStatusType caseStatusType,
            Person person,
            DateTime? endDate = null
        )
        {
            return new Faker<CaseStatus>()
                .RuleFor(cs => cs.StartDate, f => f.Date.Recent())
                .RuleFor(cs => cs.EndDate, f => endDate ?? f.Date.Future())
                .RuleFor(cs => cs.Notes, f => f.Lorem.Paragraph())
                .RuleFor(cs => cs.Person, f => person)
                .RuleFor(cs => cs.Type, f => caseStatusType)
                .RuleFor(cs => cs.SelectedOptions,
                    (f, cs) => new List<CaseStatusFieldOption>
                    {
                        new CaseStatusFieldOption { FieldOption = caseStatusType.Fields.First().Options.First() }
                    });
        }

        public static CaseStatus StoreNewCaseStatusForPerson(
            DatabaseContext databaseContext,
            CaseStatusType caseStatusType,
            Person person,
            DateTime? endDate = null
        )
        {
            var caseStatus = NewCaseStatusForPerson(caseStatusType, person, endDate);

            databaseContext.CaseStatuses.Add(caseStatus);
            databaseContext.SaveChanges();

            return caseStatus;
        }
    }
}
