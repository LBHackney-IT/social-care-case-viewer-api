using SocialCareCaseViewerApi.V1.Domain;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListCaseNotesResponse
    {
        public List<CaseNote> CaseNotes { get; set; }
    }
}
