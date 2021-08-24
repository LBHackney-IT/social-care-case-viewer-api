using Bogus;
using SocialCareCaseViewerApi.V1.Domain;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers.Domain
{
    public static class CaseStatusDomainHelper
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

        public static CaseStatus NewCaseStatusForPerson(CaseStatusType caseStatusType)
        {
            return new Faker<CaseStatus>()
                .RuleFor(cs => cs.Type, f => caseStatusType.Name)
                .RuleFor(cs => cs.StartDate, f => f.Date.Past().ToString("s"))
                .RuleFor(cs => cs.EndDate, f => f.Date.Future().ToString("s"))
                .RuleFor(cs => cs.Notes, f => f.Lorem.Paragraph())
                .RuleFor(cs => cs.Fields, f =>
                    caseStatusType.Fields.Select(field => new CaseStatusField
                    {
                        Name = field.Name,
                        Description = field.Description,
                        SelectedOption = new CaseStatusFieldSelectedOption
                        {
                            Name = field.Options.First().Name,
                            Description = field.Options.First().Description
                        }
                    }).ToList()
                );
        }
    }
}
