using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MashReferralGateway : IMashReferralGateway
    {
        private readonly IMongoGateway _mongoGateway;
        private static readonly string _collectionName = MongoConnectionStrings.Map[Collection.MashReferrals];

        public MashReferralGateway(IMongoGateway mongoGateway)
        {
            _mongoGateway = mongoGateway;
        }

        public void Reset()
        {
            _mongoGateway.DropCollection(_collectionName);
        }

        public void InsertDocument(MashReferral referral)
        {
            _mongoGateway.InsertRecord(_collectionName, referral);
        }
    }
}
