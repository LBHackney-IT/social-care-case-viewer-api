// using System;
// using System.ComponentModel.DataAnnotations;

// namespace SocialCareCaseViewerApi.V1.Boundary.Requests
// {
//     public class CreateWarningNoteRequest
//     {
//         [Required]
//         [Range(1, long.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
//         public long PersonId { get; set; }
//         public DateTime? StartDate { get; set; }
//         public DateTime? EndDate { get; set; }
//         public Boolean IndividualNotified { get; set; }
//         public string NotificationDetails { get; set; }
//         public string ReviewDetails { get; set; }
//         public string NoteType { get; set; }
//         public string Status { get; set; }
//         public DateTime? DateInformed { get; set; }
//         public string HowInformed { get; set; }
//         public string WarningNarrative { get; set; }
//         public string ManagersName { get; set; }
//         public DateTime? DateManagerInformed { get; set; }
//     }
// }
