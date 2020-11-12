namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class AscAllocation
    {
        public long PersonId { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string DateOfBirth { get; set; }

        public int Age { get; set; }

        public string PrimarySupportReason { get; set; }

        public string AllocatedTeam { get; set; }

        public string AllocatedWorker { get; set; }

        public string Address { get; set; }

        public string Postcode { get; set; }

        public long Uprn { get; set; }

        public string LongTermService { get; set; }

        public string SocialCareInvolvement { get; set; }

        public string ShortTermSupport { get; set; }

        public string HouseholdComposition { get; set; }

        public string Fullname { get; set; }
    }
}
