using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListCaseNotesResponse
    {
        public List<CaseNote> CaseNotes { get; set; }
    }
}
