namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseNote
    {
        public string MosaicId { get; set; }
        public int CaseNoteId { get; set; }
        public string CaseNoteTitle { get; set; }
        public string CaseNoteContent { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedByEmail { get; set; }
        public string CreatedByName { get; set; }
        public string NoteType { get; set; }
    }
}
