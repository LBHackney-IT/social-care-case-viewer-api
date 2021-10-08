using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseSubmission = SocialCareCaseViewerApi.V1.Domain.CaseSubmission;
using dbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;
using AddressResponse = SocialCareCaseViewerApi.V1.Boundary.Response.Address;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;
using ResidentInformationResponse = SocialCareCaseViewerApi.V1.Boundary.Response.ResidentInformation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static AddNewResidentResponse ToResponse(this Person resident, dbAddress address, List<PersonOtherName> names, List<dbPhoneNumber> phoneNumbers, string caseNoteId, string caseNoteErrorMessage)
        {
            return new AddNewResidentResponse
            {
                Id = resident.Id,
                AddressId = address?.AddressId,
                OtherNameIds = names?.Count > 0 ? names.Select(x => x.Id).ToList() : null,
                PhoneNumberIds = phoneNumbers?.Count > 0 ? phoneNumbers.Select(x => x.Id).ToList() : null,
                CaseNoteId = caseNoteId,
                CaseNoteErrorMessage = caseNoteErrorMessage
            };
        }

        public static List<CareCaseData> ToResponse(this IEnumerable<BsonDocument> submittedFormsData)
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

        public static BsonDocument HistoricalCaseNotesToDomain(CaseNote note)
        {
            var useNoteTypeForFormName = Environment.GetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE") is ("true");

            return new BsonDocument(
                new List<BsonElement>
                {
                    new BsonElement("_id", note.CaseNoteId),
                    new BsonElement("mosaic_id", note.MosaicId),
                    new BsonElement("worker_email", note.CreatedByEmail ?? ""),
                    new BsonElement("form_name_overall", "Historical_Case_Note"),
                    new BsonElement("form_name", useNoteTypeForFormName ? FormatFormNameForHistoricCaseNote(note.NoteType) : note.CaseNoteTitle),
                    new BsonElement("title", note.CaseNoteTitle),
                    new BsonElement("timestamp", note.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")), //format used in imported data so have to match for now
                    new BsonElement("is_historical", true) //flag for front end
                }
            );
        }

        public static BsonDocument HistoricalVisitsToDomain(Visit visit)
        {
            var formTimeStamp = visit.ActualDateTime?.ToString("dd/MM/yyyy H:mm:ss") ??
                                visit.PlannedDateTime?.ToString("dd/MM/yyyy H:mm:ss") ?? ""; //format used in imported data from mongo so have to match for now

            return new BsonDocument(new List<BsonElement>
            {
                new BsonElement("_id", visit.VisitId.ToString()),
                new BsonElement("mosaic_id", visit.PersonId.ToString()),
                new BsonElement("worker_email", visit.CreatedByEmail ?? ""),
                new BsonElement("form_name_overall", "Historical_Visit"),
                new BsonElement("form_name", $"Historical Visit - {visit.VisitType ?? ""}"),
                new BsonElement("timestamp", formTimeStamp),
                new BsonElement("is_historical", true)
            });
        }

        public static CaseNoteResponse ToResponse(CaseNote historicalCaseNote)
        {
            return new CaseNoteResponse
            {
                RecordId = historicalCaseNote.CaseNoteId,
                PersonId = historicalCaseNote.MosaicId,
                Title = historicalCaseNote.CaseNoteTitle,
                Content = historicalCaseNote.CaseNoteContent,
                DateOfEvent = historicalCaseNote.CreatedOn.ToString("s"),
                OfficerName = historicalCaseNote.CreatedByName,
                OfficerEmail = historicalCaseNote.CreatedByEmail,
                FormName = FormatFormNameForHistoricCaseNote(historicalCaseNote.NoteType)
            };
        }

        public static WorkerResponse ToResponse(this Domain.Worker worker)
        {
            return new WorkerResponse
            {
                Id = worker.Id,
                Email = worker.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Role = worker.Role,
                ContextFlag = worker.ContextFlag,
                CreatedBy = worker.CreatedBy,
                DateStart = worker.DateStart?.ToString("s"),
                AllocationCount = worker.AllocationCount,
                Teams = worker.Teams
            };
        }

        private static string FormatFormNameForHistoricCaseNote(string noteType)
        {
            string pattern = @"\([^()]*\)$"; // Match brackets at the end e.g. (ASC)
            var formName = String.IsNullOrEmpty(noteType) ? "Case note" : noteType;
            var formattedFormName = Regex.Replace(formName, pattern, "").TrimEnd();

            return formattedFormName;
        }

        public static GetPersonResponse ToResponse(Person person)
        {
            //get the current display address
            var displayAddress = person.Addresses?.FirstOrDefault(x => x.IsDisplayAddress?.ToUpper() == "Y");

            return new GetPersonResponse()
            {
                SexualOrientation = person.SexualOrientation,
                DateOfBirth = person.DateOfBirth,
                DateOfDeath = person.DateOfDeath,
                ContextFlag = person.AgeContext,
                CreatedBy = person.CreatedBy,
                EmailAddress = person.EmailAddress,
                Ethnicity = person.Ethnicity,
                FirstLanguage = person.FirstLanguage,
                FirstName = person.FirstName,
                Gender = person.Gender,
                LastName = person.LastName,
                NhsNumber = person.NhsNumber,
                Id = person.Id,
                PreferredMethodOfContact = person.PreferredMethodOfContact,
                Religion = person.Religion,
                Restricted = person.Restricted,
                Title = person.Title,
                Address = displayAddress != null ? EntityFactory.DbAddressToAddressDomain(displayAddress) : null,
                OtherNames = person.OtherNames?.Select(x => x.ToDomain()).ToList(),
                PhoneNumbers = person.PhoneNumbers?.Select(x => x.ToDomain()).ToList()
            };
        }

        public static WarningNoteResponse ToResponse(this WarningNote warningNote)
        {
            return new WarningNoteResponse
            {
                Id = warningNote.Id,
                PersonId = warningNote.PersonId,
                StartDate = warningNote.StartDate?.ToString("s"),
                EndDate = warningNote.EndDate?.ToString("s"),
                DisclosedWithIndividual = warningNote.DisclosedWithIndividual,
                DisclosedDetails = warningNote.DisclosedDetails,
                Notes = warningNote.Notes,
                ReviewDate = warningNote.ReviewDate?.ToString("s"),
                NextReviewDate = warningNote.NextReviewDate?.ToString("s"),
                NoteType = warningNote.NoteType,
                Status = warningNote.Status,
                DisclosedDate = warningNote.DisclosedDate?.ToString("s"),
                DisclosedHow = warningNote.DisclosedHow,
                WarningNarrative = warningNote.WarningNarrative,
                ManagerName = warningNote.ManagerName,
                DiscussedWithManagerDate = warningNote.DiscussedWithManagerDate?.ToString("s"),
                CreatedBy = warningNote.CreatedBy,
                WarningNoteReviews = warningNote.WarningNoteReviews?.Select(x => x.ToResponse()).ToList()
            };
        }

        public static TeamResponse ToResponse(this Team team)
        {
            return new TeamResponse { Id = team.Id, Name = team.Name, Context = team.Context };
        }

        private static WarningNoteReviewResponse ToResponse(this WarningNoteReview review)
        {
            return new WarningNoteReviewResponse
            {
                Id = review.Id,
                WarningNoteId = review.WarningNoteId,
                ReviewDate = review.ReviewDate?.ToString("s"),
                DisclosedWithIndividual = review.DisclosedWithIndividual,
                ReviewNotes = review.ReviewNotes,
                ManagerName = review.ManagerName,
                DiscussedWithManagerDate = review.DiscussedWithManagerDate?.ToString("s"),
                CreatedAt = review.CreatedAt?.ToString("s"),
                CreatedBy = review.CreatedBy,
                LastModifiedAt = review.LastModifiedAt?.ToString("s"),
                LastModifiedBy = review.LastModifiedBy
            };
        }

        public static CaseSubmissionResponse ToResponse(this CaseSubmission caseSubmission)
        {
            return new CaseSubmissionResponse
            {
                SubmissionId = caseSubmission.SubmissionId,
                FormId = caseSubmission.FormId,
                Residents = caseSubmission.Residents,
                Workers = caseSubmission.Workers.Select(w => w.ToResponse()).ToList(),
                CreatedAt = caseSubmission.CreatedAt.ToString("O"),
                DateOfEvent = caseSubmission.DateOfEvent?.ToString("O"),
                CreatedBy = caseSubmission.CreatedBy.ToResponse(),
                SubmittedAt = caseSubmission.SubmittedAt?.ToString("O"),
                SubmittedBy = caseSubmission.SubmittedBy?.ToResponse(),
                ApprovedAt = caseSubmission.ApprovedAt?.ToString("O"),
                PanelApprovedAt = caseSubmission.PanelApprovedAt?.ToString("O"),
                PanelApprovedBy = caseSubmission.PanelApprovedBy?.ToResponse(),
                ApprovedBy = caseSubmission.ApprovedBy?.ToResponse(),
                RejectionReason = caseSubmission.RejectionReason,
                EditHistory = caseSubmission.EditHistory?.Select(e => new EditHistory<WorkerResponse>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToResponse()
                }).ToList(),
                SubmissionState = caseSubmission.SubmissionState,
                FormAnswers = caseSubmission.FormAnswers,
                Title = caseSubmission.Title,
                LastEdited = caseSubmission.LastEdited?.ToString("O"),
                CompletedSteps = caseSubmission.CompletedSteps
            };
        }

        public static List<Domain.PersonalRelationship> ToResponse(this List<Infrastructure.PersonalRelationship> personalRelationships)
        {
            return personalRelationships.GroupBy(
                personalRelationship => personalRelationship.Type,
                (type, relationships) => new Domain.PersonalRelationship()
                {
                    Type = type.Description,
                    Relationships = relationships.Select(relationship => new RelatedRelationship()
                    {
                        Id = relationship.Id,
                        PersonId = relationship.OtherPerson.Id,
                        FirstName = relationship.OtherPerson.FirstName,
                        LastName = relationship.OtherPerson.LastName,
                        Gender = relationship.OtherPerson.Gender,
                        IsMainCarer = relationship.IsMainCarer,
                        IsInformalCarer = relationship.IsInformalCarer,
                        Details = relationship.Details?.Details
                    }
                    ).ToList()
                }
            ).ToList();
        }

        public static ResidentInformationResponse ToResidentInformationResponse(this Person person)
        {
            return new ResidentInformationResponse
            {
                MosaicId = person.Id.ToString(),
                FirstName = person.FirstName,
                LastName = person.LastName,
                NhsNumber = person.NhsNumber?.ToString(),
                DateOfBirth = person.DateOfBirth?.ToString("O"),
                AgeContext = person.AgeContext,
                Nationality = person.Nationality,
                Gender = person.Gender,
                Restricted = person.Restricted,
                AddressList = person.Addresses?.Count > 0 ? person.Addresses.ToResponse() : null,
                PhoneNumber = person.PhoneNumbers?.Count > 0 ? person.PhoneNumbers.ToResponse() : null,
                Uprn = person.Addresses?.FirstOrDefault(a => a.EndDate == null)?.Uprn?.ToString() //use same logic as in legacy platform API for compatibility
            };
        }

        public static AddressResponse ToResponse(this dbAddress address)
        {
            return new AddressResponse()
            {
                AddressLine1 = address.AddressLines,
                DisplayAddressFlag = address.IsDisplayAddress,
                EndDate = address.EndDate,
                PostCode = address.PostCode
            };
        }

        public static List<AddressResponse> ToResponse(this List<dbAddress> addresses)
        {
            return addresses.Select(address => new AddressResponse
            {
                EndDate = address.EndDate,
                DisplayAddressFlag = address.IsDisplayAddress,
                AddressLine1 = address.AddressLines,
                PostCode = address.PostCode
            }).ToList();
        }

        public static Phone ToResponse(this dbPhoneNumber phoneNumber)
        {
            return new Phone()
            {
                PhoneNumber = phoneNumber.Number,
                PhoneType = phoneNumber.Type
            };
        }

        public static List<Phone> ToResponse(this List<dbPhoneNumber> phoneNumbers)
        {
            return phoneNumbers.Select(phoneNumber => new Phone
            {
                PhoneNumber = phoneNumber.Number,
                PhoneType = phoneNumber.Type
            }).ToList();
        }

        public static CaseStatusResponse ToResponse(this CaseStatus caseStatus)
        {
            return new CaseStatusResponse
            {
                Id = caseStatus.Id,
                Type = caseStatus.Type,
                StartDate = caseStatus.StartDate.ToString("O"),
                EndDate = caseStatus.EndDate?.ToString("O"),
                Notes = caseStatus.Notes,
                Fields = caseStatus.Fields
            };
        }
    }
}
