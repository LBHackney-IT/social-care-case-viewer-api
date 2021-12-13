using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class MashResidentHelper
    {
        public static MashResident SaveMashResidentToDatabase(DatabaseContext databaseContext, long? mashReferralId = null)
        {
            var resident = TestHelpers.CreateMashResident(mashReferralId);
            databaseContext.MashResidents.Add(resident);
            databaseContext.SaveChanges();
            return resident;
        }
    }
}
