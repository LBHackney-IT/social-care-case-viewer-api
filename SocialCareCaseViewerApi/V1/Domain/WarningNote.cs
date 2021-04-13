using System;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class WarningNote
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Boolean IndividualNotified { get; set; }
        public string NotificationDetails { get; set; }
        public string ReviewDetails { get; set; }
        public string NoteType { get; set; }
        public string Status { get; set; }
        public DateTime? DateInformed { get; set; }
        public string HowInformed { get; set; }
        public string WarningNarrative { get; set; }
        public string ManagersName { get; set; }
        public DateTime? DateManagerInformed { get; set; }
    }
}
