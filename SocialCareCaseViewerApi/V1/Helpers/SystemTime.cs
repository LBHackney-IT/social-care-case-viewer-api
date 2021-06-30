using System;

namespace SocialCareCaseViewerApi.V1.Helpers
{
    public class SystemTime : ISystemTime
    {
        public DateTime Now => DateTime.Now;
    }
}
