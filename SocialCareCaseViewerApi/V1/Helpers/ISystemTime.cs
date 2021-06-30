using System;

namespace SocialCareCaseViewerApi.V1.Helpers
{
    public interface ISystemTime
    {
        DateTime Now { get; }
    }
}
