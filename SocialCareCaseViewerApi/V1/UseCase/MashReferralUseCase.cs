using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class MashReferralUseCase : IMashReferralUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMashReferralGateway _mashReferralGateway;

        public MashReferralUseCase(IMashReferralGateway mashReferralGateway, IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
            _mashReferralGateway = mashReferralGateway;
        }

        public Boundary.Response.MashReferral? GetMashReferralUsingId(long requestId)
        {
            return _mashReferralGateway
                .GetReferralUsingId_2(requestId)
                ?.ToResponse();
        }


        public IEnumerable<Boundary.Response.MashReferral> GetMashReferrals(QueryMashReferrals request)
        {
            return _mashReferralGateway
                .GetReferralsUsingQuery(request)
                .Select(x => x.ToResponse());
        }

        public void CreateNewMashReferral(CreateReferralRequest request)
        {
            _mashReferralGateway.CreateReferral(request);
        }

        public Boundary.Response.MashReferral UpdateMashReferral(UpdateMashReferral request, long referralId)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.WorkerEmail);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email \"{request.WorkerEmail}\" not found");
            }

            return _mashReferralGateway.UpdateReferral(request, referralId).ToResponse();
        }

        public void Reset()
        {
            _mashReferralGateway.Reset2();

            var referral1 = new CreateReferralRequest
            {
                Referrer = "Police - red",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Sally", LastName = "Samuels" } },
                ReferralUri = "hardcoded-referral-1-URI"
            };
            var referral2 = new CreateReferralRequest
            {
                Referrer = "School",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Bert", LastName = "Bertram" }, new MashResidentRequest() { FirstName = "Courtney", LastName = "Bushell" }, new MashResidentRequest() { FirstName = "Jake", LastName = "Bucks" }, new MashResidentRequest() { FirstName = "Frank", LastName = "Gallagher" } },
                ReferralUri = "hardcoded-referral-2-URI"
            };
            var referral3 = new CreateReferralRequest
            {
                Referrer = "Family",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Bertie", LastName = "Stephens" }, new MashResidentRequest() { FirstName = "Katie", LastName = "Ryans" }, new MashResidentRequest() { FirstName = "Hannah", LastName = "Stephens" } },
                ReferralUri = "hardcoded-referral-3-URI"
            };
            var referral4 = new CreateReferralRequest
            {
                Referrer = "Individual",
                RequestedSupport = "Early help",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Elysia", LastName = "Hughs" } },
                ReferralUri = "hardcoded-referral-4-URI"
            };
            var referral5 = new CreateReferralRequest
            {
                Referrer = "School",
                RequestedSupport = "Early help",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Benji", LastName = "Stephens" } },
                ReferralUri = "hardcoded-referral-5-URI"
            };
            var referral6 = new CreateReferralRequest
            {
                Referrer = "Police - green",
                RequestedSupport = "Early help",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Sophie", LastName = "Smith" } },
                ReferralUri = "hardcoded-referral-6-URI"
            };
            var referral7 = new CreateReferralRequest
            {
                Referrer = "Police - red",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Sally", LastName = "Stephens" } },
                ReferralUri = "hardcoded-referral-7-URI"
            };
            var referral8 = new CreateReferralRequest
            {
                Referrer = "Police - green",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Barry", LastName = "Smith" }, new MashResidentRequest() { FirstName = "Bert", LastName = "Smith" }, new MashResidentRequest() { FirstName = "Sally", LastName = "Smith" } },
                ReferralUri = "hardcoded-referral-8-URI"
            };
            var referral9 = new CreateReferralRequest
            {
                Referrer = "Police - red",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Sophie", LastName = "Owens" } },
                ReferralUri = "Emotional abuse"
            };
            var referral10 = new CreateReferralRequest
            {
                Referrer = "Police - green",
                RequestedSupport = "Safeguarding",
                MashResidents = new List<MashResidentRequest>() { new MashResidentRequest() { FirstName = "Max", LastName = "Smith" }, new MashResidentRequest() { FirstName = "Georgie", LastName = "Smith" }, new MashResidentRequest() { FirstName = "Hugh", LastName = "Smith" } },
                ReferralUri = "hardcoded-referral-10-URI"
            };

            _mashReferralGateway.CreateReferral(referral1);
            _mashReferralGateway.CreateReferral(referral2);
            _mashReferralGateway.CreateReferral(referral3);
            _mashReferralGateway.CreateReferral(referral4);
            _mashReferralGateway.CreateReferral(referral5);
            _mashReferralGateway.CreateReferral(referral6);
            _mashReferralGateway.CreateReferral(referral7);
            _mashReferralGateway.CreateReferral(referral8);
            _mashReferralGateway.CreateReferral(referral9);
            _mashReferralGateway.CreateReferral(referral10);

        }
    }
}
