using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using SocialCareCaseViewerApi.V1.Exceptions;
using Newtonsoft.Json;

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

        public List<Allocation> SelectAllocations(long mosaicId, long workerId) {

            List<Allocation> allocations = new List<Allocation>();
            //TODO: look into using navigation properties
            if (mosaicId != 0)  
            {
                allocations = (
                    from allocation in _databaseContext.Allocations

                    join worker in _databaseContext.Workers on allocation.WorkerId equals worker.Id into Workers
                    from w in Workers.DefaultIfEmpty()

                    join team in _databaseContext.Teams on w.TeamId equals team.Id into Teams
                    from t in Teams.DefaultIfEmpty()

                    where allocation.MosaicId == mosaicId

                    select new Allocation()
                    {
                        PersonId = allocation.MosaicId,
                        AllocatedWorker = w == null ? null : $"{w.FirstName} {w.LastName }",
                        AllocatedWorkerTeam = t.Name,
                        WorkerType = w.Role,
                        AllocationStartDate = (allocation.AllocationStartDate != null) ? allocation.AllocationStartDate.Value.ToString("dd/MM/yyyy") : null,
                        AllocationEndDate = (allocation.AllocationEndDate != null) ? allocation.AllocationEndDate.Value.ToString("dd/MM/yyyy") : null,
                        CaseStatus = allocation.CaseStatus
                    }

                    ).ToList();
            }
            else if(workerId != 0)
            {
                allocations = (
                    from allocation in _databaseContext.Allocations

                    join worker in _databaseContext.Workers on allocation.WorkerId equals worker.Id into Workers
                    from w in Workers.DefaultIfEmpty()

                    join team in _databaseContext.Teams on w.TeamId equals team.Id into Teams
                    from t in Teams.DefaultIfEmpty()

                    where w.Id == workerId

                    select new Allocation()
                    {
                        PersonId = allocation.MosaicId,
                        AllocatedWorker = w == null ? null : $"{w.FirstName} {w.LastName }",
                        AllocatedWorkerTeam = t.Name,
                        WorkerType = w.Role,
                        AllocationStartDate = (allocation.AllocationStartDate != null) ? allocation.AllocationStartDate.Value.ToString("dd/MM/yyyy") : null,
                        AllocationEndDate = (allocation.AllocationEndDate != null) ? allocation.AllocationEndDate.Value.ToString("dd/MM/yyyy") : null,
                        CaseStatus = allocation.CaseStatus
                    }

                    ).ToList();
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

        public AddNewResidentResponse AddNewResident(AddNewResidentRequest request)
        {
            var resident = new Person
            {
                DateOfBirth = request.DateOfBirth,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                Gender = request.Gender,
                Nationality = request.Nationality,
                NhsNumber = request.NhsNumber,
                Title = request.Title,
                AgeContext = request.AgeGroup,
                DataIsFromDmPersonsBackup = "N"
            };
            try
            {
                _databaseContext.Persons.Add(resident);
                _databaseContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new ResidentCouldNotBeinsertedException($"Error with inserting resident record has occurred - {ex.Message} - {ex.InnerException}");
            }
            AddResidentAddress(request.Address, resident.Id);
            return resident.ToResponse(request.Address);
        }

        public void AddResidentAddress(AddressDomain addressRequest, long personId)
        {
            var address = new Address
            {
                AddressLines = addressRequest.Address,
                PersonId = personId,
                PostCode = addressRequest.Postcode,
                Uprn = addressRequest.Uprn,
                DataIsFromDmPersonsBackup = "N"
            };

            try
            {
                _databaseContext.Addresses.Add(address);
                _databaseContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new AddressCouldNotBeInsertedException($"Error with inserting address has occurred - {ex.Message} - {ex.InnerException}");
            }
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

        public List<Worker> GetWorkers(int teamId)
        {
            return _databaseContext.Workers.Where(x => x.TeamId == teamId).ToList();
        }

        //TODO: use db views or queries 
        public List<dynamic> GetWorkerAllocations(List<Worker> workers)
        {
            List<dynamic> allocationsPerWorker = new List<dynamic>();

            foreach(var worker in workers)
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
            CreateAllocationResponse response = new CreateAllocationResponse();

            //get worker details
            Worker worker = _databaseContext.Workers.FirstOrDefault(x => x.Id == request.AllocatedWorkerId);

            if (string.IsNullOrEmpty(worker?.Email))
            {
                throw new CreateAllocationException("Worker details cannot be found");
            }

            //get team details for the note
            Team team = _databaseContext.Teams.FirstOrDefault(x => x.Id == worker.TeamId);

            if (team?.Id == null)
            {
                throw new CreateAllocationException("Team details cannot be found");
            }

            var entity = request.ToEntity(worker.Id, DateTime.Now, "Open");
            _databaseContext.Allocations.Add(entity);
            _databaseContext.SaveChanges();

            int allocationId = entity.Id; //new given id will be available here

            //check that required records exist
            Person person = _databaseContext.Persons.FirstOrDefault(x => x.Id == request.MosaicId);

            Worker allocatedBy = _databaseContext.Workers.FirstOrDefault(x => x.Email.ToUpper() == request.AllocatedBy.ToUpper());

            if (person == null)
            {
                AllocationSet allocationToDelete = _databaseContext.Allocations.Where(x => x.Id == allocationId).FirstOrDefault();
                _databaseContext.Allocations.Remove(allocationToDelete);
                _databaseContext.SaveChanges();

                throw new CreateAllocationException($"Person with given id ({request.MosaicId}) not found");
            }

            if (allocatedBy == null)
            {
                AllocationSet allocationToDelete = _databaseContext.Allocations.Where(x => x.Id == allocationId).FirstOrDefault();
                _databaseContext.Allocations.Remove(allocationToDelete);
                _databaseContext.SaveChanges();

                throw new CreateAllocationException($"Worker with given allocated by email address ({request.AllocatedBy}) not found");
            }

            //Add note
            try
            {
                DateTime dt = DateTime.Now;

                AllocationCaseNote note = new AllocationCaseNote()
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    MosaicId = person.Id.ToString(),
                    Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"), //in line with imported form data
                    WorkerEmail = worker.Email,
                    Note = $"{dt.ToShortDateString()} | Allocation | {worker.FirstName} {worker.LastName} in {team.Name} was allocated to this person (by {allocatedBy.FirstName} {allocatedBy.LastName})",
                    FormNameOverall = "API_Allocation"
                };

                CaseNotesDocument caseNotesDocument = new CaseNotesDocument()
                {
                    CaseFormData = JsonConvert.SerializeObject(note)
                };

                response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
            }
            catch (Exception ex)
            {
                //roll back allocation record
                AllocationSet allocationToDelete = _databaseContext.Allocations.Where(x => x.Id == allocationId).FirstOrDefault();
                _databaseContext.Allocations.Remove(allocationToDelete);
                _databaseContext.SaveChanges();

                throw new UpdateAllocationException($"Unable to create a case note. Allocation not created: {ex.Message}");
            }

            return response;
        }

        public UpdateAllocationResponse UpdateAllocation(UpdateAllocationRequest request)
        {
            DateTime dt = DateTime.Now;
            UpdateAllocationResponse response = new UpdateAllocationResponse();

            try
            {
                AllocationSet allocation = _databaseContext.Allocations.Where(x => x.Id == request.Id).FirstOrDefault();

                if (allocation != null)
                {
                    if (allocation.CaseStatus == "Closed")
                    {
                        throw new UpdateAllocationException("Allocation already closed");
                    }

                    //check that person exists
                    Person person = _databaseContext.Persons.FirstOrDefault(x => x.Id == allocation.MosaicId);

                    if (person == null)
                    {
                        throw new UpdateAllocationException("Person not found");
                    }

                    Worker worker = _databaseContext.Workers.FirstOrDefault(x => x.Id == allocation.WorkerId);

                    if(worker == null)
                    {
                        throw new UpdateAllocationException("Worker now found");
                    }

                    //copy existing values in case adding note fails
                    AllocationSet tmpAllocation = (AllocationSet) allocation.Clone();

                    SetDeallocationValues(allocation, dt);

                    _databaseContext.SaveChanges();

                    //TODO: use single data source for records and case notes
                    try
                    {
                        DeallocationCaseNote note = new DeallocationCaseNote()
                        {
                            FirstName = person.FirstName,
                            LastName = person.LastName,
                            MosaicId = person.Id.ToString(),
                            Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"), 
                            WorkerEmail = worker.Email, //required for my cases search
                            DeallocationReason = request.DeallocationReason,
                            FormNameOverall = "API_Deallocation" //prefix API notes so they are easy to identify
                        };

                        CaseNotesDocument caseNotesDocument = new CaseNotesDocument()
                        {
                            CaseFormData = JsonConvert.SerializeObject(note)
                        };

                        response.CaseNoteId = _processDataGateway.InsertCaseNoteDocument(caseNotesDocument).Result;
                    }
                    catch (Exception ex)
                    {
                        //roll back allocation record
                        //TODO: move case notes to postgresql for robust transaction handling
                        AllocationSet allocationToRestore = _databaseContext.Allocations.Where(x => x.Id == request.Id).FirstOrDefault();
                        RestoreAllocationValues(tmpAllocation, allocationToRestore);

                        _databaseContext.SaveChanges();

                        throw new UpdateAllocationException($"Unable to create a case note. Allocation not updated: {ex.Message}");
                    }
                }
                else
                {
                    throw new EntityUpdateException($"Allocation {request.Id} not found");
                }
            }
            catch (Exception ex)
            {
                throw new EntityUpdateException($"Unable to update allocation {request.Id}: {ex.Message}");
            }

            return response;
        }

        private static void SetDeallocationValues(AllocationSet allocation, DateTime dt)
        {
            allocation.AllocationEndDate = dt;
            allocation.CaseStatus = "Closed";
            allocation.WorkerId = null;
            allocation.CaseClosureDate = dt;
        }

        private static void RestoreAllocationValues(AllocationSet tmpAllocation, AllocationSet allocationToRestore)
        {
            allocationToRestore.AllocationEndDate = tmpAllocation.AllocationEndDate;
            allocationToRestore.CaseStatus = tmpAllocation.CaseStatus;
            allocationToRestore.WorkerId = tmpAllocation.WorkerId;
            allocationToRestore.CaseClosureDate = tmpAllocation.CaseClosureDate;
        }
    }
}
