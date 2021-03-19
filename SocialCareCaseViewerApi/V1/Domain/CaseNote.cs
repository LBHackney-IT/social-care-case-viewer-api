using System;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseNote
    {
        public string MosaicId { get; set; }
        public string CaseNoteId { get; set; }
        public string CaseNoteTitle { get; set; }
        public string CaseNoteContent { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedByEmail { get; set; }
        public string CreatedByName { get; set; }
        public string NoteType { get; set; }
    }
}
