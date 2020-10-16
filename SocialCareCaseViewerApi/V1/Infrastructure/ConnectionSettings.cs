using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class ConnectionSettings
    {
        public string Database { get; set; }
        public string ConnectionString { get; set; }
        public string CollectionName { get; set; }
    }
}
