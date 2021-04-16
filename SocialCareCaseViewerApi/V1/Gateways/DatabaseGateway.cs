using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class DatabaseGateway : IDatabaseGateway
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IProcessDataGateway _processDataGateway;

        public DatabaseGateway(DatabaseContext databaseContext, IProcessDataGateway processDataGateway)
        {
            _databaseContext = databaseContext;
            _processDataGateway = processDataGateway;
        }

        public List<Allocation> SelectAllocations(long mosaicId, long workerId)
        {
            List<Allocation> allocations = new List<Allocation>();

            //TODO: optimise these queries
            if (mosaicId != 0)
            {
                allocations = _databaseContext.Allocations
                    .Where(x => x.PersonId == mosaicId)
                    .Include(x => x.Team)
                    .Include(x => x.Person)
                    .ThenInclude(y => y.Addresses)
                    .Select(x => new Allocation()
                    {
                        Id = x.Id,
                        PersonId = x.PersonId,
                        PersonDateOfBirth = x.Person.DateOfBirth,
                        PersonName = ToTitleCaseFullPersonName(x.Person.FirstName, x.Person.LastName),
                        AllocatedWorker = x.Worker == null ? null : $"{x.Worker.FirstName} {x.Worker.LastName }",
                        AllocatedWorkerTeam = x.Team.Name,
                        WorkerType = x.Worker.Role,
                        AllocationStartDate = x.AllocationStartDate,
                        AllocationEndDate = x.AllocationEndDate,
                        CaseStatus = x.CaseStatus,
                        PersonAddress = x.Person.Addresses.FirstOrDefault(x => !string.IsNullOrEmpty(x.IsDisplayAddress) && x.IsDisplayAddress.ToUpper() == "Y") == null ? null : x.Person.Addresses.FirstOrDefault(x => x.IsDisplayAddress.ToUpper() == "Y").AddressLines
                    }
                    ).AsNoTracking().ToList();

            }
            else if (workerId != 0)
            {
                allocations = _databaseContext.Allocations
                    .Where(x => x.WorkerId == workerId)
                    .Include(x => x.Team)
                    .Include(x => x.Person)
                    .ThenInclude(y => y.Addresses)
                    .Select(x => new Allocation()
                    {
                        Id = x.Id,
                        PersonId = x.PersonId,
                        PersonDateOfBirth = x.Person.DateOfBirth,
                        PersonName = ToTitleCaseFullPersonName(x.Person.FirstName, x.Person.LastName),
                        AllocatedWorker = x.Worker == null ? null : $"{x.Worker.FirstName} {x.Worker.LastName }",
                        AllocatedWorkerTeam = x.Team.Name,
                        WorkerType = x.Worker.Role,
                        AllocationStartDate = x.AllocationStartDate,
                        AllocationEndDate = x.AllocationEndDate,
                        CaseStatus = x.CaseStatus,
                        PersonAddress = x.Person.Addresses.FirstOrDefault(x => !string.IsNullOrEmpty(x.IsDisplayAddress) && x.IsDisplayAddress.ToUpper() == "Y") == null ? null : x.Person.Addresses.FirstOrDefault(x => x.IsDisplayAddress.ToUpper() == "Y").AddressLines
                    }
                    ).AsNoTracking().ToList();
            }

            return allocations;
        }

        public List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null,
            string lastname = null, string dateOfBirth = null, string mosaicid = null, string agegroup = null)
        {
            var peopleIds = PeopleIds(cursor, limit, firstname, lastname, dateOfBirth, mosaicid, agegroup);

            var dbRecords = _databaseContext.Persons
                .Where(p => peopleIds.Contains(p.Id))
                .Select(p => new
                {
                    Person = p,
                    Addresses = _databaseContext
                        .Addresses
                        .Where(add => add.PersonId == p.Id)
                        .ToList(),
                }).ToList();

            return dbRecords.Select(x => MapPersonAndAddressesToResidentInformation(x.Person, x.Addresses)
            ).ToList();
        }

        private List<long> PeopleIds(int cursor, int limit, string firstname, string lastname,
            string dateOfBirth = null, string mosaicid = null, string agegroup = null)
        {
            var firstNameSearchPattern = GetSearchPattern(firstname);
            var lastNameSearchPattern = GetSearchPattern(lastname);
            var dateOfBirthSearchPattern = GetSearchPattern(dateOfBirth);
            var mosaicIdSearchPattern = GetSearchPattern(mosaicid);
            var ageGroupSearchPattern = GetSearchPattern(agegroup);
            return _databaseContext.Persons
                .Where(person => person.Id > cursor)
                .Where(person =>
                    string.IsNullOrEmpty(firstname) || EF.Functions.ILike(person.FirstName, firstNameSearchPattern))
                .Where(person =>
                    string.IsNullOrEmpty(lastname) || EF.Functions.ILike(person.LastName, lastNameSearchPattern))
                .Where(person =>
                    string.IsNullOrEmpty(dateOfBirth) || EF.Functions.ILike(person.DateOfBirth.ToString(), dateOfBirthSearchPattern))
                .Where(person =>
                    string.IsNullOrEmpty(mosaicid) || EF.Functions.ILike(person.Id.ToString(), mosaicIdSearchPattern))
                .Where(person =>
                    string.IsNullOrEmpty(agegroup) || EF.Functions.ILike(person.AgeContext, ageGroupSearchPattern))
                .Take(limit)
                .Select(p => p.Id)
                .ToList();
        }

        private static ResidentInformation MapPersonAndAddressesToResidentInformation(Person person,
            IEnumerable<Address> addresses)
        {
            var resident = person.ToDomain();
            var addressesDomain = addresses.Select(address => address.ToDomain()).ToList();
            resident.AddressList = addressesDomain;
            resident.AddressList = addressesDomain.Any()
                ? addressesDomain
                : null;
            return resident;
        }

        private static string GetSearchPattern(string str)
        {
            return $"%{str?.Replace(" ", "")}%";
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Include case note creation error as a message to the response until this is refactored to new pattern")]
        //Handle case note creation exception like below for now
        public AddNewResidentResponse AddNewResident(AddNewResidentRequest request)
        {
            Address address = null;
            List<PersonOtherName> names = null;
            Person resident;
            List<dbPhoneNumber> phoneNumbers = null;

            try
            {
                resident = AddNewPerson(request);

                if (request.Address != null)
                {
                    address = AddResidentAddress(request.Address, resident.Id, request.CreatedBy);
                    resident.Addresses = new List<Address>
                    {
                        address
                    };
                }

                if (request.OtherNames?.Count > 0)
                {
                    names = AddOtherNames(request.OtherNames, resident.Id, request.CreatedBy);
                    resident.OtherNames = new List<PersonOtherName>();
                    resident.OtherNames.AddRange(names);
                }
                if (request.PhoneNumbers?.Count > 0)
                {
                    phoneNumbers = AddPhoneNumbers(request.PhoneNumbers, resident.Id, request.CreatedBy);
                    resident.PhoneNumbers = new List<dbPhoneNumber>();
                    resident.PhoneNumbers.AddRange(phoneNumbers);
                }

                _databaseContext.Persons.Add(resident);
                _databaseContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new ResidentCouldNotBeinsertedException($"Error with inserting resident record has occurred - {ex.Message}");
            }
            string caseNoteId = null;
            string caseNoteErrorMessage = null;

            //Add note
            try
            {
                DateTime dt = DateTime.Now;

                CreatePersonCaseNote note = new CreatePersonCaseNote()
                {
                    FirstName = resident.FirstName,
                    LastName = resident.LastName,
                    MosaicId = resident.Id.ToString(),
                    Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"), //in line with imported form data
                    WorkerEmail = request.CreatedBy,
                    Note = $"{dt.ToShortDateString()} Person added - by {request.CreatedBy}.",
                    FormNameOverall = "API_Create_Person",
                    FormName = "Person added",
                    CreatedBy = request.CreatedBy
                };

                CaseNotesDocument caseNotesDocument = new CaseNotesDocument()
                {
                    CaseFormData = JsonConvert.SerializeObject(note)
                };

                //TODO: refactor to appropriate pattern when using base API

                caseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
            }
            catch (Exception ex)
            {
                caseNoteErrorMessage = $"Unable to create a case note for creating a person {resident.Id}: {ex.Message}";
            }

            return resident.ToResponse(address, names, phoneNumbers, caseNoteId, caseNoteErrorMessage);
        }

        public static Person AddNewPerson(AddNewResidentRequest request)
        {
            return new Person
            {
                Title = request.Title,
                FirstName = request.FirstName,
                LastName = request.LastName,
                //othernames
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                DateOfDeath = request.DateOfDeath,
                Ethnicity = request.Ethnicity,
                FirstLanguage = request.FirstLanguage,
                Religion = request.Religion,
                SexualOrientation = request.SexualOrientation,
                NhsNumber = request.NhsNumber,
                //address
                //phonenumbers
                EmailAddress = request.EmailAddress,
                PreferredMethodOfContact = request.PreferredMethodOfContact,
                AgeContext = request.ContextFlag,

                //calculated and additional values
                FullName = $"{request.FirstName} {request.LastName}",
                DataIsFromDmPersonsBackup = "N",
                CreatedBy = request.CreatedBy
            };
        }

        public static Address AddResidentAddress(AddressDomain addressRequest, long personId, string createdBy)
        {
            return new Address
            {
                AddressLines = addressRequest.Address,
                PersonId = personId,
                PostCode = addressRequest.Postcode,
                Uprn = addressRequest.Uprn,
                DataIsFromDmPersonsBackup = "N",
                IsDisplayAddress = "Y",
                CreatedBy = createdBy
            };
        }

        public static List<PersonOtherName> AddOtherNames(List<OtherName> names, long personId, string createdBy)
        {
            return names.Where(x => x.FirstName != null || x.LastName != null).Select(x => x.ToEntity(personId, createdBy)).ToList();
        }

        public static List<dbPhoneNumber> AddPhoneNumbers(List<PhoneNumber> numbers, long personId, string createdBy)
        {
            return numbers.Select(x => x.ToEntity(personId, createdBy)).ToList();
        }

        public string GetPersonIdByNCReference(string ncId)
        {
            PersonIdLookup lookup = _databaseContext.PersonLookups.Where(x => x.NCId == ncId).FirstOrDefault();

            return lookup?.MosaicId;
        }

        public string GetNCReferenceByPersonId(string personId)
        {
            PersonIdLookup lookup = _databaseContext.PersonLookups.Where(x => x.MosaicId == personId).FirstOrDefault();

            return lookup?.NCId;
        }

        public Worker GetWorkerByWorkerId(int workerId)
        {
            return _databaseContext.Workers
                .Where(x => x.Id == workerId)
                .Include(x => x.Allocations)
                .Include(x => x.WorkerTeams)
                .ThenInclude(y => y.Team)
                .FirstOrDefault();
        }

        public Worker GetWorkerByEmail(string email)
        {
            return _databaseContext.Workers
                .Where(worker => worker.Email == email)
                .Include(x => x.Allocations)
                .Include(x => x.WorkerTeams)
                .ThenInclude(y => y.Team)
                .FirstOrDefault();
        }

        public List<Team> GetTeamsByTeamId(int teamId)
        {
            return _databaseContext.Teams
                .Where(x => x.Id == teamId)
                .Include(x => x.WorkerTeams)
                .ThenInclude(x => x.Worker)
                .ThenInclude(x => x.Allocations)
                .ToList();
        }

        //TODO: use db views or queries
        public List<dynamic> GetWorkerAllocations(List<Worker> workers)
        {
            List<dynamic> allocationsPerWorker = new List<dynamic>();

            foreach (var worker in workers)
            {
                allocationsPerWorker.Add(new { WorkerId = worker.Id, AllocationCount = _databaseContext.Allocations.Where(x => x.WorkerId == worker.Id).Count() });
            }

            return allocationsPerWorker;
        }

        public List<Team> GetTeams(string context)
        {
            return (context.ToUpper()) switch
            {
                "B" => _databaseContext.Teams.ToList(),
                _ => _databaseContext.Teams.Where(x => x.Context.ToUpper() == context.ToUpper()).ToList(),
            };
        }

        public CreateAllocationResponse CreateAllocation(CreateAllocationRequest request)
        {
            var (worker, team, person, allocatedBy) = GetCreateAllocationRequirements(request);

            var allocation = new AllocationSet
            {
                PersonId = person.Id,
                WorkerId = worker.Id,
                TeamId = team.Id,
                AllocationStartDate = request.AllocationStartDate ?? DateTime.Now,
                CaseStatus = "Open",
                CreatedBy = allocatedBy.Email
            };

            _databaseContext.Allocations.Add(allocation);
            _databaseContext.SaveChanges();


            var response = new CreateAllocationResponse();
            //Add note
            try
            {
                var dt = DateTime.Now;

                var note = new AllocationCaseNote
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    MosaicId = person.Id.ToString(),
                    Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"), //in line with imported form data
                    WorkerEmail = allocatedBy.Email,
                    Note = $"{dt.ToShortDateString()} | Allocation | {worker.FirstName} {worker.LastName} in {team.Name} was allocated to this person (by {allocatedBy.FirstName} {allocatedBy.LastName})",
                    FormNameOverall = "API_Allocation",
                    FormName = "Worker allocated",
                    AllocationId = allocation.Id.ToString(),
                    CreatedBy = request.CreatedBy
                };

                var caseNotesDocument = new CaseNotesDocument()
                {
                    CaseFormData = JsonConvert.SerializeObject(note)
                };

                response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
                response.AllocationId = allocation.Id;
            }
            catch (Exception ex)
            {
                //roll back allocation record
                _databaseContext.Allocations.Remove(allocation);
                _databaseContext.SaveChanges();

                throw new UpdateAllocationException($"Unable to create a case note. Allocation not created: {ex.Message}");
            }

            return response;
        }

        public UpdateAllocationResponse UpdateAllocation(UpdateAllocationRequest request)
        {
            var response = new UpdateAllocationResponse();

            try
            {
                var allocation = _databaseContext.Allocations.FirstOrDefault(x => x.Id == request.Id);

                if (allocation == null)
                {
                    throw new EntityUpdateException($"Allocation {request.Id} not found");
                }

                if (allocation.CaseStatus?.ToUpper() == "CLOSED")
                {
                    throw new UpdateAllocationException("Allocation already closed");
                }

                var (person, createdBy) = GetUpdateAllocationRequirements(allocation, request);


                //copy existing values in case adding note fails
                var tmpAllocation = (AllocationSet) allocation.Clone();
                SetDeallocationValues(allocation, request.DeallocationDate, request.CreatedBy);
                _databaseContext.SaveChanges();

                try
                {
                    var note = new DeallocationCaseNote
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        MosaicId = person.Id.ToString(),
                        Timestamp = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss"),
                        WorkerEmail = createdBy.Email, //required for my cases search
                        DeallocationReason = request.DeallocationReason,
                        FormNameOverall = "API_Deallocation", //prefix API notes so they are easy to identify
                        FormName = "Worker deallocated",
                        AllocationId = request.Id.ToString(),
                        CreatedBy = request.CreatedBy
                    };

                    var caseNotesDocument = new CaseNotesDocument()
                    {
                        CaseFormData = JsonConvert.SerializeObject(note)
                    };

                    response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
                }
                catch (Exception ex)
                {
                    var allocationToRestore = _databaseContext.Allocations.FirstOrDefault(x => x.Id == request.Id);
                    RestoreAllocationValues(tmpAllocation, allocationToRestore);

                    _databaseContext.SaveChanges();

                    throw new UpdateAllocationException($"Unable to create a case note. Allocation not updated: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new EntityUpdateException($"Unable to update allocation {request.Id}: {ex.Message}");
            }

            return response;
        }

        #region Warning Notes
        public CreateWarningNoteResponse CreateWarningNote(CreateWarningNoteRequest request)
        {
            Person person = _databaseContext.Persons.FirstOrDefault(x => x.Id == request.PersonId);

            if (person == null)
            {
                throw new CreateWarningNoteException($"Person with given id ({request.PersonId}) not found");
            }

            //TODO: Extract request to domain process to EntityFactory
            WarningNote warningNote = new WarningNote()
            {
                PersonId = request.PersonId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IndividualNotified = request.IndividualNotified,
                NotificationDetails = request.NotificationDetails,
                ReviewDetails = request.ReviewDetails,
                NoteType = request.NoteType,
                Status = request.Status,
                DateInformed = request.DateInformed,
                HowInformed = request.HowInformed,
                WarningNarrative = request.WarningNarrative,
                ManagersName = request.ManagersName,
                DateManagerInformed = request.DateManagerInformed,
                CreatedBy = request.CreatedBy
            };

            _databaseContext.WarningNotes.Add(warningNote);
            _databaseContext.SaveChanges();

            CreateWarningNoteResponse response = new CreateWarningNoteResponse()
            {
                WarningNoteId = warningNote.Id
            };

            // try
            // {
            DateTime dt = DateTime.Now;

            WarningNoteCaseNote note = new WarningNoteCaseNote()
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                MosaicId = person.Id.ToString(),
                Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"),
                Note = $"{dt.ToShortDateString()} | Warning Note | Warning note created against this person",
                FormNameOverall = "API_WarningNote",
                FormName = "Warning Note Created",
                WarningNoteId = warningNote.Id.ToString()
            };

            CaseNotesDocument caseNotesDocument = new CaseNotesDocument()
            {
                CaseFormData = JsonConvert.SerializeObject(note)
            };

            response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
            // }
            // catch (Exception ex)
            // {
            //     _databaseContext.WarningNotes.Remove(warningNote);
            //     _databaseContext.SaveChanges();

            //     throw new CreateWarningNoteException($"Unable to create a case note. Warning Note not created: {ex.Message}");
            // }

            return response;
        }

        public IEnumerable<WarningNote> GetWarningNotes(GetWarningNoteRequest request)
        {
            var warningNotes = _databaseContext.WarningNotes
                .Where(x => x.PersonId == request.PersonId);

            if (warningNotes.FirstOrDefault() == null) throw new DocumentNotFoundException($"No warning notes found relating to person id {request.PersonId}");

            return warningNotes;
        }

        #endregion

        private static AllocationSet SetDeallocationValues(AllocationSet allocation, DateTime dt, string modifiedBy)
        {
            //keep workerId and TeamId in the record so they can be easily exposed to front end
            allocation.AllocationEndDate = dt;
            allocation.CaseStatus = "Closed";
            allocation.CaseClosureDate = dt;
            allocation.LastModifiedBy = modifiedBy;
            return allocation;
        }

        private static void RestoreAllocationValues(AllocationSet tmpAllocation, AllocationSet allocationToRestore)
        {
            allocationToRestore.AllocationEndDate = tmpAllocation.AllocationEndDate;
            allocationToRestore.CaseStatus = tmpAllocation.CaseStatus;
            allocationToRestore.WorkerId = tmpAllocation.WorkerId;
            allocationToRestore.TeamId = tmpAllocation.TeamId;
            allocationToRestore.CaseClosureDate = tmpAllocation.CaseClosureDate;
        }

        private static string ToTitleCaseFullPersonName(string firstName, string lastName)
        {
            string first = string.IsNullOrWhiteSpace(firstName) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstName.ToLower());
            string last = string.IsNullOrWhiteSpace(lastName) ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName.ToLower());

            return (first + " " + last).TrimStart().TrimEnd();
        }

        private (Worker, Team, Person, Worker) GetCreateAllocationRequirements(CreateAllocationRequest request)
        {
            var worker = _databaseContext.Workers.FirstOrDefault(x => x.Id == request.AllocatedWorkerId);
            if (string.IsNullOrEmpty(worker?.Email))
            {
                throw new CreateAllocationException("Worker details cannot be found");
            }

            var team = _databaseContext.Teams.FirstOrDefault(x => x.Id == request.AllocatedTeamId);
            if (team == null)
            {
                throw new CreateAllocationException("Team details cannot be found");
            }

            var person = _databaseContext.Persons.FirstOrDefault(x => x.Id == request.MosaicId);
            if (person == null)
            {
                throw new CreateAllocationException($"Person with given id ({request.MosaicId}) not found");
            }

            var allocatedBy = _databaseContext.Workers.FirstOrDefault(x => x.Email.ToUpper().Equals(request.CreatedBy.ToUpper()));
            if (allocatedBy == null)
            {
                throw new CreateAllocationException($"Worker with given allocated by email address ({request.CreatedBy}) not found");
            }

            return (worker, team, person, allocatedBy);
        }

        private (Person, Worker) GetUpdateAllocationRequirements(AllocationSet allocation, UpdateAllocationRequest request)
        {
            if (allocation.CaseStatus?.ToUpper() == "CLOSED")
            {
                throw new UpdateAllocationException("Allocation already closed");
            }

            var worker = _databaseContext.Workers.FirstOrDefault(x => x.Id == allocation.WorkerId);
            if (worker == null)
            {
                throw new UpdateAllocationException("Worker not found");
            }

            var person = _databaseContext.Persons.FirstOrDefault(x => x.Id == allocation.PersonId);
            if (person == null)
            {
                throw new UpdateAllocationException("Person not found");
            }

            var createdBy = _databaseContext.Workers.FirstOrDefault(x =>
                string.Equals(x.Email.ToUpper(), request.CreatedBy.ToUpper(), StringComparison.Ordinal));
            if (createdBy == null)
            {
                throw new UpdateAllocationException("CreatedBy not found");
            }

            return (person, createdBy);
        }
    }
}
