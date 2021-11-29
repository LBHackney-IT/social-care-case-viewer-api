
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class MashReferralHelper
    {
        public static MashReferral_2 SaveMashReferralToDatabase(DatabaseContext databaseContext)
        {
            var referral = TestHelpers.CreateMashReferral2();

            databaseContext.MashReferral_2.Add(referral);
            databaseContext.SaveChanges();

            return referral;
        }
    }
}
