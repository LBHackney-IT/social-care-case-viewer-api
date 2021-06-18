using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class PersonalRelationshipsV1<T>
    {
        public List<T> Parents { get; set; } = new List<T>();

        public List<T> Siblings { get; set; } = new List<T>();

        public List<T> Children { get; set; } = new List<T>();

        public List<T> Other { get; set; } = new List<T>();
    }
}
