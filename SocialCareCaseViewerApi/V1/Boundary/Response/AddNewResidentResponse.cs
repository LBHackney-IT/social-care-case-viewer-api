using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class AddNewResidentResponse
    {
        public long? PersonId { get; set; }
        public long? AddressId { get; set; }
        public List<int> OtherNameIds { get; set; }
        public List<int> PhoneNumberIds { get; set; }
        public string CaseNoteId { get; set; }
        public string CaseNoteErrorMessage { get; set; }
    }
}
