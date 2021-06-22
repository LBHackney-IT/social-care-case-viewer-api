using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class PersonalRelationships
    {
        public List<RelatedPerson> Acquaintance { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> AuntUncle { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Child { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Cousin { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> ExPartner { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> ExSpouse { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> FosterCarer { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> FosterCarerSupportCarer { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> FosterChild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Friend { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Grandchild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Grandparent { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> GreatGrandchild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> GreatGrandparent { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> HalfSibling { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> InContactWith { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Neighbour { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> NieceNephew { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Other { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Parent { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> ParentOfUnbornChild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Partner { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> PrivateFosterCarer { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> PrivateFosterChild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Sibling { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> SiblingOfUnbornChild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> Spouse { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> StepChild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> StepParent { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> StepSibling { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> SupportCarerFosterCarer { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> UnbornChild { get; set; } = new List<RelatedPerson>();
        public List<RelatedPerson> UnbornSibling { get; set; } = new List<RelatedPerson>();
    }
}
