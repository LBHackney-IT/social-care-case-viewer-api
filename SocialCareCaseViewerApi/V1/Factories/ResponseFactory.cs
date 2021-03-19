using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using dbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;

namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static AddNewResidentResponse ToResponse(this Person resident, dbAddress address, List<PersonOtherName> names, List<dbPhoneNumber> phoneNumbers, string caseNoteId, string caseNoteErrorMessage)
        {
            return new AddNewResidentResponse
            {
                PersonId = resident.Id,
                AddressId = address?.AddressId,
                OtherNameIds = names?.Count > 0 ? names.Select(x => x.Id).ToList() : null,
                PhoneNumberIds = phoneNumbers?.Count > 0 ? phoneNumbers.Select(x => x.Id).ToList() : null,
                CaseNoteId = caseNoteId,
                CaseNoteErrorMessage = caseNoteErrorMessage
            };
        }

        public static List<CareCaseData> ToResponse(List<BsonDocument> submittedFormsData)
        {
            return submittedFormsData.Select(x => x.ToResponse()).ToList();
        }

        private static CareCaseData ToResponse(this BsonDocument formData)
        {
            var caseData = BsonSerializer.Deserialize<FormData>(formData);
            return caseData.ToResponse(formData);
        }
        private static CareCaseData ToResponse(this FormData formData, BsonDocument rawData)
        {
            var dummyFormData = formData;
            return new CareCaseData
            {
                RecordId = formData.RecordId,
                FirstName = formData.FirstName,
                LastName = formData.LastName,
                DateOfBirth = formData.DateOfBirth,
                OfficerEmail = formData.OfficerEmail,
                FormName = formData.FormName,
                CaseFormUrl = formData.CaseFormUrl,
                CaseFormTimestamp = formData.CaseFormTimestamp,
                DateOfEvent = formData.DateOfEvent,
                PersonId = formData.PersonId,
                CaseFormData = rawData.ToJson()
            };
        }

        public static List<BsonDocument> HistoricalCaseNotesToDomain(List<CaseNote> caseNotes)
        {
            List<BsonDocument> historicalCaseNotes = new List<BsonDocument>();

            foreach (var note in caseNotes)
            {
                historicalCaseNotes.Add(new BsonDocument(
                        new List<BsonElement>
                        {
                                new BsonElement("_id", note.CaseNoteId ?? ""),
                                new BsonElement("worker_email", note.CreatedByEmail ?? ""),
                                new BsonElement("form_name_overall", "Historical_Case_Note"),
                                new BsonElement("form_name", note.CaseNoteTitle ?? ""),
                                new BsonElement("timestamp", note.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")), //format used in imported data so have to match for now
                                new BsonElement("is_historical", true) //flag for front end
                        }
                        ));
            }

            return historicalCaseNotes;
        }

        public static List<BsonDocument> HistoricalVisitsToDomain(List<Visit> visits)
        {
            List<BsonDocument> historicalVisits = new List<BsonDocument>();

            foreach (var visit in visits)
            {
                historicalVisits.Add(new BsonDocument(
                        new List<BsonElement>
                        {
                                new BsonElement("_id", visit.Id ?? ""),
                                new BsonElement("worker_email", visit.CreatedByEmail),
                                new BsonElement("form_name_overall", "Historical_Visit"),
                                new BsonElement("form_name", visit.Title),
                                new BsonElement("timestamp", visit.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")), //format used in imported data so have to match for now
                                new BsonElement("is_historical", true) //flag for front end
                        }
                        ));
            }

            return historicalVisits;
        }
    }
}
