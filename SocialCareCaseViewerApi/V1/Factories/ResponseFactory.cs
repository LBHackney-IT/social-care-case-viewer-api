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
                Role = worker.Role ?? "",
                ContextFlag = worker.ContextFlag ?? "",
                CreatedBy = worker.CreatedBy ?? "",
                DateStart = worker.DateStart?.ToString("s") ?? "",
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

        public static ListRelationshipsV1Response ToResponse(List<Person> personRecords, RelationshipsV1 relationships, List<long> personIds, long personId)
        {
            ListRelationshipsV1Response response = new ListRelationshipsV1Response() { PersonId = personId };

            if (personIds.Count == 0 || relationships == null)
                return response;

            if (personRecords?.Count > 0)
            {
                if (relationships?.PersonalRelationships?.Children?.Count > 0)
                    response.PersonalRelationships.Children.AddRange(PersonsToRelatedPersonsList(personRecords, relationships.PersonalRelationships.Children));

                if (relationships?.PersonalRelationships?.Other?.Count > 0)
                    response.PersonalRelationships.Other.AddRange(PersonsToRelatedPersonsList(personRecords, relationships.PersonalRelationships.Other));

                if (relationships?.PersonalRelationships?.Parents?.Count > 0)
                    response.PersonalRelationships.Parents.AddRange(PersonsToRelatedPersonsList(personRecords, relationships.PersonalRelationships.Parents));

                if (relationships?.PersonalRelationships?.Siblings?.Count > 0)
                    response.PersonalRelationships.Siblings.AddRange(PersonsToRelatedPersonsList(personRecords, relationships.PersonalRelationships.Siblings));
            }

            return response;
        }

        public static CaseSubmissionResponse ToResponse(this CaseSubmission caseSubmission)
        {
            return new CaseSubmissionResponse
            {
                SubmissionId = caseSubmission.SubmissionId,
                FormId = caseSubmission.FormId,
                Residents = caseSubmission.Residents,
                Workers = caseSubmission.Workers.Select(w => w.ToResponse()).ToList(),
                CreatedAt = caseSubmission.CreatedAt,
                CreatedBy = caseSubmission.CreatedBy.ToResponse(),
                SubmittedAt = caseSubmission.SubmittedAt,
                SubmittedBy = caseSubmission.SubmittedBy?.ToResponse(),
                EditHistory = caseSubmission.EditHistory.Select(e => new EditHistory<WorkerResponse>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToResponse()
                }).ToList(),
                SubmissionState = caseSubmission.SubmissionState,
                FormAnswers = caseSubmission.FormAnswers
            };
        }

        public static List<RelatedPersonV1> PersonsToRelatedPersonsList(List<Person> personList, List<long> relationshipIds)
        {
            return personList
                .Where(p => relationshipIds.Contains(p.Id))
                .Select(x => new RelatedPersonV1()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName
                }
                ).ToList();
        }

        public static PersonalRelationships ToResponse(this List<PersonalRelationship> personalRelationships)
        {
            return new PersonalRelationships()
            {
                GreatGrandchild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "greatGrandchild"),
                GreatGrandparent = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "greatGrandparent"),
                Grandchild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "grandchild"),
                Grandparent = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "grandparent"),
                Parent = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "parent"),
                StepParent = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "stepParent"),
                AuntUncle = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "auntUncle"),
                Child = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "child"),
                StepChild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "stepChild"),
                UnbornChild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "unbornChild"),
                Partner = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "partner"),
                ExPartner = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "exPartner"),
                Sibling = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "sibling"),
                HalfSibling = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "halfSibling"),
                StepSibling = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "stepSibling"),
                UnbornSibling = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "unbornSibling"),
                Spouse = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "spouse"),
                Cousin = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "cousin"),
                NieceNephew = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "nieceNephew"),
                FosterCarer = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "fosterCarer"),
                Friend = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "friend"),
                Other = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "other"),
                ExSpouse = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "exSpouse"),
                ParentOfUnbornChild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "parentOfUnbornChild"),
                SiblingOfUnbornChild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "siblingOfUnbornChild"),
                FosterCarerSupportCarer = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "fosterCarerSupportCarer"),
                PrivateFosterCarer = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "privateFosterCarer"),
                PrivateFosterChild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "privateFosterChild"),
                FosterChild = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "fosterChild"),
                SupportCarerFosterCarer = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "supportCarerFosterCarer"),
                Neighbour = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "neighbour"),
                InContactWith = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "inContactWith"),
                Acquaintance = MapPersonalRelationshipsToRelatedPersonList(personalRelationships, "acquaintance")
            };
        }

        private static List<RelatedPerson> MapPersonalRelationshipsToRelatedPersonList(List<PersonalRelationship> personalRelationships, string type)
        {
            return personalRelationships
                .Where(pr => pr.Type.Description == type)
                .Select(rp => new RelatedPerson()
                {
                    Id = rp.OtherPerson.Id,
                    FirstName = rp.OtherPerson.FirstName,
                    LastName = rp.OtherPerson.LastName,
                    Gender = rp.OtherPerson.Gender
                }
                ).ToList();
        }
    }
}
