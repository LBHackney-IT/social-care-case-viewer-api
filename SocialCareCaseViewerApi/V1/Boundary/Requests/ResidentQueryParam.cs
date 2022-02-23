using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ResidentQueryParam
    {
        [FromQuery(Name = "mosaic_id")]
        public string MosaicId { get; set; }

        /// <example>
        /// Ciasom
        /// </example>
        /// Databind to first_name
        [FromQuery(Name = "first_name")]
        public string FirstName { get; set; }


        /// <example>
        /// Ciasom
        /// </example>
        /// Databind to the team
        [FromQuery(Name = "team_id")]
        public string TeamId { get; set; }

        /// <example>
        /// Tessellate
        /// </example>
        /// Databind to last_name
        [FromQuery(Name = "last_name")]
        public string LastName { get; set; }
        [FromQuery(Name = "date_of_birth")]
        public string DateOfBirth { get; set; }

        /// <example>
        /// 1 Montage street
        /// </example>
        /// Databind to address
        [FromQuery(Name = "address")]
        public string Address { get; set; }

        /// <example>
        /// E8 1DY
        /// </example>
        /// Databind to post_code
        [FromQuery(Name = "postcode")]
        public string Postcode { get; set; }

        [FromQuery(Name = "context_flag")]
        public string ContextFlag { get; set; }
    }
}
