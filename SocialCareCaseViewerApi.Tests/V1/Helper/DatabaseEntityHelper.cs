using AutoFixture;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Helper
{
    public static class DatabaseEntityHelper
    {
        public static Person CreateDatabaseEntity()
        {
            var entity = new Fixture().Create<Entity>();

            return CreateDatabaseEntityFrom(entity);
        }

        public static Person CreateDatabaseEntityFrom(Entity entity)
        {
            return new Person
            {
                Id = entity.Id,
                //CreatedAt = entity.CreatedAt,
            };
        }
    }
}
