using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class WaitingList
    {
        public List<Person> Persons { get; set; }
    }
}
