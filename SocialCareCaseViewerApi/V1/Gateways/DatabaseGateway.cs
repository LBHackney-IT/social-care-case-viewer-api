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

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class DatabaseGateway : IDatabaseGateway
    {
        private readonly DatabaseContext _databaseContext;

        public DatabaseGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public List<Allocation> SelectAllocations(string mosaicId)
        {
            //check if we have lookup details for this id
            string personId = GetNCReferenceByPersonId(mosaicId);

            if (!string.IsNullOrEmpty(personId)) mosaicId = personId;

            var allocations = _databaseContext.Allocations.Where(x => x.Id.ToUpper() == mosaicId.ToUpper())
                .Select(rec => new Allocation
                {
                    PersonId = rec.Id,
                    FirstName = rec.FirstName,
                    LastName = rec.LastName,
                    DateOfBirth = (rec.DateOfBirth != null) ? rec.DateOfBirth.ToString() : null,
                    Gender = rec.Gender,
                    GroupId = (rec.GroupId != null) ? rec.GroupId : null,
                    Ethnicity = rec.Ethnicity,
                    SubEthnicity = rec.SubEthnicity,
                    Religion = rec.Religion,
                    ServiceUserGroup = rec.ServiceUserGroup,
                    SchoolName = rec.SchoolName,
                    SchoolAddress = rec.SchoolAddress,
                    GpName = rec.GpName,
                    GpAddress = rec.GpAddress,
                    GpSurgery = rec.GpSurgery,
                    AllocatedWorker = rec.AllocatedWorker,
                    WorkerType = rec.WorkerType,
                    AllocatedWorkerTeam = rec.AllocatedWorkerTeam,
                    TeamName = rec.TeamName,
                    AllocationStartDate = (rec.AllocationStartDate != null) ? rec.AllocationStartDate.ToString() : null,
                    AllocationEndDate = (rec.AllocationEndDate != null) ? rec.AllocationEndDate.ToString() : null,
                    LegalStatus = rec.LegalStatus,
                    Placement = rec.Placement,
                    OnCpRegister = rec.OnCpRegister,
                    ContactAddress = rec.ContactAddress,
                    CaseStatus = rec.CaseStatus,
                    CaseClosureDate = (rec.CaseClosureDate != null) ? rec.CaseClosureDate.ToString() : null,
                    WorkerEmail = rec.WorkerEmail,
                    LAC = rec.LAC
                }
                ).ToList();

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
    }

}
