using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using MashReferral = SocialCareCaseViewerApi.V1.Domain.MashReferral;
using MashReferral_2 = SocialCareCaseViewerApi.V1.Domain.MashReferral_2;


#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MashReferralGateway : IMashReferralGateway
    {
        private readonly IMongoGateway _mongoGateway;
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemTime _systemTime;
        private static readonly string _collectionName = MongoConnectionStrings.Map[Collection.MashReferrals];

        private readonly DatabaseContext _databaseContext;

        public MashReferralGateway(IMongoGateway mongoGateway, IDatabaseGateway databaseGateway, ISystemTime systemTime, DatabaseContext databaseContext)
        {
            _mongoGateway = mongoGateway;
            _databaseGateway = databaseGateway;
            _systemTime = systemTime;
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

        public MashReferral_2? GetReferralUsingId_2(long requestId)
        {
            return _databaseContext.MashReferral_2
                .Where(x => x.Id == requestId)
                .Include(x => x.MashResidents)
                .FirstOrDefault()
                ?.ToDomain();
        }

        public IEnumerable<MashReferral_2> GetReferralsUsingQuery(QueryMashReferrals request)
        {
            var results = _databaseContext.MashReferral_2.AsQueryable();

            if (!string.IsNullOrEmpty(request.Id))
            {
                results = results.Where(x => x.Id == long.Parse(request.Id));
            }

            return results
                .Include(x => x.MashResidents)
                .Select(m => m.ToDomain());
        }

        public MashReferral_2 UpdateReferral(UpdateMashReferral request, long referralId)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.WorkerEmail);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email \"{request.WorkerEmail}\" not found");
            }

            var referral = _databaseContext.MashReferral_2
                .FirstOrDefault(x => x.Id == referralId);

            if (referral == null)
            {
                throw new MashReferralNotFoundException($"MASH referral with id {referralId} not found");
            }

            if (request.UpdateType.Equals("CONTACT-DECISION"))
            {
                if (!referral.Stage.Equals("CONTACT"))
                {
                    throw new MashReferralStageMismatchException($"Referral {referral.Id} is in stage \"{referral.Stage}\", this request requires the referral to be in stage \"contact\"");
                }

                referral.ContactDecisionCreatedAt = _systemTime.Now;
                referral.ContactDecisionUrgentContactRequired = request.RequiresUrgentContact;
                referral.Stage = "INITIAL";
            }

            if (request.UpdateType.Equals("INITIAL-DECISION"))
            {
                if (!referral.Stage.Equals("INITIAL"))
                {
                    throw new MashReferralStageMismatchException($"Referral {referral.Id} is in stage \"{referral.Stage}\", this request requires the referral to be in stage \"initial\"");
                }

                referral.InitialDecisionCreatedAt = _systemTime.Now;
                referral.InitialDecision = request.Decision;
                referral.InitialDecisionUrgentContactRequired = request.RequiresUrgentContact;
                referral.InitialDecisionReferralCategory = request.ReferralCategory;
                referral.Stage = "SCREENING";
            }

            if (request.UpdateType.Equals("SCREENING-DECISION"))
            {
                if (!referral.Stage.Equals("SCREENING"))
                {
                    throw new MashReferralStageMismatchException($"Referral {referral.Id} is in stage \"{referral.Stage}\", this request requires the referral to be in stage \"screening\"");
                }

                referral.ScreeningCreatedAt = _systemTime.Now;
                referral.ScreeningDecision = request.Decision;
                referral.ScreeningUrgentContactRequired = request.RequiresUrgentContact;
                referral.Stage = "FINAL";
            }

            if (request.UpdateType.Equals("FINAL-DECISION"))
            {
                if (!referral.Stage.Equals("FINAL"))
                {
                    throw new MashReferralStageMismatchException($"Referral {referral.Id} is in stage \"{referral.Stage}\", this request requires the referral to be in stage \"final\"");
                }

                referral.FinalDecisionCreatedAt = _systemTime.Now;
                referral.FinalDecision = request.Decision;
                referral.FinalDecisionUrgentContactRequired = request.RequiresUrgentContact;
                referral.FinalDecisionReferralCategory = request.ReferralCategory;
                referral.Stage = "POST-FINAL";
            }

            _databaseContext.SaveChanges();

            return referral.ToDomain();
        }

    }
}
