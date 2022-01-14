using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class MashResidentHelper
    {
        public static MashResident SaveMashResidentToDatabase(DatabaseContext databaseContext, long? mashReferralId = null, long? socialCareId = null)
        {
            var resident = TestHelpers.CreateMashResident(mashReferralId);
            resident.SocialCareId = socialCareId;
            databaseContext.MashResidents.Add(resident);
            databaseContext.SaveChanges();
            return resident;
        }
    }
}
