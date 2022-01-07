using System;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class MashReferralHelper
    {
        public static MashReferral SaveMashReferralToDatabase(DatabaseContext databaseContext, string? stage = null, long? mashReferralId = null, string? referrer = null, DateTime? referralCreatedAt = null)
        {
            var referral = TestHelpers.CreateMashReferral(stage, mashReferralId, referrer, referralCreatedAt);
            databaseContext.MashReferrals.Add(referral);
            databaseContext.SaveChanges();
            return referral;
        }
    }
}
