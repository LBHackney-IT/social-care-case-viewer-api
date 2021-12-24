using System;
using System.Collections.Generic;
using Bogus;
using MongoDB.Bson;
using SocialCareCaseViewerApi.Tests.V1.Gateways.WorkerGatewayTests;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using CaseSubmission = SocialCareCaseViewerApi.V1.Infrastructure.CaseSubmission;
using InfrastructurePerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using CaseStatus = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus;
using CaseStatusAnswer = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatusAnswer;
using MashReferral = SocialCareCaseViewerApi.V1.Infrastructure.MashReferral;
using MashResident = SocialCareCaseViewerApi.V1.Infrastructure.MashResident;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class TestHelpers
    {
        public static Visit CreateVisit(long? personId = null)
        {
            return new Faker<Visit>()
                .RuleFor(v => v.VisitId, f => f.UniqueIndex)
                .RuleFor(v => v.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(v => v.VisitType, f => f.Random.String2(1, 20))
                .RuleFor(v => v.PlannedDateTime, f => f.Date.Past().ToUniversalTime())
                .RuleFor(v => v.ActualDateTime, f => f.Date.Past().ToUniversalTime())
                .RuleFor(v => v.ReasonNotPlanned, f => f.Random.String2(1, 16))
                .RuleFor(v => v.ReasonVisitNotMade, f => f.Random.String2(1, 16))
                .RuleFor(v => v.SeenAloneFlag, f => f.Random.Bool())
                .RuleFor(v => v.CompletedFlag, f => f.Random.Bool())
                .RuleFor(v => v.CreatedByEmail, f => f.Person.Email)
                .RuleFor(v => v.CreatedByName, f => f.Person.FullName);
        }

        public static List<Visit> CreateVisits(long? personId = null)
        {
            var visits = new List<Visit>();

            for (var i = 0; i < new Random().Next(1, 5); i++)
            {
                visits.Add(CreateVisit(personId));
            }

            return visits;
        }

        public static CaseNote CreateCaseNote(string? noteType = null, DateTime? createdOn = null)
        {
            return new Faker<CaseNote>()
                .RuleFor(c => c.CaseNoteId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.MosaicId, f => f.UniqueIndex.ToString())
                .RuleFor(c => c.CreatedOn, f => createdOn ?? f.Date.Past().ToUniversalTime())
                .RuleFor(c => c.NoteType, f => noteType ?? f.Random.String2(50))
                .RuleFor(c => c.CaseNoteContent, f => f.Random.String2(50))
                .RuleFor(c => c.CaseNoteTitle, f => f.Random.String2(50))
                .RuleFor(c => c.CreatedByEmail, f => f.Person.Email)
                .RuleFor(c => c.CreatedByName, f => f.Person.FirstName);
        }

        public static CaseNoteResponse CreateCaseNoteResponse(CaseNote? caseNote)
        {
            return new Faker<CaseNoteResponse>()
                .RuleFor(c => c.RecordId, f => caseNote?.CaseNoteId ?? f.UniqueIndex.ToString())
                .RuleFor(c => c.PersonId, f => caseNote?.MosaicId ?? f.UniqueIndex.ToString())
                .RuleFor(c => c.Title, f => caseNote?.CaseNoteTitle ?? f.Lorem.Lines(1))
                .RuleFor(c => c.Content, f => caseNote?.CaseNoteContent ?? f.Lorem.Paragraph())
                .RuleFor(c => c.DateOfEvent, f => caseNote?.CreatedOn.ToString("s") ?? f.Date.Recent().ToString("s"))
                .RuleFor(c => c.OfficerEmail, f => caseNote?.CreatedByEmail ?? f.Person.Email)
                .RuleFor(c => c.OfficerName, f => caseNote?.CreatedByName ?? f.Person.FullName)
                .RuleFor(c => c.FormName, f => caseNote?.NoteType ?? f.Random.String2(10));
        }

        public static (CreateAllocationRequest, Worker, Worker, InfrastructurePerson, Team) CreateAllocationRequest(
            int? mosaicId = null,
            int? teamId = null,
            int? workerId = null,
            string? createdBy = null
        )
        {
            var worker = CreateWorker();
            var createdByWorker = CreateWorker();
            var person = CreatePerson();
            var team = CreateTeam();

            var createAllocationRequest = new Faker<CreateAllocationRequest>()
                .RuleFor(c => c.MosaicId, f => mosaicId ?? person.Id)
                .RuleFor(c => c.AllocatedTeamId, f => teamId ?? team.Id)
                .RuleFor(c => c.AllocatedWorkerId, f => workerId ?? worker.Id)
                .RuleFor(c => c.CreatedBy, f => createdBy ?? createdByWorker.Email)
                .RuleFor(c => c.AllocationStartDate, DateTime.Now);

            return (createAllocationRequest, worker, createdByWorker, person, team);
        }

        public static (UpdateAllocationRequest, Worker, Worker, InfrastructurePerson, Team)
            CreateUpdateAllocationRequest(
                int? id = null,
                string? deallocationReason = null,
                string? createdBy = null,
                DateTime? deallocationDate = null
            )
        {
            var worker = CreateWorker();
            var updatedByWorker = CreateWorker();
            var person = CreatePerson();
            var team = CreateTeam();

            var updateAllocationRequest = new Faker<UpdateAllocationRequest>()
                .RuleFor(u => u.Id, f => id ?? f.UniqueIndex + 1)
                .RuleFor(u => u.DeallocationReason, f => deallocationReason ?? f.Random.String2(200))
                .RuleFor(u => u.CreatedBy, createdBy ?? updatedByWorker.Email)
                .RuleFor(u => u.DeallocationDate, f => deallocationDate ?? f.Date.Recent());

            return (updateAllocationRequest, worker, updatedByWorker, person, team);
        }

        public static Worker CreateWorker(
            int? id = null,
            string? firstName = null,
            string? lastName = null,
            string? email = null,
            string? role = null,
            string? contextFlag = null,
            string? createdBy = null,
            DateTime? createdAt = null,
            bool isActive = true,
            bool hasAllocations = true,
            bool hasWorkerTeams = true)
        {
            DateTime? start = null;
            if (isActive)
            {
                start = DateTime.Now;
            }

            DateTime? end = null;
            if (!isActive)
            {
                end = DateTime.Now;
            }

            return new Faker<Worker>()
                .RuleFor(w => w.Id, f => id ?? f.UniqueIndex + 1)
                .RuleFor(w => w.FirstName, f => firstName ?? f.Person.FirstName)
                .RuleFor(w => w.LastName, f => lastName ?? f.Person.LastName)
                .RuleFor(w => w.Email, f => email ?? f.Person.Email)
                .RuleFor(w => w.Role, f => role ?? f.Random.String2(1, 200))
                .RuleFor(w => w.ContextFlag, f => contextFlag ?? f.Random.String2(1, "AC"))
                .RuleFor(w => w.CreatedBy, f => createdBy ?? f.Person.Email)
                .RuleFor(w => w.CreatedAt, f => createdAt ?? f.Date.Soon())
                .RuleFor(w => w.DateStart, start)
                .RuleFor(w => w.DateEnd, end)
                .RuleFor(w => w.IsActive, isActive)
                .RuleFor(w => w.Allocations, hasAllocations ? new List<AllocationSet> { CreateAllocation(), CreateAllocation(), CreateAllocation() } : null)
                .RuleFor(w => w.WorkerTeams, hasWorkerTeams ? new List<WorkerTeam> { CreateWorkerTeam(), CreateWorkerTeam(), CreateWorkerTeam() } : null)
                .RuleFor(w => w.LastModifiedBy, f => createdBy ?? f.Person.Email);
        }

        public static AllocationSet CreateAllocation(int? id = null, int? personId = null, int? workerId = null, int? teamId = null)
        {
            var caseStatusChoices = new List<string> { "open", "closed" };

            return new Faker<AllocationSet>()
                .RuleFor(a => a.Id, f => id ?? f.UniqueIndex + 1)
                .RuleFor(a => a.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(a => a.WorkerId, f => workerId ?? f.UniqueIndex + 1)
                .RuleFor(a => a.TeamId, f => teamId ?? f.UniqueIndex + 1)
                .RuleFor(a => a.AllocationStartDate, f => f.Date.Past().ToUniversalTime())
                .RuleFor(a => a.AllocationEndDate, f => f.Date.Past().ToUniversalTime())
                .RuleFor(a => a.CaseStatus, f => f.PickRandom(caseStatusChoices))
                .RuleFor(a => a.CaseClosureDate, f => f.Date.Past().ToUniversalTime());


        }

        public static InfrastructurePerson CreatePerson(long? personId = null, string? firstName = null, string? lastName = null, string? ageContext = null)
        {
            return new Faker<InfrastructurePerson>()
                .RuleFor(p => p.Id, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(p => p.Title, f => f.Name.Prefix())
                .RuleFor(p => p.FirstName, f => firstName ?? f.Person.FirstName)
                .RuleFor(p => p.LastName, f => lastName ?? f.Person.FirstName)
                .RuleFor(p => p.FullName, f => f.Person.FullName)
                .RuleFor(p => p.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(p => p.DateOfDeath, f => f.Date.Recent())
                .RuleFor(p => p.Ethnicity, f => f.Random.String2(0, 30))
                .RuleFor(p => p.FirstLanguage, f => f.Random.String2(10, 100))
                .RuleFor(p => p.Religion, f => f.Random.String2(10, 80))
                .RuleFor(p => p.EmailAddress, f => f.Person.Email)
                .RuleFor(p => p.Gender, f => f.Random.String2(1, "MF"))
                .RuleFor(p => p.Nationality, f => f.Address.Country())
                .RuleFor(p => p.NhsNumber, f => f.Random.Number(int.MaxValue))
                .RuleFor(p => p.PersonIdLegacy, f => f.Random.String2(16))
                .RuleFor(p => p.AgeContext, f => ageContext ?? f.Random.String2(1, "ACac"))
                .RuleFor(p => p.DataIsFromDmPersonsBackup, f => f.Random.String2(1))
                .RuleFor(p => p.SexualOrientation, f => f.Random.String2(100))
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.String2(100))
                .RuleFor(p => p.Restricted, f => f.Random.String2(1));
        }

        public static Address CreateAddress(long? personId = null, string? postCode = null, string? address = null, int? uprn = null)
        {
            var person = CreatePerson();

            return new Faker<Address>()
                .RuleFor(a => a.Person, person)
                .RuleFor(a => a.PersonAddressId, f => f.Random.Number(int.MaxValue))
                .RuleFor(a => a.AddressId, f => f.Random.Number(999999999))
                .RuleFor(a => a.PersonId, personId ?? person.Id)
                .RuleFor(a => a.EndDate, f => f.Date.Recent().Date)
                .RuleFor(a => a.AddressLines, f => address ?? f.Address.FullAddress())
                .RuleFor(a => a.PostCode, f => postCode ?? f.Address.ZipCode())
                .RuleFor(a => a.Uprn, f => uprn ?? f.Random.Number(int.MaxValue))
                .RuleFor(a => a.DataIsFromDmPersonsBackup, f => f.Random.String2(1, "YN"))
                .RuleFor(a => a.IsDisplayAddress, f => f.Random.String2(1, "YN"));
        }

        public static PhoneNumber CreatePhoneNumber(long? personId = null)
        {
            var person = CreatePerson();

            return new Faker<PhoneNumber>()
                .RuleFor(p => p.Person, person)
                .RuleFor(p => p.Id, f => f.UniqueIndex)
                .RuleFor(p => p.PersonId, personId ?? person.Id)
                .RuleFor(p => p.Number, f => f.Random.Number(int.MaxValue).ToString())
                .RuleFor(p => p.Type, f => f.Random.String2(1, 80));
        }

        public static PersonOtherName CreatePersonOtherName(long? personId = null)
        {
            var person = CreatePerson();

            return new Faker<PersonOtherName>()
                .RuleFor(p => p.Person, person)
                .RuleFor(p => p.Id, f => f.UniqueIndex)
                .RuleFor(p => p.PersonId, personId ?? person.Id)
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.LastName);
        }

        public static AddNewResidentResponse CreateAddNewResidentResponse(
            long? personId = null,
            long? addressId = null,
            string? caseNoteId = null,
            List<int>? otherNameIds = null,
            List<int>? phoneNumberIds = null
        )
        {
            var person = CreatePerson();
            var address = CreateAddress();
            var caseNote = CreateCaseNote();

            return new Faker<AddNewResidentResponse>()
                .RuleFor(a => a.Id, personId ?? person.Id)
                .RuleFor(a => a.AddressId, addressId ?? address.AddressId)
                .RuleFor(a => a.OtherNameIds, otherNameIds ?? new Faker<List<int>>())
                .RuleFor(a => a.PhoneNumberIds, phoneNumberIds ?? new Faker<List<int>>())
                .RuleFor(a => a.CaseNoteId, caseNoteId ?? caseNote.CaseNoteId)
                .RuleFor(a => a.CaseNoteErrorMessage, f => f.Random.String2(100));
        }

        public static CreateWorkerRequest CreateWorkerRequest(
            bool createATeam = true,
            int? teamId = null,
            string? teamName = null,
            string? email = null,
            string? firstName = null,
            string? lastName = null,
            string? contextFlag = null,
            string? role = null,
            string? createdByEmail = null,
            DateTime? dateStart = null
        )
        {
            var team = CreateWorkerRequestWorkerTeam(teamId, teamName);
            var teams = createATeam ? new List<WorkerTeamRequest> { team } : new List<WorkerTeamRequest>();

            return new Faker<CreateWorkerRequest>()
                .RuleFor(w => w.EmailAddress, f => email ?? f.Person.Email)
                .RuleFor(w => w.FirstName, f => firstName ?? f.Person.FirstName)
                .RuleFor(w => w.LastName, f => lastName ?? f.Person.LastName)
                .RuleFor(w => w.ContextFlag, f => contextFlag ?? f.Random.String2(1, "AC"))
                .RuleFor(w => w.Teams, teams)
                .RuleFor(w => w.Role, f => role ?? f.Random.String2(200))
                .RuleFor(w => w.DateStart, dateStart ?? DateTime.Now)
                .RuleFor(w => w.CreatedBy, f => createdByEmail ?? f.Person.Email);
        }

        public static WorkerResponse CreateWorkerResponse(
            int? id = null,
            string? firstName = null,
            string? lastName = null,
            string? email = null,
            int? allocationCount = null,
            string? role = null)
        {
            return new Faker<WorkerResponse>()
                .RuleFor(w => w.Id, f => id ?? f.UniqueIndex)
                .RuleFor(w => w.FirstName, f => firstName ?? f.Person.FirstName)
                .RuleFor(w => w.LastName, f => lastName ?? f.Person.LastName)
                .RuleFor(w => w.Email, f => email ?? f.Person.Email)
                .RuleFor(w => w.AllocationCount, f => allocationCount ?? f.Random.Int(1, 10))
                .RuleFor(w => w.Role, f => role ?? f.Random.String2(1, 200));
        }

        public static WorkerTeam CreateWorkerTeam(
            int? workerId = null
        )
        {
            var team = CreateTeam();

            return new Faker<WorkerTeam>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.WorkerId, f => workerId ?? f.UniqueIndex + 1)
                .RuleFor(t => t.TeamId, f => f.UniqueIndex)
                .RuleFor(t => t.Team, team);
        }

        private static WorkerTeamRequest CreateWorkerRequestWorkerTeam(
            int? teamId = null,
            string? teamName = null
        )
        {
            return new Faker<WorkerTeamRequest>()
                .RuleFor(t => t.Id, f => teamId ?? f.UniqueIndex + 1)
                .RuleFor(t => t.Name, f => teamName ?? f.Random.String2(200));
        }

        public static Team CreateTeam(int? teamId = null, string? name = null, string? context = null)
        {
            return new Faker<Team>()
                .RuleFor(t => t.Id, f => teamId ?? f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => context ?? f.Random.String2(1, "AC"))
                .RuleFor(t => t.Name, f => name ?? f.Random.String2(1, 200));
        }

        public static WarningNote CreateWarningNote(long? personId = null, string? status = null)
        {
            return new Faker<WarningNote>()
                .RuleFor(w => w.Id, f => f.UniqueIndex + 1)
                .RuleFor(w => w.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(w => w.StartDate, f => f.Date.Recent())
                .RuleFor(w => w.ReviewDate, f => f.Date.Recent())
                .RuleFor(w => w.EndDate, f => f.Date.Recent())
                .RuleFor(w => w.LastReviewDate, f => f.Date.Recent())
                .RuleFor(w => w.NextReviewDate, f => f.Date.Recent())
                .RuleFor(w => w.DisclosedWithIndividual, f => f.Random.Bool())
                .RuleFor(w => w.DisclosedDetails, f => f.Random.String2(1, 1000))
                .RuleFor(w => w.Notes, f => f.Random.String2(1, 1000))
                .RuleFor(w => w.NoteType, f => f.Random.String2(1, 50))
                .RuleFor(w => w.Status, f => status ?? "open")
                .RuleFor(w => w.DisclosedDate, f => f.Date.Recent())
                .RuleFor(w => w.DisclosedHow, f => f.Random.String2(1, 50))
                .RuleFor(w => w.WarningNarrative, f => f.Random.String2(1, 1000))
                .RuleFor(w => w.ManagerName, f => f.Random.String2(1, 100))
                .RuleFor(w => w.DiscussedWithManagerDate, f => f.Date.Recent())
                .RuleFor(w => w.CreatedBy, f => f.Internet.Email());
        }


        public static (PatchWarningNoteRequest, InfrastructurePerson, Worker, WarningNote) CreatePatchWarningNoteRequest(
            long? warningNoteId = null,
            DateTime? reviewDate = null,
            string? reviewedBy = null,
            DateTime? nextReviewDate = null,
            string? startingStatus = "open",
            string? requestStatus = "open",
            string? endedBy = null,
            string? reviewNotes = null,
            string? managerName = null,
            DateTime? discussedWithManagerDate = null,
            bool? disclosedWithIndividual = null
        )
        {
            var person = CreatePerson();
            var worker = CreateWorker();
            WarningNote warningNote = CreateWarningNote(person.Id, startingStatus);

            var patchWarningNoteRequest = new Faker<PatchWarningNoteRequest>()
                .RuleFor(p => p.WarningNoteId, f => warningNoteId ?? warningNote.Id)
                .RuleFor(p => p.ReviewDate, f => reviewDate ?? f.Date.Recent())
                .RuleFor(p => p.ReviewedBy, f => reviewedBy ?? worker.Email)
                .RuleFor(p => p.NextReviewDate, f => nextReviewDate ?? f.Date.Future())
                .RuleFor(p => p.DisclosedWithIndividual, f => disclosedWithIndividual)
                .RuleFor(p => p.Status, f => requestStatus)
                .RuleFor(p => p.EndedBy, f => endedBy ?? worker.Email)
                .RuleFor(p => p.ReviewNotes, f => reviewNotes ?? f.Random.String2(1, 1000))
                .RuleFor(p => p.ManagerName, f => managerName ?? f.Random.String2(1, 100))
                .RuleFor(p => p.DiscussedWithManagerDate, f => discussedWithManagerDate ?? f.Date.Recent());

            return (patchWarningNoteRequest, person, worker, warningNote);
        }

        public static UpdateWorkerRequest CreateUpdateWorkersRequest(
            bool createATeam = true,
            int? teamId = null,
            string? teamName = null,
            int? workerId = null,
            string? modifiedBy = null,
            string? firstName = null,
            string? lastName = null,
            string? contextFlag = null,
            string? role = null,
            DateTime? dateStart = null)
        {
            var team = CreateWorkerRequestWorkerTeam(teamId, teamName);
            var teams = createATeam ? new List<WorkerTeamRequest> { team } : new List<WorkerTeamRequest>();

            return new Faker<UpdateWorkerRequest>()
                .RuleFor(w => w.WorkerId, f => workerId ?? f.UniqueIndex + 1)
                .RuleFor(w => w.ModifiedBy, f => modifiedBy ?? f.Person.Email)
                .RuleFor(w => w.FirstName, f => firstName ?? f.Person.FirstName)
                .RuleFor(w => w.LastName, f => lastName ?? f.Person.LastName)
                .RuleFor(w => w.ContextFlag, f => contextFlag ?? f.Random.String2(1, "AC"))
                .RuleFor(w => w.Teams, teams)
                .RuleFor(w => w.Role, f => role ?? f.Random.String2(200))
                .RuleFor(w => w.DateStart, f => dateStart ?? f.Date.Recent());
        }

        public static WarningNoteReview CreateWarningNoteReview(long warningNoteId, bool? disclosedWithIndividual = null)
        {
            return new Faker<WarningNoteReview>()
                .RuleFor(r => r.Id, f => f.UniqueIndex)
                .RuleFor(r => r.WarningNoteId, f => warningNoteId)
                .RuleFor(r => r.ReviewDate, f => f.Date.Future())
                .RuleFor(r => r.DisclosedWithIndividual, f => disclosedWithIndividual)
                .RuleFor(r => r.ReviewNotes, f => f.Random.String2(1, 50))
                .RuleFor(r => r.ManagerName, f => f.Person.FullName)
                .RuleFor(r => r.DiscussedWithManagerDate, f => f.Date.Past())
                .RuleFor(r => r.CreatedAt, f => f.Date.Past())
                .RuleFor(r => r.CreatedBy, f => f.Person.FullName)
                .RuleFor(r => r.LastModifiedAt, f => f.Date.Recent())
                .RuleFor(r => r.LastModifiedBy, f => f.Person.FullName);
        }

        public static CreateTeamRequest CreateTeamRequest(string? name = null, string? context = null)
        {
            return new Faker<CreateTeamRequest>()
                .RuleFor(t => t.Name, f => name ?? f.Random.String2(1, 200))
                .RuleFor(t => t.Context, f => context ?? f.Random.String2(1, "AC"));
        }

        public static TeamResponse CreateTeamResponse(int? id = null, string? name = null, string? context = null)
        {
            return new Faker<TeamResponse>()
                .RuleFor(t => t.Id, f => id ?? f.UniqueIndex)
                .RuleFor(t => t.Name, f => name ?? f.Random.String2(1, 200))
                .RuleFor(t => t.Context, f => context ?? f.Random.String2(1, "AC"));
        }

        public static GetTeamsRequest CreateGetTeamsRequest(string? contextFlag = null)
        {
            return new Faker<GetTeamsRequest>()
                .RuleFor(t => t.ContextFlag, f => contextFlag ?? f.Random.String2(1, "ACac"));
        }

        public static CreateCaseSubmissionRequest CreateCaseSubmissionRequest(
            string? formId = null,
            int? residentId = null,
            string? createdBy = null)
        {
            return new Faker<CreateCaseSubmissionRequest>()
                .RuleFor(s => s.FormId, f => formId ?? f.Random.String2(20))
                .RuleFor(s => s.ResidentId, f => residentId ?? f.UniqueIndex + 1)
                .RuleFor(s => s.CreatedBy, f => createdBy ?? f.Person.Email);
        }

        public static UpdateFormSubmissionAnswersRequest CreateUpdateFormSubmissionAnswersRequest(string? editedBy = null,
            string? stepAnswers = null,
            DateTime? dateOfEvent = null,
            string? title = null)
        {
            stepAnswers ??= "{\"1\":\"one\"}";

            return new Faker<UpdateFormSubmissionAnswersRequest>()
                .RuleFor(u => u.EditedBy, f => editedBy ?? f.Person.Email)
                .RuleFor(u => u.StepAnswers, stepAnswers)
                .RuleFor(u => u.DateOfEvent, dateOfEvent)
                .RuleFor(u => u.Title, title);
        }

        public static CaseSubmission CreateCaseSubmission(SubmissionState? submissionState = null,
            DateTime? createdAt = null,
            List<Worker>? workers = null,
            List<InfrastructurePerson>? residents = null,
            int? residentId = null,
            ObjectId? id = null,
            string? formId = null,
            DateTime? dateOfEvent = null,
            DateTime? submittedAt = null,
            string? title = null,
            bool? deleted = null)
        {
            workers ??= new List<Worker> { CreateWorker() };
            residents ??= new List<InfrastructurePerson> { CreatePerson(residentId) };

            return new Faker<CaseSubmission>()
                .RuleFor(s => s.SubmissionId, f => id ?? ObjectId.Parse(f.Random.String2(24, "0123456789abcdef")))
                .RuleFor(s => s.FormId, f => formId ?? f.Random.String2(20))
                .RuleFor(s => s.Residents, residents)
                .RuleFor(s => s.Workers, workers)
                .RuleFor(s => s.CreatedAt, f => createdAt ?? f.Date.Recent())
                .RuleFor(s => s.CreatedBy, workers[0])
                .RuleFor(s => s.EditHistory,
                    f => new List<EditHistory<Worker>>
                    {
                        new EditHistory<Worker> {Worker = workers[0], EditTime = createdAt ?? f.Date.Recent()}
                    })
                .RuleFor(s => s.SubmissionState, f => submissionState ?? SubmissionState.InProgress)
                .RuleFor(s => s.FormAnswers, new Dictionary<string, string>() { { "foo", "bar" } })
                .RuleFor(s => s.DateOfEvent, dateOfEvent)
                .RuleFor(s => s.SubmittedAt, submittedAt)
                .RuleFor(s => s.Title, title)
                .RuleFor(s => s.Deleted, deleted)
                .RuleFor(s => s.DeletionDetails, f => deleted == true ? new DeletionDetails()
                {
                    DeletedAt = f.Date.Recent(),
                    DeletedBy = f.Person.Email,
                    DeleteReason = f.Random.String2(200),
                    DeleteRequestedBy = f.Person.FullName
                } : null
                );
        }

        public static ListCasesRequest CreateListCasesRequest(
            long? mosaicId = null,
            string? firstName = null,
            string? lastName = null,
            string? workerEmail = null,
            bool? includeDeletedRecords = null,
            bool? includeDeletedRecordsCount = null)
        {
            return new Faker<ListCasesRequest>()
                .RuleFor(r => r.MosaicId, mosaicId == null ? null : mosaicId.ToString())
                .RuleFor(r => r.FirstName, firstName)
                .RuleFor(r => r.LastName, lastName)
                .RuleFor(r => r.WorkerEmail, workerEmail)
                .RuleFor(r => r.IncludeDeletedRecords, includeDeletedRecords ?? false)
                .RuleFor(r => r.IncludeDeletedRecordsCount, includeDeletedRecordsCount ?? false);
        }

        public static UpdateCaseSubmissionRequest UpdateCaseSubmissionRequest(
            string? updatedBy = null,
            string? submissionState = null,
            List<long>? residents = null,
            string? rejectionReason = null
        )
        {
            return new Faker<UpdateCaseSubmissionRequest>()
                .RuleFor(s => s.EditedBy, f => updatedBy ?? f.Person.Email)
                .RuleFor(s => s.SubmissionState, submissionState)
                .RuleFor(s => s.Residents, residents)
                .RuleFor(s => s.RejectionReason, rejectionReason);
        }

        public static DeleteCaseSubmissionRequest DeleteCaseSubmissionRequest(
            string? deletedBy = null,
            string? deletedReason = null,
            string? deleteRequestedBy = null
        )
        {
            return new Faker<DeleteCaseSubmissionRequest>()
                .RuleFor(x => x.DeletedBy, f => deletedBy ?? f.Person.Email)
                .RuleFor(x => x.DeleteReason, f => deletedReason ?? f.Random.String2(100))
                .RuleFor(x => x.DeleteRequestedBy, f => deleteRequestedBy ?? f.Person.FullName);
        }

        public static CaseStatus CreateCaseStatus(
            long? personId = null,
            string? notes = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            InfrastructurePerson? resident = null,
            string? type = null,
            long? id = null)
        {
            resident ??= CreatePerson();

            return new Faker<CaseStatus>()
                .RuleFor(cs => cs.Id, f => id ?? f.UniqueIndex + 1)
                .RuleFor(cs => cs.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(cs => cs.Notes, f => notes ?? f.Random.String2(1000))
                .RuleFor(cs => cs.StartDate, f => startDate ?? f.Date.Past())
                .RuleFor(cs => cs.EndDate, f => endDate ?? f.Date.Future())
                .RuleFor(cs => cs.Person, resident)
                .RuleFor(cs => cs.Answers, new List<CaseStatusAnswer>())
                .RuleFor(cs => cs.Type, f => type ?? f.Random.String2(3));
        }

        public static QueryCaseSubmissionsRequest CreateQueryCaseSubmissions(
            string? formId = null,
            IEnumerable<string>? submissionStates = null,
            DateTime? createdBefore = null,
            DateTime? createdAfter = null,
            int page = 0,
            int size = 100,
            bool? includeEditHistory = null,
            bool? includeFormAnswers = null,
            string? ageContext = null,
            string? workerEmail = null,
            long? personID = null,
            bool? pruneUnfinished = null,
            bool? includeDeletedRecords = null,
            bool? includeDeletedRecordsCount = null)
        {
            return new Faker<QueryCaseSubmissionsRequest>()
                .RuleFor(q => q.FormId, formId)
                .RuleFor(q => q.SubmissionStates, submissionStates)
                .RuleFor(q => q.CreatedBefore, createdBefore)
                .RuleFor(q => q.CreatedAfter, createdAfter)
                .RuleFor(q => q.Page, page)
                .RuleFor(q => q.Size, size)
                .RuleFor(q => q.IncludeEditHistory, f => includeEditHistory ?? f.Random.Bool())
                .RuleFor(q => q.IncludeFormAnswers, f => includeFormAnswers ?? f.Random.Bool())
                .RuleFor(q => q.AgeContext, ageContext)
                .RuleFor(q => q.WorkerEmail, workerEmail)
                .RuleFor(q => q.PersonID, personID)
                .RuleFor(q => q.PruneUnfinished, f => pruneUnfinished ?? f.Random.Bool())
                .RuleFor(q => q.IncludeDeletedRecords, f => includeDeletedRecords ?? f.Random.Bool())
                .RuleFor(q => q.IncludeDeletedRecordsCount, f => includeDeletedRecordsCount ?? f.Random.Bool());
        }

        private static List<CaseStatusValue> CreateCaseStatusValues(int? min, int? max)
        {
            var caseStatusValues = new List<CaseStatusValue>();

            for (var i = 0; i < new Random().Next(min ?? 1, max ?? 10); i++)
            {
                var value = new Faker<CaseStatusValue>()
                    .RuleFor(c => c.Option, f => f.Random.String2(512))
                    .RuleFor(c => c.Value, f => f.Random.String2(512));

                caseStatusValues.Add(value);
            }

            return caseStatusValues;
        }

        public static List<CaseStatusAnswer> CreateCaseStatusAnswers(
            DateTime? startDate = null,
            DateTime? createdAt = null,
            long? caseStatusId = null,
            string? groupId = null,
            int? min = null,
            int? max = null,
            DateTime? endDate = null,
            DateTime? discardedAt = null)
        {
            var caseStatusAnswers = new List<CaseStatusAnswer>();

            for (var i = 0; i < new Random().Next(min ?? 1, max ?? 2); i++)
            {
                var answer = new Faker<CaseStatusAnswer>()
                    .RuleFor(a => a.CaseStatusId, f => caseStatusId ?? f.Random.Long())
                    .RuleFor(a => a.Option, f => f.Random.String2(100))
                    .RuleFor(a => a.Value, f => f.Random.String2(100))
                    .RuleFor(a => a.GroupId, f => groupId ?? f.Random.String2(36))
                    .RuleFor(a => a.StartDate, f => startDate ?? f.Date.Past())
                    .RuleFor(a => a.CreatedAt, f => createdAt ?? f.Date.Past())
                    .RuleFor(a => a.EndDate, f => endDate)
                    .RuleFor(a => a.DiscardedAt, f => discardedAt);

                caseStatusAnswers.Add(answer);
            }

            return caseStatusAnswers;
        }

        public static UpdateCaseStatusRequest CreateUpdateCaseStatusRequest(DateTime? endDate = null, string? email = null,
            string? notes = null, long? caseStatusId = null, int? min = null, int? max = null, DateTime? startDate = null)
        {
            return new Faker<UpdateCaseStatusRequest>()
                .RuleFor(u => u.CaseStatusId, f => caseStatusId ?? f.UniqueIndex + 1)
                .RuleFor(u => u.EndDate, f => endDate ?? f.Date.Future())
                .RuleFor(u => u.EditedBy, f => email ?? f.Person.Email)
                .RuleFor(u => u.Notes, f => notes ?? f.Random.String2(512))
                .RuleFor(u => u.Answers, CreateCaseStatusValues(min ?? null, max ?? null))
                .RuleFor(u => u.StartDate, f => startDate ?? null);
        }

        public static QueryMashReferrals CreateQueryMashReferral(long? id = null)
        {
            return new Faker<QueryMashReferrals>()
                .RuleFor(x => x.Id, f => id ?? f.Random.Long(0, 100));
        }

        public static UpdateMashReferral CreateUpdateMashReferral(
            string? workerEmail = null,
            string? updateType = null,
            string? decision = null,
            bool? requiresUrgentContact = null,
            string? referralCategory = null,
            int? workerId = null)
        {
            var updateTypes = new List<string> { "SCREENING-DECISION", "INITIAL-DECISION", "ASSIGN-WORKER" };

            return new Faker<UpdateMashReferral>()
                .RuleFor(x => x.WorkerEmail, f => workerEmail ?? f.Person.Email)
                .RuleFor(x => x.UpdateType, f => updateType ?? f.PickRandom(updateTypes))
                .RuleFor(x => x.Decision, f => decision ?? f.Random.String2(100))
                .RuleFor(x => x.RequiresUrgentContact, f => requiresUrgentContact ?? f.Random.Bool())
                .RuleFor(x => x.ReferralCategory, f => referralCategory ?? f.Random.String2(100))
                .RuleFor(x => x.WorkerId, f => workerId ?? f.UniqueIndex);

        }

        public static MashResident CreateMashResident(long? mashReferralId = null)
        {
            return new Faker<MashResident>()
                .RuleFor(x => x.Id, f => f.UniqueIndex)
                .RuleFor(x => x.FirstName, f => f.Random.String2(50))
                .RuleFor(x => x.LastName, f => f.Random.String2(50))
                .RuleFor(x => x.DateOfBirth, f => f.Date.Past())
                .RuleFor(x => x.Gender, f => f.Random.String2(20))
                .RuleFor(x => x.Ethnicity, f => f.Random.String2(33))
                .RuleFor(x => x.FirstLanguage, f => f.Random.String2(100))
                .RuleFor(x => x.School, f => f.Random.String2(100))
                .RuleFor(x => x.Address, f => f.Random.String2(100))
                .RuleFor(x => x.Postcode, f => f.Random.String2(20))
                .RuleFor(x => x.MashReferralId, f => mashReferralId ?? f.UniqueIndex);
        }

        public static MashReferral CreateMashReferral(string? stage = null, long? mashReferralId = null)
        {
            return new Faker<MashReferral>()
                .RuleFor(x => x.Id, f => mashReferralId ?? f.UniqueIndex)
                .RuleFor(x => x.Referrer, f => f.Random.String2(20))
                .RuleFor(x => x.Worker, CreateWorker())
                .RuleFor(x => x.RequestedSupport, f => f.Random.String2(20))
                .RuleFor(x => x.ReferralCreatedAt, f => f.Date.Recent())
                .RuleFor(x => x.ReferralDocumentURI, f => f.Random.String2(20))
                .RuleFor(x => x.Stage, f => stage ?? f.Random.String2(20))
                .RuleFor(x => x.ReferralCategory, f => f.Random.String2(20))
                .RuleFor(x => x.InitialDecision, f => f.Random.String2(20))
                .RuleFor(x => x.InitialDecisionReferralCategory, f => f.Random.String2(20))
                .RuleFor(x => x.InitialDecisionUrgentContactRequired, f => f.Random.Bool())
                .RuleFor(x => x.InitialDecisionCreatedAt, f => f.Date.Recent())
                .RuleFor(x => x.ScreeningDecision, f => f.Random.String2(100))
                .RuleFor(x => x.ScreeningUrgentContactRequired, f => f.Random.Bool())
                .RuleFor(x => x.ScreeningCreatedAt, f => f.Date.Recent())
                .RuleFor(x => x.FinalDecision, f => f.Random.String2(20))
                .RuleFor(x => x.FinalDecisionReferralCategory, f => f.Random.String2(20))
                .RuleFor(x => x.FinalDecisionUrgentContactRequired, f => f.Random.Bool())
                .RuleFor(x => x.FinalDecisionCreatedAt, f => f.Date.Recent())
                .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
                .RuleFor(x => x.CreatedBy, f => f.Random.String2(20))
                .RuleFor(x => x.LastModifiedAt, f => f.Date.Recent())
                .RuleFor(x => x.LastModifiedBy, f => f.Random.String2(20))
                .RuleFor(x => x.MashResidents, new List<MashResident>());
        }


        public static MashResidentRequest CreateNewMashResidentRequest()
        {
            return new Faker<MashResidentRequest>()
                .RuleFor(x => x.FirstName, f => f.Random.String2(50))
                .RuleFor(x => x.LastName, f => f.Random.String2(50))
                .RuleFor(x => x.DateOfBirth, f => f.Date.Past())
                .RuleFor(x => x.Gender, f => f.Random.String2(50))
                .RuleFor(x => x.Ethnicity, f => f.Random.String2(33))
                .RuleFor(x => x.FirstLanguage, f => f.Random.String2(100))
                .RuleFor(x => x.School, f => f.Random.String2(100))
                .RuleFor(x => x.Address, f => f.Random.String2(50))
                .RuleFor(x => x.Postcode, f => f.Random.String2(20));
        }

        public static CreateReferralRequest CreateNewMashReferralRequest(
            string? referrer = null,
            string? requestedSupport = null,
            string? referralUri = null)
        {
            var mashResidentsRequests = new List<MashResidentRequest>();

            for (var i = 0; i < new Random().Next(10) + 1; i++)
            {
                mashResidentsRequests.Add(CreateNewMashResidentRequest());
            }

            return new Faker<CreateReferralRequest>()
                .RuleFor(x => x.Referrer, f => referrer ?? f.Random.String2(20))
                .RuleFor(x => x.RequestedSupport, f => requestedSupport ?? f.Random.String2(20))
                .RuleFor(x => x.ReferralUri, f => referralUri ?? f.Random.String2(20))
                .RuleFor(x => x.MashResidents, mashResidentsRequests);
        }
    }
}
