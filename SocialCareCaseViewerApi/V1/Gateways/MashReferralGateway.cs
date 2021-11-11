using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using MashReferral = SocialCareCaseViewerApi.V1.Domain.MashReferral;
using MashReferral_2 = SocialCareCaseViewerApi.V1.Domain.MashReferral_2;


#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MashReferralGateway : IMashReferralGateway
    {
        private readonly IMongoGateway _mongoGateway;
        private static readonly string _collectionName = MongoConnectionStrings.Map[Collection.MashReferrals];

        private readonly DatabaseContext _databaseContext;

        public MashReferralGateway(IMongoGateway mongoGateway, DatabaseContext databaseContext)
        {
            _mongoGateway = mongoGateway;
            _databaseContext = databaseContext;

        }

        public IEnumerable<MashReferral> GetReferralsUsingFilter(FilterDefinition<Infrastructure.MashReferral> filter)
        {
            return _mongoGateway
                .LoadMashReferralsByFilter(_collectionName, filter)
                .Select(x => x.ToDomain());
        }

        public MashReferral? GetReferralUsingId(string requestId)
        {
            return _mongoGateway
                .LoadRecordById<Infrastructure.MashReferral?>(_collectionName, ObjectId.Parse(requestId))
                ?.ToDomain();
        }

        public Infrastructure.MashReferral? GetInfrastructureUsingId(string requestId)
        {
            return _mongoGateway
                .LoadRecordById<Infrastructure.MashReferral?>(_collectionName, ObjectId.Parse(requestId));
        }

        public void UpsertRecord(Infrastructure.MashReferral referral)
        {
            _mongoGateway.UpsertRecord(_collectionName, referral.Id, referral);
        }

        public void Reset()
        {
            _mongoGateway.DropCollection(_collectionName);
        }

        public void InsertDocument(Infrastructure.MashReferral referral)
        {
            _mongoGateway.InsertRecord(_collectionName, referral);
        }

        public MashReferral_2? GetReferralUsingId_2(string requestId)
        {
            return _databaseContext.MashReferral_2
                    .Where(x => x.Id.ToString() == requestId)
                    .FirstOrDefault().ToDomain();

        }

        // public MashResident? GetMashResidentsByReferral(string requestId)        {
        //     return _databaseContext.MashResident
        //             .Where(x => x.ReferralId.ToString() == requestId)
        //             .FirstOrDefault();

        // }

    }
}
