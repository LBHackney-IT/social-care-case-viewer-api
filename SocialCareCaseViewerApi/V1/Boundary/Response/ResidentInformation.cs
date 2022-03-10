using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ResidentInformation
    {
        /// <example>
        /// abc123
        /// </example>
        public string MosaicId { get; set; }
        /// <example>
        /// Ciasom
        /// </example>
        public string FirstName { get; set; }
        /// <example>
        /// Tessellate
        /// </example>
        public string LastName { get; set; }
        /// <example>
        /// 1000000000
        /// </example>
        public string Uprn { get; set; }
        /// <example>
        /// 2020-05-15
        /// </example>
        public string DateOfBirth { get; set; }
        public string AgeContext { get; set; }

        /// <example>
        /// Female
        /// </example>
        public string Gender { get; set; }
        /// <example>
        /// British
        /// </example>
        public string Nationality { get; set; }
        public List<ResidentTeamResponse>? ResidentTeams { get; set; }
        public List<Phone> PhoneNumber { get; set; }
        public List<Address> AddressList { get; set; }
        /// <example>
        /// 450 557 7104
        /// </example>
        public string NhsNumber { get; set; }

        public string Restricted { get; set; }
    }
}
