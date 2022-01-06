using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.IdGenerators;
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
        private readonly ISystemTime _systemTime;

        private readonly DatabaseContext _databaseContext;

        public MashReferralGateway(ISystemTime systemTime, DatabaseContext databaseContext)
        {
            _systemTime = systemTime;
            _databaseContext = databaseContext;
        }

        public void Reset()
        {
            //THIS IS FOR TESTING/STAGING PURPOSES, REMEMBER TO OMIT FROM PROD RELEASE
            _databaseContext.Database.ExecuteSqlRaw("DELETE FROM DBO.REF_MASH_RESIDENTS;");
            _databaseContext.Database.ExecuteSqlRaw("DELETE FROM DBO.REF_MASH_REFERRALS;");
        }

        public MashReferral CreateReferral(CreateReferralRequest request)
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

            _databaseContext.MashReferrals.Add(referral);
            _databaseContext.SaveChanges();
            return referral.ToDomain();
        }

        public MashReferral? GetReferralUsingId(long requestId)
        {
            return _databaseContext.MashReferrals
                .Where(x => x.Id == requestId)
                .Include(x => x.MashResidents)
                .Include(x => x.Worker)
                .FirstOrDefault()
                ?.ToDomain();
        }

        public IEnumerable<MashReferral> GetReferralsUsingQuery(QueryMashReferrals request)
        {
            var results = _databaseContext.MashReferrals.AsQueryable();

            if (request.Id != null)
            {
                results = results.Where(x => x.Id == request.Id);
            }

            return results
                .Include(x => x.MashResidents)
                .Include(x => x.Worker)
                .Select(m => m.ToDomain());
        }

        public MashReferral UpdateReferral(UpdateMashReferral request, long referralId)
        {
            var referral = _databaseContext.MashReferrals
                .Include(x => x.MashResidents)
                .Include(x => x.Worker)
                .FirstOrDefault(x => x.Id == referralId);

            if (referral == null)
            {
                throw new MashReferralNotFoundException($"MASH referral with id {referralId} not found");
            }

            if (request.UpdateType.Equals("CONTACT-DECISION"))
            {
                if (!referral.Stage.Equals("CONTACT"))
                {
                    throw new MashReferralStageMismatchException(
                        $"Referral {referral.Id} is in stage '{referral.Stage}', this request requires the referral to be in stage 'CONTACT'");
                }

                referral.ContactDecisionCreatedAt = _systemTime.Now;
                referral.ContactDecisionUrgentContactRequired = request.RequiresUrgentContact;
                referral.Stage = "INITIAL";
                referral.WorkerId = request.WorkerId;
            }

            if (request.UpdateType.Equals("INITIAL-DECISION"))
            {
                if (!referral.Stage.Equals("INITIAL"))
                {
                    throw new MashReferralStageMismatchException(
                        $"Referral {referral.Id} is in stage '{referral.Stage}', this request requires the referral to be in stage 'INITIAL'");
                }

                referral.InitialDecisionCreatedAt = _systemTime.Now;
                referral.InitialDecision = request.Decision;
                referral.InitialDecisionUrgentContactRequired = request.RequiresUrgentContact;
                referral.InitialDecisionReferralCategory = request.ReferralCategory;
                referral.Stage = "SCREENING";
                referral.WorkerId = null;
            }

            if (request.UpdateType.Equals("SCREENING-DECISION"))
            {
                if (!referral.Stage.Equals("SCREENING"))
                {
                    throw new MashReferralStageMismatchException(
                        $"Referral {referral.Id} is in stage '{referral.Stage}', this request requires the referral to be in stage 'SCREENING'");
                }

                referral.ScreeningCreatedAt = _systemTime.Now;
                referral.ScreeningDecision = request.Decision;
                referral.ScreeningUrgentContactRequired = request.RequiresUrgentContact;
                referral.Stage = "FINAL";
                referral.WorkerId = null;
            }

            if (request.UpdateType.Equals("FINAL-DECISION"))
            {
                if (!referral.Stage.Equals("FINAL"))
                {
                    throw new MashReferralStageMismatchException(
                        $"Referral {referral.Id} is in stage '{referral.Stage}', this request requires the referral to be in stage 'FINAL'");
                }

                referral.FinalDecisionCreatedAt = _systemTime.Now;
                referral.FinalDecision = request.Decision;
                referral.FinalDecisionUrgentContactRequired = request.RequiresUrgentContact;
                referral.FinalDecisionReferralCategory = request.ReferralCategory;
                referral.Stage = "POST-FINAL";
                referral.WorkerId = null;
            }

            if (request.UpdateType.Equals("ASSIGN-WORKER"))
            {
                if (request.WorkerId != null)
                {
                    if (_databaseContext.Workers.Find(request.WorkerId) == null)
                    {
                        throw new WorkerNotFoundException($"Worker with id {request.WorkerId} not found");
                    }

                    referral.WorkerId = request.WorkerId;
                }
                else
                {
                    Worker worker = _databaseContext.Workers.FirstOrDefault(w => w.Email == request.WorkerEmail);
                    if (worker == null)
                    {
                        throw new WorkerNotFoundException($"Worker with email {request.WorkerEmail} not found");
                    }

                    referral.WorkerId = worker.Id;
                }
            }

            _databaseContext.SaveChanges();

            return referral.ToDomain();
        }
    }
}
