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

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MashReferralGateway : IMashReferralGateway
    {
        private readonly IMongoGateway _mongoGateway;
        private readonly ISystemTime _systemTime;

        private readonly DatabaseContext _databaseContext;

        public MashReferralGateway(IMongoGateway mongoGateway, ISystemTime systemTime, DatabaseContext databaseContext)
        {
            _mongoGateway = mongoGateway;
            _systemTime = systemTime;
            _databaseContext = databaseContext;
        }

        public void Reset2()
        {
            //THIS IS FOR TESTING/STAGING PURPOSES, REMEMBER TO OMIT FROM PROD RELEASE
            _databaseContext.Database.ExecuteSqlRaw("DELETE FROM DBO.REF_MASH_RESIDENTS;");
            _databaseContext.Database.ExecuteSqlRaw("DELETE FROM DBO.REF_MASH_REFERRALS;");

        }

        public void CreateReferral(CreateReferralRequest request)
        {
            var referral = new Infrastructure.MashReferral
            {
                Referrer = request.Referrer,
                RequestedSupport = request.RequestedSupport,
                ReferralDocumentURI = request.ReferralUri,
                Stage = "CONTACT",
                ReferralCreatedAt = _systemTime.Now,
                MashResidents = new List<MashResident>(),
                CreatedBy = request.Referrer,
                LastModifiedBy = request.Referrer
            };

            foreach (var mashResident in request.MashResidents)
            {
                var resident = new MashResident
                {
                    FirstName = mashResident.FirstName,
                    LastName = mashResident.LastName,
                    Address = mashResident.Address,
                    Ethnicity = mashResident.Ethnicity,
                    Gender = mashResident.Gender,
                    Postcode = mashResident.Postcode,
                    School = mashResident.School,
                    FirstLanguage = mashResident.FirstLanguage,
                    DateOfBirth = mashResident.DateOfBirth,
                    MashReferralId = referral.Id
                };
                referral.MashResidents.Add(resident);
            }

            _databaseContext.MashReferral_2.Add(referral);
            _databaseContext.SaveChanges();
        }

        public MashReferral? GetReferralUsingId_2(long requestId)
        {
            return _databaseContext.MashReferral_2
                .Where(x => x.Id == requestId)
                .Include(x => x.MashResidents)
                .FirstOrDefault()
                ?.ToDomain();
        }

        public IEnumerable<MashReferral> GetReferralsUsingQuery(QueryMashReferrals request)
        {
            var results = _databaseContext.MashReferral_2.AsQueryable();

            if (request.Id != null)
            {
                results = results.Where(x => x.Id == request.Id);
            }

            return results
                .Include(x => x.MashResidents)
                .Select(m => m.ToDomain());
        }

        public MashReferral UpdateReferral(UpdateMashReferral request, long referralId)
        {
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
