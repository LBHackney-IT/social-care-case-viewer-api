using Bogus;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class TestHelper
    {
        public static Visit CreateVisitEntity()
        {
            return new Faker<Visit>()
                .RuleFor(v => v.Id, f => f.UniqueIndex.ToString())
                .RuleFor(v => v.Content, f => f.Random.String2(200))
                .RuleFor(v => v.Title, f => f.Random.String2(50))
                .RuleFor(v => v.CreatedOn, f => f.Date.Past(1))
                .RuleFor(v => v.CreatedByEmail, f => f.Person.Email)
                .RuleFor(v => v.MosaicId, f => f.UniqueIndex.ToString());
        }
    }
}
