using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class AddressDomain
    {
        [Required]
        public string Address { get; set; }
        public long? Uprn { get; set; }
        public string Postcode { get; set; }
    }
}
