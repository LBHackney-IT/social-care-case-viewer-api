
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ResidentQueryParam
    {
        /// <example>
        /// Ciasom
        /// </example>
        /// Databind to first_name
        [FromQuery(Name = "first_name")]
        public string FirstName { get; set; }
        /// <example>
        /// Tessellate
        /// </example>
        /// Databind to last_name
        [FromQuery(Name = "last_name")]
        public string LastName { get; set; }

        /// <example>
        /// 01-01-2001
        /// </example>
        /// Databind to date_of_birth
        [FromQuery(Name = "date_of_birth")]
        public string DateOfBirth { get; set; }

        /// <example>
        /// 100000
        /// </example>
        /// Databind to person_id
        [FromQuery(Name = "person_id")]
        public string PersonId { get; set; }

        /// <example>
        /// a
        /// </example>
        /// Databind to context_flag
        [FromQuery(Name = "context_flag")]
        public string AgeGroup { get; set; }
    }
}
