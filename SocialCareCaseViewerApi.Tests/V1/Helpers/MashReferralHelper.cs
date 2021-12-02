
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class MashReferralHelper
    {
        public static MashReferral_2 SaveMashReferralToDatabase(DatabaseContext databaseContext, string? stage = null)
        {
            var referral = TestHelpers.CreateMashReferral2(stage);

            databaseContext.MashReferral_2.Add(referral);
            databaseContext.SaveChanges();

            return referral;
        }
    }
}
