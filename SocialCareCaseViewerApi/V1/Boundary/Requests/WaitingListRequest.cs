using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class WaitingListRequest
    {
        [FromQuery(Name = "team_id")]
        public long TeamId { get; set; }
    }
}
