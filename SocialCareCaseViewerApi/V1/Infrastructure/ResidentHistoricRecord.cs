#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class ResidentHistoricRecord {

        public long RecordId { get; set; }

        public string? FormName { get; set; }

        public long PersonId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? DateOfBirth { get; set; }

        public string? OfficerEmail { get; set; }

        public string? CaseFormUrl { get; set; }

        public string? CaseFormTimeStamp { get; set; }

        public string? DateOfEvent { get; set; }

        public string? CaseNoteTitle { get; set; }

        public RecordType RecordType { get; set; }

        public bool IsHistoric { get; set; } = true;
    }

public enum RecordType
{
    CaseNote,
    Visit
}
}
