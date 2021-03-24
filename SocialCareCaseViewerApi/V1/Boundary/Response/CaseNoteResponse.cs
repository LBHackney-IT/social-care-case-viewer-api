namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseNoteResponse
    {
        public string RecordId { get; set; }
        public string PersonId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string DateOfEvent { get; set; }
        public string OfficerEmail { get; set; }
        public string OfficerName { get; set; }
        public string FormName { get; set; }
    }
}
