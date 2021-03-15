using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class GenericCaseNote : CaseNoteBase
    {
        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; }
    }
}
