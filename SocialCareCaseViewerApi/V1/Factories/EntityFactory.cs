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
using DbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using dbWarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using DbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class EntityFactory
    {
        public static ResidentInformation ToDomain(this Person databaseEntity)
        {
            return new ResidentInformation
            {
                PersonId = databaseEntity.Id.ToString(),
                Title = databaseEntity.Title,
                FirstName = databaseEntity.FirstName,
                LastName = databaseEntity.LastName,
                DateOfBirth = databaseEntity.DateOfBirth?.ToString("O"),
                Gender = databaseEntity.Gender,
                Nationality = databaseEntity.Nationality,
                NhsNumber = databaseEntity.NhsNumber?.ToString(),

            };
        }

        public static List<ResidentInformation> ToDomain(this IEnumerable<Person> people)
        {
            return people.Select(p => p.ToDomain()).ToList();
        }

        public static Address ToDomain(this DbAddress address)
        {
            return new Address
            {
                AddressLine1 = address.AddressLines,
                PostCode = address.PostCode
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
                AllocationCount = worker.Allocations?.Where(x => x.CaseStatus.ToUpper() == "OPEN").Count() ?? 0,
                Teams = (includeTeamData ? worker.WorkerTeams?.Select(x => new Team() { Id = x.Team.Id, Name = x.Team.Name, Context = x.Team.Context }).ToList() : null) ?? new List<Team>()
            };
        }

        public static List<Worker> ToDomain(this IEnumerable<DbWorker> workers, bool includeTeamData)
        {
            return workers.Select(w => w.ToDomain(includeTeamData)).ToList();
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

        public static List<Team> ToDomain(this IEnumerable<DbTeam> teams)
        {
            return teams.Select(t => t.ToDomain()).ToList();
        }

        public static WarningNote ToDomain(this dbWarningNote dbWarningNote, List<WarningNoteReview> reviews = null)
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

        public static OtherName ToDomain(this PersonOtherName otherName)
        {
            return new OtherName()
            {
                FirstName = otherName.FirstName,
                LastName = otherName.LastName
            };
        }

        public static Domain.CaseSubmission ToDomain(this CaseSubmission caseSubmission)
        {
            var mapSubmissionStateToString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "In progress" },
                { SubmissionState.Submitted, "Submitted" }
            };

            return new Domain.CaseSubmission
            {
                SubmissionId = caseSubmission.SubmissionId,
                FormId = caseSubmission.FormId,
                Residents = caseSubmission.Residents,
                Workers = caseSubmission.Workers.Select(w => w.ToDomain(false)).ToList(),
                CreatedAt = caseSubmission.CreatedAt,
                CreatedBy = caseSubmission.CreatedBy.ToDomain(false),
                EditHistory = caseSubmission.EditHistory.Select(e => new EditHistory<Worker>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToDomain(false)
                }).ToList(),
                SubmissionState = mapSubmissionStateToString[caseSubmission.SubmissionState],
                FormAnswers = caseSubmission.FormAnswers
            };
        }

        public static AllocationSet ToEntity(this CreateAllocationRequest request, int workerId, DateTime allocationStartDate, string caseStatus)
        {
            return new AllocationSet
            {
                PersonId = request.MosaicId,
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
    }
}
