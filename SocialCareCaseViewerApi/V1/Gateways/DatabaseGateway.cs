using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using ResidentInformationResponse = SocialCareCaseViewerApi.V1.Boundary.Response.ResidentInformation;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class DatabaseGateway : IDatabaseGateway
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IProcessDataGateway _processDataGateway;
        private readonly ISystemTime _systemTime;

        public DatabaseGateway(DatabaseContext databaseContext, IProcessDataGateway processDataGateway, ISystemTime systemTime)
        {
            _databaseContext = databaseContext;
            _processDataGateway = processDataGateway;
            _systemTime = systemTime;
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
                        AllocatedWorker = x.Worker == null ? null : $"{x.Worker.FirstName} {x.Worker.LastName}",
                        AllocatedWorkerTeam = x.Team.Name,
                        WorkerType = x.Worker.Role,
                        AllocationStartDate = x.AllocationStartDate,
                        AllocationEndDate = x.AllocationEndDate,
                        CaseStatus = x.CaseStatus,
                        PersonAddress =
                                x.Person.Addresses.FirstOrDefault(x =>
                                    !string.IsNullOrEmpty(x.IsDisplayAddress) && x.IsDisplayAddress.ToUpper() == "Y") ==
                                null
                                    ? null
                                    : x.Person.Addresses.FirstOrDefault(x => x.IsDisplayAddress.ToUpper() == "Y")
                                        .AddressLines
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
                        AllocatedWorker = x.Worker == null ? null : $"{x.Worker.FirstName} {x.Worker.LastName}",
                        AllocatedWorkerTeam = x.Team.Name,
                        WorkerType = x.Worker.Role,
                        AllocationStartDate = x.AllocationStartDate,
                        AllocationEndDate = x.AllocationEndDate,
                        CaseStatus = x.CaseStatus,
                        PersonAddress =
                                x.Person.Addresses.FirstOrDefault(x =>
                                    !string.IsNullOrEmpty(x.IsDisplayAddress) && x.IsDisplayAddress.ToUpper() == "Y") ==
                                null
                                    ? null
                                    : x.Person.Addresses.FirstOrDefault(x => x.IsDisplayAddress.ToUpper() == "Y")
                                        .AddressLines
                    }
                    ).AsNoTracking().ToList();
            }

            return allocations;
        }

        public List<ResidentInformationResponse> GetResidentsBySearchCriteria(int cursor, int limit, long? id = null, string firstname = null,
          string lastname = null, string dateOfBirth = null, string postcode = null, string address = null, string contextflag = null)
        {
            var addressSearchPattern = GetSearchPattern(address);
            var postcodeSearchPattern = GetSearchPattern(postcode);

            var queryByAddress = postcode != null || address != null;

            var peopleIds = queryByAddress
                ? GetPersonIdsBasedOnAddressCriteria(cursor, limit, id, firstname, lastname, postcode, address, contextflag)
                : GetPersonIdsBasedOnSearchCriteria(cursor, limit, id, firstname, lastname, dateOfBirth, contextflag);

            var var = _databaseContext.Persons
                .Where(p => peopleIds.Contains(p.Id))
                .Include(p => p.Addresses)
                .Include(p => p.PhoneNumbers);

            var dbRecords = _databaseContext.Persons
                .Where(p => peopleIds.Contains(p.Id) && p.MarkedForDeletion == false)
                .Include(p => p.Addresses)
                .Include(p => p.PhoneNumbers)
                .Select(x => x.ToResidentInformationResponse()).ToList();

            return dbRecords;
        }

        private List<long> GetPersonIdsBasedOnSearchCriteria(int cursor, int limit, long? id, string firstname, string lastname, string dateOfBirth, string contextflag)
        {
            var firstNameSearchPattern = GetSearchPattern(firstname);
            var lastNameSearchPattern = GetSearchPattern(lastname);
            var dateOfBirthSearchPattern = GetSearchPattern(dateOfBirth);
            var contextFlagSearchPattern = GetSearchPattern(contextflag);

            return _databaseContext.Persons
                .Where(person => person.Id > cursor)
                .Where(person => id == null || EF.Functions.ILike(person.Id.ToString(), id.ToString()))
                .Where(person => string.IsNullOrEmpty(firstname) || EF.Functions.ILike(person.FirstName.Replace(" ", ""), firstNameSearchPattern))
                .Where(person => string.IsNullOrEmpty(lastname) || EF.Functions.ILike(person.LastName, lastNameSearchPattern))
                .Where(person => string.IsNullOrEmpty(dateOfBirth) || EF.Functions.ILike(person.DateOfBirth.ToString(), dateOfBirthSearchPattern))
                .Where(person => string.IsNullOrEmpty(contextflag) || EF.Functions.ILike(person.AgeContext, contextFlagSearchPattern))
                .OrderBy(p => p.Id)
                .Take(limit)
                .Select(p => p.Id)
                .ToList();
        }

        private List<long> GetPersonIdsBasedOnAddressCriteria(int cursor, int limit, long? id, string firstname, string lastname,
           string postcode, string address, string contextflag)
        {
            var firstNameSearchPattern = GetSearchPattern(firstname);
            var lastNameSearchPattern = GetSearchPattern(lastname);
            var addressSearchPattern = GetSearchPattern(address);
            var postcodeSearchPattern = GetSearchPattern(postcode);
            var contextFlagSearchPattern = GetSearchPattern(contextflag);

            return _databaseContext.Addresses
                .Where(add => id == null || EF.Functions.ILike(add.PersonId.ToString(), id.ToString()))
                .Where(add => string.IsNullOrEmpty(address) || EF.Functions.ILike(add.AddressLines.Replace(" ", ""), addressSearchPattern))
                .Where(add => string.IsNullOrEmpty(postcode) || EF.Functions.ILike(add.PostCode.Replace(" ", ""), postcodeSearchPattern) && add.IsDisplayAddress == "Y")
                .Where(add => string.IsNullOrEmpty(firstname) || EF.Functions.ILike(add.Person.FirstName.Replace(" ", ""), firstNameSearchPattern))
                .Where(add => string.IsNullOrEmpty(lastname) || EF.Functions.ILike(add.Person.LastName, lastNameSearchPattern))
                .Where(add => string.IsNullOrEmpty(contextflag) || EF.Functions.ILike(add.Person.AgeContext, contextFlagSearchPattern))
                .Include(add => add.Person)
                .Where(add => add.PersonId > cursor)
                .OrderBy(add => add.PersonId)
                .GroupBy(add => add.PersonId)
                .Where(p => p.Key != null)
                .Take(limit)
                .Select(p => (long) p.Key)
                .ToList();
        }

        private static string GetSearchPattern(string str)
        {
            return $"%{str?.Replace(" ", "")}%";
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types",
            Justification =
                "Include case note creation error as a message to the response until this is refactored to new pattern")]
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
                    resident.Addresses = new List<Address> { address };
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
                throw new ResidentCouldNotBeinsertedException(
                    $"Error with inserting resident record has occurred - {ex.Message}");
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
                caseNoteErrorMessage =
                    $"Unable to create a case note for creating a person {resident.Id}: {ex.Message}";
            }

            return resident.ToResponse(address, names, phoneNumbers, caseNoteId, caseNoteErrorMessage);
        }

        public void UpdatePerson(UpdatePersonRequest request)
        {
            Person person = _databaseContext
                .Persons
                .Include(x => x.Addresses)
                .Include(x => x.PhoneNumbers)
                .Include(x => x.OtherNames)
                .FirstOrDefault(x => x.Id == request.Id);

            if (person == null)
            {
                throw new UpdatePersonException("Person not found");
            }

            person.AgeContext = request.ContextFlag;
            person.DateOfBirth = request.DateOfBirth;
            person.DateOfDeath = request.DateOfDeath;
            person.EmailAddress = request.EmailAddress;
            person.Ethnicity = request.Ethnicity;
            person.FirstLanguage = request.FirstLanguage;
            person.FirstName = request.FirstName;
            person.FullName = $"{request.FirstName} {request.LastName}";
            person.Gender = request.Gender;
            person.LastModifiedBy = request.CreatedBy;
            person.LastName = request.LastName;
            person.NhsNumber = request.NhsNumber;
            person.PreferredMethodOfContact = request.PreferredMethodOfContact;
            person.Religion = request.Religion;
            person.Restricted = request.Restricted;
            person.SexualOrientation = request.SexualOrientation;
            person.Title = request.Title;

            //replace phone numbers
            _databaseContext.PhoneNumbers.RemoveRange(person.PhoneNumbers);

            if (request.PhoneNumbers != null)
            {
                foreach (var number in request.PhoneNumbers)
                {
                    person.PhoneNumbers.Add(number.ToEntity(person.Id, request.CreatedBy));
                }
            }

            //replace other names
            _databaseContext.PersonOtherNames.RemoveRange(person.OtherNames);

            if (request.OtherNames != null)
            {
                foreach (var otherName in request.OtherNames)
                {
                    person.OtherNames.Add(otherName.ToEntity(person.Id, request.CreatedBy));
                }
            }

            //check for changed address
            if (request.Address != null)
            {
                Address displayAddress = person.Addresses.FirstOrDefault(x => x.IsDisplayAddress == "Y");

                if (displayAddress == null)
                {
                    person.Addresses.Add(GetNewDisplayAddress(request.Address.Address, request.Address.Postcode,
                        request.Address.Uprn, request.CreatedBy));
                }
                else
                {
                    //has address changed?
                    if (!(request.Address.Address == displayAddress.AddressLines
                          && request.Address.Postcode == displayAddress.PostCode
                          && displayAddress.Uprn == request.Address.Uprn))
                    {
                        displayAddress.IsDisplayAddress = "N";
                        displayAddress.EndDate = DateTime.Now;
                        displayAddress.LastModifiedBy = request.CreatedBy;

                        person.Addresses.Add(GetNewDisplayAddress(request.Address.Address, request.Address.Postcode,
                            request.Address.Uprn, request.CreatedBy));
                    }
                }
            }
            else //address not provided, remove current display address if it exists
            {
                Address displayAddress = person.Addresses.FirstOrDefault(x => x.IsDisplayAddress == "Y");

                if (displayAddress != null)
                {
                    displayAddress.IsDisplayAddress = "N";
                    displayAddress.EndDate = DateTime.Now;
                    displayAddress.LastModifiedBy = request.CreatedBy;
                }
            }

            DateTime dt = DateTime.Now;

            UpdatePersonCaseNote note = new UpdatePersonCaseNote()
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                MosaicId = person.Id.ToString(),
                Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"), //in line with imported form data
                WorkerEmail = request.CreatedBy,
                Note = $"{dt.ToShortDateString()} Person details updated - by {request.CreatedBy}.",
                FormNameOverall = "API_Update_Person",
                FormName = "Person updated",
                CreatedBy = request.CreatedBy
            };

            CaseNotesDocument caseNotesDocument = new CaseNotesDocument()
            {
                CaseFormData = JsonConvert.SerializeObject(note)
            };

            //TODO: refactor so gateways don't call each other
            _ = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;

            _databaseContext.SaveChanges();
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
                CreatedBy = request.CreatedBy,
                Restricted = request.Restricted
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
            return names.Where(x => x.FirstName != null || x.LastName != null)
                .Select(x => x.ToEntity(personId, createdBy)).ToList();
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

        public Person GetPersonByMosaicId(long mosaicId)
        {
            return _databaseContext.Persons.FirstOrDefault(x => x.Id == mosaicId && x.MarkedForDeletion == false);
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
                    .Where(worker => worker.Email.ToLower() == email.ToLower())
                    .Include(x => x.Allocations)
                    .Include(x => x.WorkerTeams)
                    .ThenInclude(y => y.Team)
                    .FirstOrDefault();
        }

        public Worker CreateWorker(CreateWorkerRequest createWorkerRequest)
        {
            if (GetWorkerByEmail(createWorkerRequest.EmailAddress) != null)
            {
                throw new PostWorkerException($"Worker with Email {createWorkerRequest.EmailAddress} already exists");
            }

            var worker = new Worker
            {
                Role = createWorkerRequest.Role,
                Email = createWorkerRequest.EmailAddress,
                FirstName = createWorkerRequest.FirstName,
                LastName = createWorkerRequest.LastName,
                IsActive = true,
                CreatedBy = createWorkerRequest.CreatedBy,
                ContextFlag = createWorkerRequest.ContextFlag,
                DateStart = createWorkerRequest.DateStart,
                DateEnd = null,
                LastModifiedBy = createWorkerRequest.CreatedBy
            };

            var workerTeams = GetTeams(createWorkerRequest.Teams);

            worker.WorkerTeams = new List<WorkerTeam>();
            foreach (var team in workerTeams)
            {
                worker.WorkerTeams.Add(new WorkerTeam { Team = team, Worker = worker });
            }

            _databaseContext.Workers.Add(worker);
            _databaseContext.SaveChanges();
            return worker;
        }

        public void UpdateWorker(UpdateWorkerRequest request)
        {
            var worker = _databaseContext.Workers.FirstOrDefault(x => x.Id == request.WorkerId);

            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with Id {request.WorkerId} not found");
            }

            worker.LastModifiedBy = request.ModifiedBy;
            worker.FirstName = request.FirstName;
            worker.LastName = request.LastName;
            worker.ContextFlag = request.ContextFlag;
            worker.Role = request.Role;
            worker.DateStart = request.DateStart;
            worker.IsActive = true;

            var workerTeams = GetTeams(request.Teams);

            worker.WorkerTeams = workerTeams.Select(t => new WorkerTeam { Team = t, Worker = worker }).ToList();
            _databaseContext.SaveChanges();

            // Update any assigned allocations to reflect the worker's new team
            var allocations = _databaseContext.Allocations.Where(x => x.WorkerId == request.WorkerId).ToList();

            if (!allocations.Any()) return;

            var updatedTeam = _databaseContext.WorkerTeams.FirstOrDefault(x => x.WorkerId.Equals(worker.Id))?.Team;

            foreach (var allocation in allocations)
            {
                allocation.TeamId = updatedTeam?.Id;
                allocation.Team = updatedTeam;
                _databaseContext.SaveChanges();
            }
        }

        private ICollection<Team> GetTeams(List<WorkerTeamRequest> request)
        {
            var teamsWorkerBelongsIn = new List<Team>();
            foreach (var requestTeam in request)
            {
                var team = GetTeamByTeamId(requestTeam.Id);
                if (team == null)
                {
                    throw new GetTeamException($"Team with Name {requestTeam.Name} and ID {requestTeam.Id} not found");
                }

                teamsWorkerBelongsIn.Add(team);
            }

            return teamsWorkerBelongsIn;
        }

        public Team CreateTeam(CreateTeamRequest request)
        {
            var team = new Team { Name = request.Name, Context = request.Context, WorkerTeams = new List<WorkerTeam>() };

            _databaseContext.Teams.Add(team);
            _databaseContext.SaveChanges();

            return team;
        }

        public Team GetTeamByTeamId(int teamId)
        {
            return _databaseContext.Teams
                .Where(x => x.Id == teamId)
                .Include(x => x.WorkerTeams)
                .ThenInclude(x => x.Worker)
                .ThenInclude(x => x.Allocations)
                .FirstOrDefault();
        }

        public Team GetTeamByTeamName(string teamName)
        {
            return _databaseContext.Teams
                .Where(x => x.Name.ToUpper().Equals(teamName.ToUpper()))
                .Include(x => x.WorkerTeams)
                .ThenInclude(x => x.Worker)
                .ThenInclude(x => x.Allocations)
                .FirstOrDefault();
        }

        public IEnumerable<Team> GetTeamsByTeamContextFlag(string context)
        {
            return _databaseContext.Teams.Where(x => x.Context.ToUpper().Equals(context.ToUpper()));
        }

        //TODO: use db views or queries
        public List<dynamic> GetWorkerAllocations(List<Worker> workers)
        {
            List<dynamic> allocationsPerWorker = new List<dynamic>();

            foreach (var worker in workers)
            {
                allocationsPerWorker.Add(new
                {
                    WorkerId = worker.Id,
                    AllocationCount = _databaseContext.Allocations.Where(x => x.WorkerId == worker.Id).Count()
                });
            }

            return allocationsPerWorker;
        }

        public CreateAllocationResponse CreateAllocation(CreateAllocationRequest request)
        {
            var (worker, team, person, allocatedBy) = GetCreateAllocationRequirements(request);

            var allocation = new AllocationSet
            {
                PersonId = person.Id,
                WorkerId = worker.Id,
                TeamId = team.Id,
                AllocationStartDate = request.AllocationStartDate,
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
                    Note =
                        $"{dt.ToShortDateString()} | Allocation | {worker.FirstName} {worker.LastName} in {team.Name} was allocated to this person (by {allocatedBy.FirstName} {allocatedBy.LastName})",
                    FormNameOverall = "API_Allocation",
                    FormName = "Worker allocated",
                    AllocationId = allocation.Id.ToString(),
                    CreatedBy = request.CreatedBy
                };

                var caseNotesDocument = new CaseNotesDocument() { CaseFormData = JsonConvert.SerializeObject(note) };

                response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
                response.AllocationId = allocation.Id;
            }
            catch (Exception ex)
            {
                //roll back allocation record
                _databaseContext.Allocations.Remove(allocation);
                _databaseContext.SaveChanges();

                throw new UpdateAllocationException(
                    $"Unable to create a case note. Allocation not created: {ex.Message}");
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

                    var caseNotesDocument = new CaseNotesDocument() { CaseFormData = JsonConvert.SerializeObject(note) };

                    response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
                }
                catch (Exception ex)
                {
                    var allocationToRestore = _databaseContext.Allocations.FirstOrDefault(x => x.Id == request.Id);
                    RestoreAllocationValues(tmpAllocation, allocationToRestore);

                    _databaseContext.SaveChanges();

                    throw new UpdateAllocationException(
                        $"Unable to create a case note. Allocation not updated: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new EntityUpdateException($"Unable to update allocation {request.Id}: {ex.Message}");
            }

            return response;
        }

        #region Warning Notes

        public PostWarningNoteResponse PostWarningNote(PostWarningNoteRequest request)
        {
            var person = _databaseContext.Persons.FirstOrDefault(x => x.Id == request.PersonId);

            if (person == null)
            {
                throw new PersonNotFoundException($"Person with given id ({request.PersonId}) not found");
            }

            var warningNote = request.ToDatabaseEntity();

            _databaseContext.WarningNotes.Add(warningNote);
            _databaseContext.SaveChanges();

            var response = new PostWarningNoteResponse
            {
                WarningNoteId = warningNote.Id
            };

            // try
            // {
            var dt = DateTime.Now;

            var note = new WarningNoteCaseNote
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                MosaicId = person.Id.ToString(),
                Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"),
                Note = $"{dt.ToShortDateString()} | Warning Note | Warning note created against this person",
                FormNameOverall = "API_WarningNote",
                FormName = "Warning Note Created",
                WarningNoteId = warningNote.Id.ToString(),
                WorkerEmail = request.CreatedBy
            };

            var caseNotesDocument = new CaseNotesDocument
            {
                CaseFormData = JsonConvert.SerializeObject(note)
            };

            response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
            // }
            // catch (Exception ex)
            // {
            //     _databaseContext.WarningNotes.Remove(warningNote);
            //     _databaseContext.SaveChanges();

            //     throw new PostWarningNoteException($"Unable to create a case note. Warning Note not created: {ex.Message}");
            // }

            return response;
        }

        public IEnumerable<WarningNote> GetWarningNotes(long personId)
        {
            return _databaseContext.WarningNotes
                .Where(x => x.PersonId == personId);
        }

        public Domain.WarningNote GetWarningNoteById(long warningNoteId)
        {
            var warningNote = _databaseContext.WarningNotes.FirstOrDefault(x => x.Id == warningNoteId);

            var reviews = _databaseContext.WarningNoteReview
                .Where(x => x.WarningNoteId == warningNoteId).ToList();

            return warningNote?.ToDomain(reviews);
        }

        public void PatchWarningNote(PatchWarningNoteRequest request)
        {
            WarningNote warningNote = _databaseContext.WarningNotes.FirstOrDefault(x => x.Id == request.WarningNoteId);

            if (warningNote == null)
            {
                throw new PatchWarningNoteException($"Warning Note with given id ({request.WarningNoteId}) not found");
            }

            if (warningNote.Status == "closed")
            {
                throw new PatchWarningNoteException(
                    $"Warning Note with given id ({request.WarningNoteId}) has already been closed");
            }

            Person person = _databaseContext.Persons.FirstOrDefault(x => x.Id == warningNote.PersonId);
            if (person == null)
            {
                throw new PatchWarningNoteException("Person not found");
            }

            Worker worker = _databaseContext.Workers.FirstOrDefault(x => x.Email == request.ReviewedBy);
            if (worker == null)
            {
                throw new PatchWarningNoteException($"Worker ({request.ReviewedBy}) not found");
            }


            warningNote.LastReviewDate = request.ReviewDate;
            warningNote.NextReviewDate = request.NextReviewDate;
            if (request.Status?.ToLower() == "closed")
            {
                warningNote.Status = "closed";
                warningNote.EndDate = _systemTime.Now;
                warningNote.NextReviewDate = null;
            }

            warningNote.LastModifiedBy = request.ReviewedBy;

            var review = PostWarningNoteReview(request);
            _databaseContext.WarningNoteReview.Add(review);
            _databaseContext.SaveChanges();

            var dt = DateTime.Now;

            var note = new WarningNoteCaseNote
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                MosaicId = person.Id.ToString(),
                Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"),
                Note = $"{dt.ToShortDateString()} | Warning Note | {((request.Status == "closed") ? "Warning note against this person ended" : "Warning note against this person reviewed")}",
                FormNameOverall = "API_WarningNote",
                FormName = (request.Status == "closed") ? "Warning Note Ended" : "Warning Note Reviewed",
                WarningNoteId = warningNote.Id.ToString(),
                WorkerEmail = request.ReviewedBy
            };

            var caseNotesDocument = new CaseNotesDocument
            {
                CaseFormData = JsonConvert.SerializeObject(note)
            };

            _ = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
        }

        private static WarningNoteReview PostWarningNoteReview(PatchWarningNoteRequest request)
        {
            return new WarningNoteReview
            {
                WarningNoteId = request.WarningNoteId,
                ReviewDate = request.ReviewDate,
                ReviewNotes = request.ReviewNotes,
                DisclosedWithIndividual = request.DisclosedWithIndividual,
                ManagerName = request.ManagerName,
                DiscussedWithManagerDate = request.DiscussedWithManagerDate,
                CreatedBy = request.ReviewedBy,
                LastModifiedBy = request.ReviewedBy
            };
        }

        #endregion

        public Person GetPersonDetailsById(long id)
        {
            //load related entities to minimise SQL calls
            return _databaseContext
                .Persons
                .Include(x => x.Addresses)
                .Include(x => x.PhoneNumbers)
                .Include(x => x.OtherNames)
                .FirstOrDefault(x => x.Id == id && x.MarkedForDeletion == false);
        }
        public List<Person> GetPersonsByListOfIds(List<long> ids)
        {
            return _databaseContext.Persons.Where(x => ids.Contains(x.Id) && x.MarkedForDeletion == false).ToList();
        }

        public List<long> GetPersonIdsByEmergencyId(string id)
        {
            return _databaseContext
               .PersonLookups
               .AsNoTracking()
               .AsEnumerable()
               .Where(x => Regex.Replace(x.NCId, "[^0-9.]", "") == id)
               .Select(x => Convert.ToInt64(x.MosaicId)).ToList();
        }

        public Person GetPersonWithPersonalRelationshipsByPersonId(long personId, bool includeEndedRelationships = false)
        {
            var personWithRelationships = _databaseContext
                .Persons
                .Include(person => person.PersonalRelationships)
                .ThenInclude(personalRelationship => personalRelationship.Type)
                .Include(person => person.PersonalRelationships)
                .ThenInclude(personalRelationship => personalRelationship.OtherPerson)
                .Include(person => person.PersonalRelationships)
                .ThenInclude(personalRelationship => personalRelationship.Details)
                .FirstOrDefault(p => p.Id == personId);

            if (personWithRelationships == null) return null;

            if (includeEndedRelationships == false)
            {
                personWithRelationships.PersonalRelationships = personWithRelationships.PersonalRelationships.Where(pr => pr.EndDate == null).ToList();
            }

            personWithRelationships.PersonalRelationships = personWithRelationships.PersonalRelationships.Where(pr => pr.OtherPerson.MarkedForDeletion == false).ToList();

            return personWithRelationships;
        }

        public PersonalRelationshipType GetPersonalRelationshipTypeByDescription(string description)
        {
            return _databaseContext.PersonalRelationshipTypes
                .FirstOrDefault(prt => prt.Description.ToLower() == description.ToLower());
        }

        public Infrastructure.PersonalRelationship GetPersonalRelationshipById(long relationshipId)
        {
            return _databaseContext.PersonalRelationships
                .FirstOrDefault(prt => prt.Id == relationshipId);
        }

        public void DeleteRelationship(long id)
        {
            var relationship = _databaseContext.PersonalRelationships
                .Where(prt => prt.Id == id)
                .Include(pr => pr.Type)
                .Include(pr => pr.Details)
                .FirstOrDefault();

            var inverseRelationship = _databaseContext.PersonalRelationships
                .Where(pr => pr.PersonId == relationship.OtherPersonId && pr.TypeId == relationship.Type.InverseTypeId)
                .Include(pr => pr.Details)
                .FirstOrDefault();

            _databaseContext.PersonalRelationships.Remove(relationship);
            _databaseContext.PersonalRelationships.Remove(inverseRelationship);

            _databaseContext.SaveChanges();
        }

        public Infrastructure.PersonalRelationship CreatePersonalRelationship(CreatePersonalRelationshipRequest request)
        {
            var personalRelationship = new Infrastructure.PersonalRelationship()
            {
                PersonId = request.PersonId,
                OtherPersonId = request.OtherPersonId,
                TypeId = (long) request.TypeId,
                IsMainCarer = request.IsMainCarer?.ToUpper(),
                IsInformalCarer = request.IsInformalCarer?.ToUpper(),
                StartDate = _systemTime.Now,
                CreatedBy = request.CreatedBy,
                Details = new PersonalRelationshipDetail()
                {
                    Details = request.Details,
                    CreatedBy = request.CreatedBy
                }
            };

            _databaseContext.PersonalRelationships.Add(personalRelationship);
            _databaseContext.SaveChanges();

            return personalRelationship;
        }

        public void CreateRequestAudit(CreateRequestAuditRequest request)
        {
            var requestAudit = (new RequestAudit()
            {
                ActionName = request.ActionName,
                UserName = request.UserName,
                Metadata = request.Metadata,
                Timestamp = DateTime.Now
            });

            _databaseContext.RequestAudits.Add(requestAudit);
            _databaseContext.SaveChanges();
        }

        public IEnumerable<Infrastructure.CaseStatus> GetCaseStatusesByPersonId(long personId)
        {
            var caseStatuses = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.EndDate == null || cs.EndDate > DateTime.Today)
                .Include(cs => cs.Type)
                .Include(cs => cs.SelectedOptions)
                .ThenInclude(csso => csso.FieldOption)
                .ThenInclude(fo => fo.TypeField);

            return caseStatuses;
        }
        public Infrastructure.CaseStatus GetCaseStatusesByPersonIdDate(long personId, DateTime date)
        {
            var caseStatuse = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.StartDate <= date)
                .Where(cs => cs.EndDate == null || cs.EndDate >= date)
                .Include(cs => cs.Type)
                .Include(cs => cs.SelectedOptions)
                .ThenInclude(csso => csso.FieldOption)
                .ThenInclude(fo => fo.TypeField)
                .FirstOrDefault();

            return caseStatuse;
        }

        public Infrastructure.CaseStatusType GetCaseStatusTypeWithFields(string type)
        {
            var response = _databaseContext.CaseStatusTypes
                .Where(cs => cs.Name == type)
                .Include(cs => cs.Fields)
                .ThenInclude(sf => sf.Options);

            return response.FirstOrDefault();
        }

        public Infrastructure.CaseStatus CreateCaseStatus(CreateCaseStatusRequest request)
        {

            var statusType = _databaseContext.CaseStatusTypes
                .Where(f => f.Name == request.Type)
                .FirstOrDefault();

            var caseStatus = new Infrastructure.CaseStatus()
            {
                PersonId = request.PersonId,
                TypeId = statusType.Id,
                StartDate = request.StartDate,
                Notes = request.Notes,
                CreatedBy = request.CreatedBy
            };

            _databaseContext.CaseStatuses.Add(caseStatus);

            if (request.Fields != null)
            {
                foreach (var optionValue in request.Fields)
                {
                    var field = _databaseContext.CaseStatusTypeFields
                        .FirstOrDefault(f => f.Name == optionValue.Name);

                    var fieldTypeOption = _databaseContext.CaseStatusTypeFieldOptions
                        .Where(fov => fov.Name == optionValue.Selected)
                        .FirstOrDefault(fov => fov.TypeFieldId == field.Id);

                    if (fieldTypeOption != null)
                    {
                        var fieldOption = new CaseStatusFieldOption
                        {
                            StatusId = caseStatus.Id,
                            FieldOptionId = fieldTypeOption.Id
                        };

                        if (caseStatus.SelectedOptions == null)
                        {
                            caseStatus.SelectedOptions = new List<CaseStatusFieldOption>();
                        }
                        caseStatus.SelectedOptions.Add(fieldOption);
                    }
                }
            }

            _databaseContext.SaveChanges();

            return caseStatus;
        }

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
            string first = string.IsNullOrWhiteSpace(firstName)
                ? null
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstName.ToLower());
            string last = string.IsNullOrWhiteSpace(lastName)
                ? null
                : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lastName.ToLower());

            return (first + " " + last).TrimStart().TrimEnd();
        }

        private (Worker, Team, Person, Worker) GetCreateAllocationRequirements(CreateAllocationRequest request)
        {
            var worker = GetWorkerByWorkerId(request.AllocatedWorkerId);
            if (string.IsNullOrEmpty(worker.Email))
            {
                throw new CreateAllocationException("Worker details cannot be found");
            }

            var team = _databaseContext.Teams.FirstOrDefault(x => x.Id == request.AllocatedTeamId);
            if (team == null)
            {
                throw new CreateAllocationException("Team details cannot be found");
            }

            var person = _databaseContext.Persons.Where(x => x.Id == request.MosaicId).FirstOrDefault();
            if (person == null)
            {
                throw new CreateAllocationException($"Person with given id ({request.MosaicId}) not found");
            }

            var allocatedBy = _databaseContext.Workers.Where(x => x.Email.ToUpper().Equals(request.CreatedBy.ToUpper()))
                .FirstOrDefault();
            if (allocatedBy == null)
            {
                throw new CreateAllocationException(
                    $"Worker with given allocated by email address ({request.CreatedBy}) not found");
            }

            return (worker, team, person, allocatedBy);
        }

        private (Person, Worker) GetUpdateAllocationRequirements(AllocationSet allocation,
            UpdateAllocationRequest request)
        {
            if (allocation.CaseStatus?.ToUpper() == "CLOSED")
            {
                throw new UpdateAllocationException("Allocation already closed");
            }

            var worker = GetWorkerByWorkerId(allocation.WorkerId ?? 0);

            if (worker == null)
            {
                throw new UpdateAllocationException("Worker not found");
            }

            var person = _databaseContext.Persons.FirstOrDefault(x => x.Id == allocation.PersonId);
            if (person == null)
            {
                throw new UpdateAllocationException("Person not found");
            }

            var createdBy =
                _databaseContext.Workers.FirstOrDefault(x => x.Email.ToUpper().Equals(request.CreatedBy.ToUpper()));
            if (createdBy == null)
            {
                throw new UpdateAllocationException("CreatedBy not found");
            }

            return (person, createdBy);
        }

        private static Address GetNewDisplayAddress(string addressLines, string postcode, long? uprn, string createdBy)
        {
            return new Address()
            {
                AddressLines = addressLines,
                PostCode = postcode,
                Uprn = uprn,
                IsDisplayAddress = "Y",
                DataIsFromDmPersonsBackup = "N",
                StartDate = DateTime.Now,
                CreatedBy = createdBy
            };
        }

    }
}
