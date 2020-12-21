namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class Allocation
    {
        /// <example>
        /// abc123
        /// </example>
        public string PersonId { get; set; }

        /// <example>
        /// Ciasom
        /// </example>
        public string FirstName { get; set; }
        /// <example>
        /// Tessellate
        /// </example>
        public string LastName { get; set; }
        /// <example>
        /// 2020-05-15
        /// </example>
        public string DateOfBirth { get; set; }
        /// <example>
        /// F
        /// </example>
        public string Gender { get; set; }
        public long? GroupId { get; set; }
        public string Ethnicity { get; set; }
        public string SubEthnicity { get; set; }
        public string Religion { get; set; }
        public string ServiceUserGroup { get; set; }
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public string GpName { get; set; }
        public string GpAddress { get; set; }
        public string GpSurgery { get; set; }
        public string AllocatedWorker { get; set; }
        public string WorkerType { get; set; }
        public string AllocatedWorkerTeam { get; set; }
        public string TeamName { get; set; }
        public string AllocationStartDate { get; set; }
        public string AllocationEndDate { get; set; }
        public string LegalStatus { get; set; }
        public string Placement { get; set; }
        public string OnCpRegister { get; set; }
        public string ContactAddress { get; set; }
        public string CaseStatus { get; set; }
        public string CaseClosureDate { get; set; }
        public string WorkerEmail { get; set; }
        public string LAC { get; set; }
    }
}
