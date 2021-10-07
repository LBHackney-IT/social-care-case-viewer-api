using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseStatusResponse
    {
        //public long Id { get; set; }
        //public string Type { get; set; }
        //public string StartDate { get; set; }
        //public string? EndDate { get; set; }
        //public string? Notes { get; set; }
        //public List<CaseStatusAnswer> Answers { get; set; }

        public int PersonId { get; set; }
        public List<CaseStatusBob> CaseStatuses { get; set; }

        public class CaseStatusBob
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public List<CaseStatusResponseField> Fields { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Notes { get; set; }
        }

        public class CaseStatusResponseField
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public SelectedOption SelectedOption { get; set; }
        }

        public class SelectedOption
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}




