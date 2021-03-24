using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ListWarningNotesResponse
    {
        public List<WarningNote> WarningNotes { get; set; }
    }
}