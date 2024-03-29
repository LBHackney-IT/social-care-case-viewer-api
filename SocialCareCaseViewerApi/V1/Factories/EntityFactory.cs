using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Domain.Address;
using CaseSubmission = SocialCareCaseViewerApi.V1.Infrastructure.CaseSubmission;
using DbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using dbEmailAddress = SocialCareCaseViewerApi.V1.Infrastructure.EmailAddress;
using dbKeyContact = SocialCareCaseViewerApi.V1.Infrastructure.KeyContact;
using dbGpDetails = SocialCareCaseViewerApi.V1.Infrastructure.GpDetails;
using dbLastUpdated = SocialCareCaseViewerApi.V1.Infrastructure.LastUpdated;
using dbTechUse = SocialCareCaseViewerApi.V1.Infrastructure.TechUse;
using dbDisability = SocialCareCaseViewerApi.V1.Infrastructure.Disability;
using dbEmail = SocialCareCaseViewerApi.V1.Infrastructure.EmailAddress;
using DbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using dbWarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using DbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using KeyContact = SocialCareCaseViewerApi.V1.Domain.KeyContact;
using GpDetails = SocialCareCaseViewerApi.V1.Domain.GpDetailsDomain;
using LastUpdated = SocialCareCaseViewerApi.V1.Domain.LastUpdatedDomain;
using TechUse = SocialCareCaseViewerApi.V1.Domain.TechUse;
using Disability = SocialCareCaseViewerApi.V1.Domain.Disability;
using EmailAddress = SocialCareCaseViewerApi.V1.Domain.EmailAddress;
using PersonOtherEmails = SocialCareCaseViewerApi.V1.Domain.EmailAddress;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class EntityFactory
    {
        public static Address ToDomain(this DbAddress address)
        {
            return new Address
            {
                AddressLine1 = address.AddressLines,
                PostCode = address.PostCode
            };
        }

        public static LastUpdatedDomain DbLastUpdatedToDomain(dbLastUpdated lastUpdated)
        {
            return new LastUpdatedDomain()
            {
                Type = lastUpdated.Type,
                UpdatedAt = lastUpdated.UpdatedAt
            };
        }

        public static GpDetailsDomain DbGpDetailsToDomain(dbGpDetails details)
        {
            return new GpDetailsDomain()
            {
                Name = details.Name,
                Address = details.Address,
                Postcode = details.Postcode,
                PhoneNumber = details.PhoneNumber,
                Email = details.Email
            };
        }

        public static AddressDomain DbAddressToAddressDomain(DbAddress address)
        {
            return new AddressDomain()
            {
                Address = address.AddressLines,
                Postcode = address.PostCode,
                Uprn = address.Uprn
            };
        }

        public static Worker ToDomain(this DbWorker worker, bool includeTeamData)
        {
            return new Worker
            {
                Id = worker.Id,
                Email = worker.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Role = worker.Role,
                ContextFlag = worker.ContextFlag,
                CreatedBy = worker.CreatedBy,
                DateStart = worker.DateStart,
                AllocationCount = worker.Allocations?.Count(x => x.CaseStatus.ToUpper() == "OPEN") ?? 0,
                Teams = (includeTeamData ? worker.WorkerTeams?.Select(x => new Team() { Id = x.Team.Id, Name = x.Team.Name, Context = x.Team.Context }).ToList() : null) ?? new List<Team>()
            };
        }

        public static Team ToDomain(this DbTeam team)
        {
            return new Team
            {
                Id = team.Id,
                Name = team.Name,
                Context = team.Context
            };
        }

        public static WarningNote ToDomain(this dbWarningNote dbWarningNote, List<WarningNoteReview>? reviews = null)
        {
            return new WarningNote
            {
                Id = dbWarningNote.Id,
                PersonId = dbWarningNote.PersonId,
                StartDate = dbWarningNote.StartDate,
                EndDate = dbWarningNote.EndDate,
                DisclosedWithIndividual = dbWarningNote.DisclosedWithIndividual,
                DisclosedDetails = dbWarningNote.DisclosedDetails,
                Notes = dbWarningNote.Notes,
                ReviewDate = dbWarningNote.ReviewDate,
                NextReviewDate = dbWarningNote.NextReviewDate,
                NoteType = dbWarningNote.NoteType,
                Status = dbWarningNote.Status,
                DisclosedDate = dbWarningNote.DisclosedDate,
                DisclosedHow = dbWarningNote.DisclosedHow,
                WarningNarrative = dbWarningNote.WarningNarrative,
                ManagerName = dbWarningNote.ManagerName,
                DiscussedWithManagerDate = dbWarningNote.DiscussedWithManagerDate,
                CreatedBy = dbWarningNote.CreatedBy,
                WarningNoteReviews = reviews ?? new List<WarningNoteReview>()
            };
        }

        public static PhoneNumber ToDomain(this dbPhoneNumber phoneNumber)
        {
            return new PhoneNumber()
            {
                Number = phoneNumber.Number,
                Type = phoneNumber.Type
            };
        }

        public static KeyContact ToDomain(this dbKeyContact keyContact)
        {
            return new KeyContact()
            {
                Name = keyContact.Name,
                Email = keyContact.Email
            };
        }

        public static GpDetails ToDomain(this dbGpDetails gpDetails)
        {
            return new GpDetails()
            {
                Name = gpDetails.Name,
                Address = gpDetails.Address,
                Postcode = gpDetails.Postcode,
                PhoneNumber = gpDetails.PhoneNumber,
                Email = gpDetails.Email
            };
        }

        public static TechUse ToDomain(this dbTechUse techUse)
        {
            return new TechUse()
            {
                TechType = techUse.TechType
            };
        }

        public static Disability ToDomain(this dbDisability disability)
        {
            return new Disability()
            {
                DisabilityType = disability.DisabilityType
            };
        }

        public static EmailAddress ToDomain(this dbEmailAddress email)
        {
            return new EmailAddress()
            {
                Email = email.Email,
                Type = email.Type
            };
        }

        public static OtherName ToDomain(this PersonOtherName otherName)
        {
            return new OtherName()
            {
                FirstName = otherName.FirstName,
                LastName = otherName.LastName
            };
        }

        public static LastUpdated ToDomain(this dbLastUpdated update)
        {
            return new LastUpdated()
            {
                Type = update.Type,
                UpdatedAt = update.UpdatedAt
            };
        }

        public static Domain.CaseSubmission ToDomain(this CaseSubmission caseSubmission,
            bool includeFormAnswers = true, bool includeEditHistory = true, bool pruneUnfinished = false, bool includeDeletionDetails = true)
        {
            var mapSubmissionStateToString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "In progress" },
                { SubmissionState.Submitted, "Submitted" },
                { SubmissionState.Approved, "Approved" },
                { SubmissionState.Discarded, "Discarded" },
                { SubmissionState.PanelApproved, "Panel Approved" }
            };

            if (pruneUnfinished)
            {
                return new Domain.CaseSubmission
                {
                    SubmissionId = caseSubmission.SubmissionId.ToString(),
                    FormId = caseSubmission.FormId,
                    CreatedBy = new Worker { Email = caseSubmission.CreatedBy.Email, },
                    CreatedAt = caseSubmission.CreatedAt,
                    Residents = caseSubmission.Residents.Select(r => new Person
                    {
                        Id = r.Id,
                        AgeContext = r.AgeContext,
                        FirstName = r.FirstName,
                        LastName = r.LastName,
                        Restricted = r.Restricted
                    }).ToList(),
                    Workers = caseSubmission.Workers.Select(w => new Worker
                    {
                        Email = w.Email
                    }).ToList(),
                    SubmissionState = mapSubmissionStateToString[caseSubmission.SubmissionState],
                    LastEdited = caseSubmission.EditHistory.Last().EditTime,
                    CompletedSteps = caseSubmission.FormAnswers.Count,
                    Title = caseSubmission.Title,
                    Deleted = caseSubmission.Deleted ?? false,
                    PinnedAt = caseSubmission.PinnedAt,
                    DeletionDetails = includeDeletionDetails ? caseSubmission.DeletionDetails : null
                };
            }

            return new Domain.CaseSubmission
            {
                SubmissionId = caseSubmission.SubmissionId.ToString(),
                FormId = caseSubmission.FormId,
                Residents = caseSubmission.Residents,
                Workers = caseSubmission.Workers.Select(w => w.ToDomain(false)).ToList(),
                CreatedAt = caseSubmission.CreatedAt,
                DateOfEvent = caseSubmission.DateOfEvent,
                CreatedBy = caseSubmission.CreatedBy.ToDomain(false),
                SubmittedAt = caseSubmission.SubmittedAt,
                SubmittedBy = caseSubmission.SubmittedBy?.ToDomain(false),
                ApprovedAt = caseSubmission.ApprovedAt,
                ApprovedBy = caseSubmission.ApprovedBy?.ToDomain(false),
                PanelApprovedAt = caseSubmission.PanelApprovedAt,
                PanelApprovedBy = caseSubmission.PanelApprovedBy?.ToDomain(false),
                RejectionReason = caseSubmission.RejectionReason,
                EditHistory = includeEditHistory ? caseSubmission.EditHistory.Select(e => new EditHistory<Worker>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToDomain(false)
                }).ToList() : null,
                SubmissionState = mapSubmissionStateToString[caseSubmission.SubmissionState],
                FormAnswers = includeFormAnswers ? caseSubmission.FormAnswers : null,
                Title = caseSubmission.Title,
                LastEdited = caseSubmission.EditHistory.Last().EditTime,
                CompletedSteps = caseSubmission.FormAnswers.Count,
                Deleted = caseSubmission.Deleted ?? false,
                DeletionDetails = includeDeletionDetails ? caseSubmission.DeletionDetails : null,
                PinnedAt = caseSubmission.PinnedAt
            };
        }

        public static CareCaseData ToCareCaseData(this CaseSubmission caseSubmission, ListCasesRequest listCasesRequest)
        {
            var resident = listCasesRequest.MosaicId != null ? caseSubmission.Residents
                .First(x => x.Id == long.Parse(listCasesRequest.MosaicId)) : caseSubmission.Residents[0];

            return new CareCaseData
            {
                RecordId = caseSubmission.SubmissionId.ToString(),
                PersonId = resident.Id,
                FirstName = resident.FirstName,
                LastName = resident.LastName,
                OfficerEmail = caseSubmission.Workers[0].Email,
                PinnedAt = caseSubmission.PinnedAt.ToString(),
                CaseFormTimestamp = caseSubmission.SubmittedAt?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                FormName = caseSubmission.FormId,
                DateOfBirth = resident.DateOfBirth?.ToString("dd/MM/yyyy"),
                DateOfEvent = caseSubmission.DateOfEvent?.ToString("O") ?? caseSubmission.CreatedAt.ToString("O"),
                CaseFormUrl = caseSubmission.SubmissionId.ToString(),
                FormType = "flexible-form",
                Title = caseSubmission.Title,
                Deleted = (bool) (caseSubmission.Deleted == null ? false : caseSubmission.Deleted),
                DeletionDetails = listCasesRequest.IncludeDeletedRecords ? caseSubmission.DeletionDetails : null
            };
        }

        public static AllocationSet ToEntity(this CreateAllocationRequest request, int workerId, DateTime allocationStartDate, string caseStatus)
        {
            return new AllocationSet
            {
                PersonId = (long) request.MosaicId,
                WorkerId = workerId,
                AllocationStartDate = allocationStartDate,
                CaseStatus = caseStatus,
                CreatedBy = request.CreatedBy
            };
        }
        public static PersonOtherName ToEntity(this OtherName name, long personId, string createdBy)
        {
            return new PersonOtherName
            {
                FirstName = name.FirstName,
                LastName = name.LastName,
                PersonId = personId,
                CreatedBy = createdBy
            };
        }

        public static dbPhoneNumber ToEntity(this PhoneNumber number, long personId, string createdBy)
        {
            return new dbPhoneNumber
            {
                Number = number.Number,
                Type = number.Type,
                PersonId = personId,
                CreatedBy = createdBy
            };
        }

        public static dbEmailAddress ToEntity(this EmailAddress entry)
        {
            return new dbEmailAddress()
            {
                Email = entry.Email,
                Type = entry.Type
            };
        }

        public static dbLastUpdated ToEntity(this LastUpdated entry, long personId)
        {
            return new dbLastUpdated
            {
                Type = entry.Type,
                UpdatedAt = entry.UpdatedAt,
                PersonId = personId,
            };
        }

        public static dbGpDetails ToEntity(this GpDetails entry, long personId)
        {
            return new dbGpDetails
            {
                Name = entry.Name,
                Email = entry.Email,
                Address = entry.Address,
                Postcode = entry.Postcode,
                PhoneNumber = entry.PhoneNumber,
                PersonId = personId,
            };
        }


        public static dbKeyContact ToEntity(this KeyContact contact, long personId)
        {
            return new dbKeyContact
            {
                Name = contact.Name,
                Email = contact.Email,
                PersonId = personId,
            };
        }

        public static dbTechUse ToEntity(this TechUse tech, long personId)
        {
            return new dbTechUse
            {
                TechType = tech.TechType,
                PersonId = personId,
            };
        }

        public static CaseNotesDocument ToEntity(this CreateCaseNoteRequest request)
        {
            GenericCaseNote note = new GenericCaseNote()
            {
                Timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"),
                DateOfBirth = request.DateOfBirth?.ToString("dd/MM/yyy"),
                DateOfEvent = request.DateOfEvent?.ToString(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                FormName = request.FormName,
                FormNameOverall = request.FormNameOverall,
                WorkerEmail = request.WorkerEmail,
                MosaicId = request.PersonId.ToString()
            };

            //serialize core properties
            JObject coreProps = JObject.Parse(JsonConvert.SerializeObject(note));

            //take the custom properties
            JObject caseFormData = JObject.Parse(request.CaseFormData);

            //merge both to one object
            coreProps.Merge(caseFormData);

            return new CaseNotesDocument()
            {
                CaseFormData = coreProps.ToString()
            };
        }

        public static dbWarningNote ToDatabaseEntity(this PostWarningNoteRequest request)
        {
            return new dbWarningNote
            {
                PersonId = request.PersonId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ReviewDate = request.ReviewDate,
                NextReviewDate = request.NextReviewDate,
                DisclosedWithIndividual = request.DisclosedWithIndividual,
                DisclosedDetails = request.DisclosedDetails,
                Notes = request.Notes,
                NoteType = request.NoteType,
                Status = "open",
                DisclosedDate = request.DisclosedDate,
                DisclosedHow = request.DisclosedHow,
                WarningNarrative = request.WarningNarrative,
                ManagerName = request.ManagerName,
                DiscussedWithManagerDate = request.DiscussedWithManagerDate,
                CreatedBy = request.CreatedBy
            };
        }

        public static Domain.CaseStatus ToDomain(this Infrastructure.CaseStatus caseStatus)
        {
            return new Domain.CaseStatus
            {
                Id = caseStatus.Id,
                Type = caseStatus.Type,
                StartDate = caseStatus.StartDate,
                EndDate = caseStatus.EndDate,
                Notes = caseStatus.Notes,
                Person = caseStatus.Person,
                Answers = caseStatus.Answers.Select(a => a.ToDomain()).ToList()
            };
        }

        public static Domain.CaseStatusAnswer ToDomain(this Infrastructure.CaseStatusAnswer caseStatusAnswer)
        {
            return new Domain.CaseStatusAnswer()
            {
                Option = caseStatusAnswer.Option,
                Value = caseStatusAnswer.Value,
                StartDate = caseStatusAnswer.StartDate,
                CreatedAt = caseStatusAnswer.CreatedAt,
                GroupId = caseStatusAnswer.GroupId,
                EndDate = caseStatusAnswer.EndDate,
                DiscardedAt = caseStatusAnswer.DiscardedAt
            };
        }
    }
}
