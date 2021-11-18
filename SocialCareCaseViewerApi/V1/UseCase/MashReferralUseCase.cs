using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class MashReferralUseCase : IMashReferralUseCase
    {
        private readonly ISystemTime _systemTime;
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMashReferralGateway _mashReferralGateway;

        public MashReferralUseCase(IMashReferralGateway mashReferralGateway, IDatabaseGateway databaseGateway, ISystemTime systemTime)
        {
            _systemTime = systemTime;
            _databaseGateway = databaseGateway;
            _mashReferralGateway = mashReferralGateway;
        }

        public Boundary.Response.MashReferral? GetMashReferralUsingId(string requestId)
        {
            return _mashReferralGateway
                .GetReferralUsingId(requestId)
                ?.ToResponse();
        }

        public IEnumerable<Boundary.Response.MashReferral> GetMashReferrals(QueryMashReferrals request)
        {
            var filter = GenerateFilter(request);

            return _mashReferralGateway
                .GetReferralsUsingFilter(filter)
                .Select(x => x.ToResponse());
        }

        public Boundary.Response.MashReferral UpdateMashReferral(UpdateMashReferral request, string referralId)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.WorkerEmail);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email \"{request.WorkerEmail}\" not found");
            }

            var referral = _mashReferralGateway.GetInfrastructureUsingId(referralId);

            if (referral == null)
            {
                throw new MashReferralNotFoundException($"MASH referral with id {referralId} not found");
            }

            if (request.UpdateType.Equals("INITIAL-DECISION"))
            {
                if (!referral.Stage.Equals("INITIAL"))
                {
                    throw new MashReferralStageMismatchException($"Referral {referral.Id} is in stage \"{referral.Stage}\", this request requires the referral to be in stage \"initial decision\"");
                }

                referral.InitialCreatedAt = _systemTime.Now;
                referral.InitialDecision = request.Decision;
                referral.InitialUrgentContactRequired = request.RequiresUrgentContact;
                referral.ReferralCategory = request.ReferralCategory;
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

            _mashReferralGateway.UpsertRecord(referral);

            return referral.ToDomain().ToResponse();
        }

        public void Reset()
        {
            _mashReferralGateway.Reset();

            var referral1 = new MashReferral
            {
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Sally Samuels" },
                Stage = "CONTACT",
                ReferralDocumentURI = "hardcoded-referral-1-URI"
            };
            var referral2 = new MashReferral
            {
                Referrer = "School",
                CreatedAt = DateTime.Now.AddHours(-5),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Bert Bertram", "Courtney Bushell", "Jake Bucks", "Frank Gallagher" },
                Stage = "CONTACT",
                ReferralDocumentURI = "hardcoded-referral-2-URI"
            };
            var referral3 = new MashReferral
            {
                Referrer = "Family",
                CreatedAt = DateTime.Now.AddHours(-10),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Bertie Stephens", "Katie Ryans", "Hannah Stephens", "Will Stephens" },
                Stage = "CONTACT",
                ReferralDocumentURI = "hardcoded-referral-3-URI"
            };
            var referral4 = new MashReferral
            {
                Referrer = "Individual",
                CreatedAt = DateTime.Now.AddHours(-9),
                RequestedSupport = "Early help",
                Clients = new List<string> { "Elysia Hughs" },
                Stage = "CONTACT",
                ReferralDocumentURI = "hardcoded-referral-4-URI"
            };
            var referral5 = new MashReferral
            {
                Referrer = "School",
                CreatedAt = DateTime.Now.AddHours(-2),
                RequestedSupport = "Early help",
                Clients = new List<string> { "Benj Stephens" },
                Stage = "CONTACT",
                ReferralDocumentURI = "hardcoded-referral-5-URI"
            };
            var referral6 = new MashReferral
            {
                Referrer = "Police - green",
                CreatedAt = DateTime.Now.AddHours(-2),
                RequestedSupport = "Early help",
                Clients = new List<string> { "Sophie Smith" },
                Stage = "CONTACT",
                ReferralDocumentURI = "hardcoded-referral-6-URI"
            };
            var referral7 = new MashReferral
            {
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Sally Stephens" },
                Stage = "INITIAL",
                ReferralDocumentURI = "hardcoded-referral-7-URI"
            };
            var referral8 = new MashReferral
            {
                Referrer = "Police - green",
                CreatedAt = DateTime.Now.AddHours(-1),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Barry Smith", "Bert Smith", "Sally Smith" },
                Stage = "INITIAL",
                ReferralDocumentURI = "hardcoded-referral-8-URI"
            };
            var referral9 = new MashReferral
            {
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Sophie Owens" },
                Stage = "SCREENING",
                ReferralDocumentURI = "hardcoded-referral-9-URI",
                InitialDecision = "CSC Screening required in MASH",
                ReferralCategory = "Emotional abuse"
            };
            var referral10 = new MashReferral
            {
                Referrer = "Police - green",
                CreatedAt = DateTime.Now.AddHours(-1),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Max Smith", "Georgie Smith", "Hugh Smith" },
                Stage = "SCREENING",
                ReferralDocumentURI = "hardcoded-referral-10-URI",
                InitialDecision = "EH Screening required in MASH",
                ReferralCategory = "Emotional abuse"
            };
            var referral11 = new MashReferral
            {
                Referrer = "Police - red",
                CreatedAt = DateTime.Now.AddHours(-3),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "John Smith" },
                Stage = "FINAL",
                ReferralDocumentURI = "hardcoded-referral-11-URI",
                InitialDecision = "CSC Screening required in MASH",
                ReferralCategory = "Emotional abuse",
                ScreeningDecision = "DAIS",
                ScreeningCreatedAt = DateTime.Now.AddHours(-2),
                ScreeningUrgentContactRequired = true
            };
            var referral12 = new MashReferral
            {
                Referrer = "Police - green",
                CreatedAt = DateTime.Now.AddHours(-1),
                RequestedSupport = "Safeguarding",
                Clients = new List<string> { "Jack Owens", "Stuart Owens", "Katie Owens" },
                Stage = "FINAL",
                ReferralDocumentURI = "hardcoded-referral-12-URI",
                InitialDecision = "CSC Screening required in MASH",
                ReferralCategory = "Emotional abuse",
                ScreeningDecision = "DAIS",
                ScreeningCreatedAt = DateTime.Now.AddHours(-1),
                ScreeningUrgentContactRequired = true
            };

            _mashReferralGateway.InsertDocument(referral1);
            _mashReferralGateway.InsertDocument(referral2);
            _mashReferralGateway.InsertDocument(referral3);
            _mashReferralGateway.InsertDocument(referral4);
            _mashReferralGateway.InsertDocument(referral5);
            _mashReferralGateway.InsertDocument(referral6);
            _mashReferralGateway.InsertDocument(referral7);
            _mashReferralGateway.InsertDocument(referral8);
            _mashReferralGateway.InsertDocument(referral9);
            _mashReferralGateway.InsertDocument(referral10);
            _mashReferralGateway.InsertDocument(referral11);
            _mashReferralGateway.InsertDocument(referral12);
        }

        public static FilterDefinition<MashReferral> GenerateFilter(QueryMashReferrals request)
        {
            var builder = Builders<MashReferral>.Filter;
            var filter = builder.Empty;

            if (request.Id != null)
            {
                filter &= Builders<MashReferral>.Filter.Eq(x => x.Id, ObjectId.Parse(request.Id));
            }

            return filter;
        }
    }
}
