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
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using dbTechUse = SocialCareCaseViewerApi.V1.Infrastructure.TechUse;
using dbLastUpdated = SocialCareCaseViewerApi.V1.Infrastructure.LastUpdated;
using dbDisability = SocialCareCaseViewerApi.V1.Infrastructure.Disability;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using ResidentInformationResponse = SocialCareCaseViewerApi.V1.Boundary.Response.ResidentInformation;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using DomainWorker = SocialCareCaseViewerApi.V1.Domain.Worker;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class DatabaseGateway : IDatabaseGateway
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IProcessDataGateway _processDataGateway;
        private readonly IWorkerGateway _workerGateway;
        private readonly ITeamGateway _teamGateway;
        private readonly ISystemTime _systemTime;

        public DatabaseGateway(DatabaseContext databaseContext, IProcessDataGateway processDataGateway, ISystemTime systemTime)
        {
            _databaseContext = databaseContext;
            _processDataGateway = processDataGateway;
            _workerGateway = new WorkerGateway(databaseContext);
            _teamGateway = new TeamGateway(databaseContext);
            _systemTime = systemTime;
        }

        public (List<Allocation>, int?, int) SelectAllocations(long mosaicId, long workerId, string workerEmail, long teamId, long allocationId, string sortBy = "rag_rating", int cursor = 0, string teamAllocationStatus = null, string status = "OPEN")
        {
            var limit = 250;

            List<Allocation> allocations = new List<Allocation>();
            IQueryable<AllocationSet> query = _databaseContext.Allocations;

            if (mosaicId != 0)
            {
                query = query.Where(x => x.PersonId == mosaicId);

                var teams = query.Where(x => x.TeamId != null && x.WorkerId == null && x.CaseStatus.ToLower() != "closed").ToList();
                var workerTeams = query.Where(x => x.TeamId != null && x.WorkerId != null && x.CaseStatus.ToLower() != "closed").ToList();

                foreach (var allocation in teams)
                {
                    if (workerTeams.Any(x => x.TeamId == allocation.TeamId && x.PersonId == allocation.PersonId))
                    {
                        query = query.Where(x => !(x.TeamId == allocation.TeamId && x.WorkerId == null));
                    };
                }

                if (!String.IsNullOrEmpty(status))
                {
                    query = query.Where(x => x.CaseStatus.ToLower() == status.ToLower());
                }
            }

            else if (allocationId != 0)
            {
                query = query.Where(x => x.Id == allocationId);
            }

            else if (workerId != 0)
            {
                query = query.Where(x => x.WorkerId == workerId);

                var teams = query.Where(x => x.TeamId != null && x.WorkerId == null && x.CaseStatus.ToLower() != "closed").ToList();
                var workerTeams = query.Where(x => x.TeamId != null && x.WorkerId != null && x.CaseStatus.ToLower() != "closed").ToList();

                foreach (var allocation in teams)
                {
                    if (workerTeams.Any(x => x.TeamId == allocation.TeamId && x.PersonId == allocation.PersonId))
                    {
                        query = query.Where(x => !(x.TeamId == allocation.TeamId && x.WorkerId == null));
                    };
                }

                if (!String.IsNullOrEmpty(status))
                {
                    query = query.Where(x => x.CaseStatus.ToLower() == status.ToLower());
                }
            }
            else if (!String.IsNullOrEmpty(workerEmail))
            {
                query = query.Include(x => x.Worker)
                    .Where(x => x.Worker.Email == workerEmail);
            }
            else if (teamId != 0)
            {
                query = query.Where(x => x.TeamId == teamId);


                if (!String.IsNullOrEmpty(status))
                {
                    query = query.Where(x => x.CaseStatus.ToLower() == status.ToLower());
                }
                if (!String.IsNullOrEmpty(teamAllocationStatus))
                {
                    if (teamAllocationStatus == "allocated")
                    {
                        query = query.Where(x => x.WorkerId != null);
                    }
                    if (teamAllocationStatus == "unallocated")
                    {

                        var allocatedList = query.Where(x => x.WorkerId != null).ToList();
                        query = query.Where(x => x.WorkerId == null);

                        foreach (var allocation in query.ToList())
                        {
                            if (allocatedList.Any(x => x.PersonId == allocation.PersonId))
                            {
                                query = query.Where(y => y.Id != allocation.Id);
                            }
                        }
                    }
                }
            }

            if (query != null)
            {
                allocations = query
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
                        AllocatedWorkerTeamId = x.Team.Id,
                        WorkerType = x.Worker.Role,
                        AllocationStartDate = x.AllocationStartDate,
                        AllocationEndDate = x.AllocationEndDate,
                        CaseStatus = x.CaseStatus,
                        PersonAddress =
                            x.Person.Addresses.FirstOrDefault(x =>
                                !string.IsNullOrEmpty(x.IsDisplayAddress) &&
                                x.IsDisplayAddress.ToUpper() == "Y") == null
                                ? null
                                : x.Person.Addresses.FirstOrDefault(x => x.IsDisplayAddress.ToUpper() == "Y")
                                    .AddressLines,
                        RagRating = x.RagRating,
                        PersonReviewDate = x.Person.ReviewDate
                    }
                    ).AsNoTracking().ToList();
            }

            foreach (var allocation in allocations)
            {
                if (allocation.AllocatedWorker != null && allocation.CaseStatus?.ToLower() != "closed")
                {
                    var teamAllocation = _databaseContext.Allocations.FirstOrDefault(x => x.PersonId == allocation.PersonId && x.WorkerId == null && x.TeamId == allocation.AllocatedWorkerTeamId && x.MarkedForDeletion == false);
                    allocation.TeamAllocationStartDate = teamAllocation?.AllocationStartDate;
                }
            }

            var totalCount = allocations.Count;

            allocations = sortBy switch
            {
                "rag_rating" =>
                    allocations
                        .OrderByDescending(x => x.RagRating != null ? Enum.Parse(typeof(RagRatingToNumber), x.RagRating, true) : null)
                        .ThenByDescending(x => x.RagRating == null).ToList(),
                "date_added" =>
                    allocations
                        .OrderBy(x => x.AllocationStartDate).ToList(),
                "review_date" =>
                    allocations.OrderByDescending(x => x.PersonReviewDate.HasValue)
                        .ThenBy(x => x.PersonReviewDate).ToList(),
                _ =>
                    allocations
                        .OrderByDescending(x => x.RagRating != null ? Enum.Parse(typeof(RagRatingToNumber), x.RagRating, true) : null)
                        .ThenByDescending(x => x.RagRating == null).ToList()
            };

            return (allocations
                    .Skip(cursor)
                    .Take(limit)
                    .ToList(),
                    GetNextOffset(cursor, totalCount, limit), totalCount);
        }

        private static int? GetNextOffset(int currentOffset, int totalRecords, int limit)
        {
            int nextOffset = currentOffset + limit;

            if (nextOffset < totalRecords)
            {
                return nextOffset;
            }
            else
            {
                return null;
            }
        }
        private enum RagRatingToNumber
        {
            None,
            Low,
            Medium,
            High,
            Urgent
        }

        public (List<ResidentInformationResponse>, int) GetResidentsBySearchCriteria(int cursor, int limit, long? id = null, string firstName = null,
          string lastName = null, string dateOfBirth = null, string postcode = null, string address = null, string contextFlag = null)
        {
            var addressSearchPattern = GetSearchPattern(address);
            var postcodeSearchPattern = GetSearchPattern(postcode);

            var queryByAddress = postcode != null || address != null;

            var (peopleIds, totalCount) = queryByAddress
                ? GetPersonIdsBasedOnAddressCriteria(cursor, limit, id, firstName, lastName, postcode, address, contextFlag)
                : GetPersonIdsBasedOnSearchCriteria(cursor, limit, id, firstName, lastName, dateOfBirth, contextFlag);

            var var = _databaseContext.Persons
                .Where(p => peopleIds.Contains(p.Id))
                .Include(p => p.Addresses)
                .Include(p => p.PhoneNumbers);

            var dbRecords = _databaseContext.Persons
                .Where(p => peopleIds.Contains(p.Id) && p.MarkedForDeletion == false)
                .Include(p => p.Addresses)
                .Include(p => p.PhoneNumbers)
                .Select(x => x.ToResidentInformationResponse()).ToList();

            return (dbRecords, totalCount);
        }

        private (List<long>, int) GetPersonIdsBasedOnSearchCriteria(int cursor, int limit, long? id, string firstname, string lastname, string dateOfBirth, string contextflag)
        {
            var firstNameSearchPattern = GetSearchPattern(firstname);
            var lastNameSearchPattern = GetSearchPattern(lastname);
            var dateOfBirthSearchPattern = GetSearchPattern(dateOfBirth);
            var contextFlagSearchPattern = GetSearchPattern(contextflag);

            var query = _databaseContext.Persons
                .Where(person => id == null || EF.Functions.ILike(person.Id.ToString(), id.ToString()))
                .Where(person => string.IsNullOrEmpty(firstname) || EF.Functions.ILike(person.FirstName.Replace(" ", ""), firstNameSearchPattern))
                .Where(person => string.IsNullOrEmpty(lastname) || EF.Functions.ILike(person.LastName, lastNameSearchPattern))
                .Where(person => string.IsNullOrEmpty(dateOfBirth) || EF.Functions.ILike(person.DateOfBirth.ToString(), dateOfBirthSearchPattern))
                .Where(person => string.IsNullOrEmpty(contextflag) || EF.Functions.ILike(person.AgeContext, contextFlagSearchPattern));

            var totalCount = query.Count();

            return (query
                .Where(person => person.Id > cursor)
                .OrderBy(p => p.Id)
                .Take(limit)
                .Select(p => p.Id)
                .ToList(), totalCount);
        }

        private (List<long>, int) GetPersonIdsBasedOnAddressCriteria(int cursor, int limit, long? id, string firstname, string lastname,
           string postcode, string address, string contextflag)
        {
            var firstNameSearchPattern = GetSearchPattern(firstname);
            var lastNameSearchPattern = GetSearchPattern(lastname);
            var addressSearchPattern = GetSearchPattern(address);
            var postcodeSearchPattern = GetSearchPattern(postcode);
            var contextFlagSearchPattern = GetSearchPattern(contextflag);

            var query = _databaseContext.Addresses
                .Where(add => id == null || EF.Functions.ILike(add.PersonId.ToString(), id.ToString()))
                .Where(add => string.IsNullOrEmpty(address) || EF.Functions.ILike(add.AddressLines.Replace(" ", ""), addressSearchPattern))
                .Where(add => string.IsNullOrEmpty(postcode) || EF.Functions.ILike(add.PostCode.Replace(" ", ""), postcodeSearchPattern) && add.IsDisplayAddress == "Y")
                .Where(add => string.IsNullOrEmpty(firstname) || EF.Functions.ILike(add.Person.FirstName.Replace(" ", ""), firstNameSearchPattern))
                .Where(add => string.IsNullOrEmpty(lastname) || EF.Functions.ILike(add.Person.LastName, lastNameSearchPattern))
                .Where(add => string.IsNullOrEmpty(contextflag) || EF.Functions.ILike(add.Person.AgeContext, contextFlagSearchPattern))
                .Include(add => add.Person);

            var totalCount = query.Count();

            return (query
               .Where(add => add.PersonId > cursor)
               .OrderBy(add => add.PersonId)
               .GroupBy(add => add.PersonId)
               .Where(p => p.Key != null)
               .Take(limit)
               .Select(p => (long) p.Key)
               .ToList(), totalCount);
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
                throw new ResidentCouldNotBeInsertedException(
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
                .Include(x => x.KeyContacts)
                .Include(x => x.GpDetails)
                .Include(x => x.TechUse)
                .Include(x => x.Disability)
                .Include(x => x.Emails)
                .Include(x => x.LastUpdated)
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
            person.FluentInEnglish = request.FluentInEnglish;
            person.InterpreterNeeded = request.InterpreterNeeded;
            person.CommunicationDifficulties = request.CommunicationDifficulties;
            person.DifficultyMakingDecisions = request.DifficultyMakingDecisions;
            person.CommunicationDifficultiesDetails = request.CommunicationDifficultiesDetails;
            person.Employment = request.Employment;
            person.AllocatedTeam = request.AllocatedTeam;
            person.PreferredLanguage = request.PreferredLanguage;
            person.Nationality = request.Nationality;
            person.CreatedBy = request.CreatedBy;
            person.Pronoun = request.Pronoun;
            person.GenderAssignedAtBirth = request.GenderAssignedAtBirth;
            person.MaritalStatus = request.MaritalStatus;
            person.ImmigrationStatus = request.ImmigrationStatus;
            person.PrimarySupportReason = request.PrimarySupportReason;
            person.CareProvider = request.CareProvider;
            person.LivingSituation = request.LivingSituation;
            person.TenureType = request.TenureType;
            person.AccomodationType = request.AccomodationType;
            person.AccessToHome = request.AccessToHome;
            person.HousingOfficer = request.HousingOfficer;
            person.HousingStaffInContact = request.HousingStaffInContact;
            person.CautionaryAlert = request.CautionaryAlert;
            person.PossessionEvictionOrder = request.PossessionEvictionOrder;
            person.RentRecord = request.RentRecord;
            person.HousingBenefit = request.HousingBenefit;
            person.CouncilTenureType = request.CouncilTenureType;
            person.MentalHealthSectionStatus = request.MentalHealthSectionStatus;
            person.DeafRegister = request.DeafRegister;
            person.BlindRegister = request.BlindRegister;
            person.BlueBadge = request.BlueBadge;
            person.OpenCase = request.OpenCase;
            person.ReviewDate = request.ReviewDate;

            //replace Last Updated
            _databaseContext.LastUpdated.RemoveRange(person.LastUpdated);

            if (request.LastUpdated != null)
            {
                foreach (var entry in request.LastUpdated)
                {
                    person.LastUpdated.Add(entry.ToEntity(person.Id));
                }
            }

            //replace tech used
            _databaseContext.TechUse.RemoveRange(person.TechUse);

            if (request.TechUse != null)
            {
                foreach (var entry in request.TechUse)
                {
                    person.TechUse.Add(new dbTechUse { TechType = entry, PersonId = person.Id, });
                }
            }

            //replace disabilities
            _databaseContext.Disabilities.RemoveRange(person.Disability);

            if (request.Disabilities != null)
            {
                foreach (var entry in request.Disabilities)
                {
                    person.Disability.Add(new dbDisability { DisabilityType = entry, PersonId = person.Id, });
                }
            }
            //replace key contacts
            _databaseContext.KeyContacts.RemoveRange(person.KeyContacts);

            if (request.KeyContacts != null)
            {
                foreach (var contact in request.KeyContacts)
                {
                    person.KeyContacts.Add(contact.ToEntity(person.Id));
                }
            }

            //replace gp details
            _databaseContext.GpDetails.RemoveRange(person.GpDetails);

            if (request.GpDetails != null)
            {
                person.GpDetails.Add(request.GpDetails.ToEntity(person.Id));
            }

            //replace emails
            _databaseContext.Emails.RemoveRange(person.Emails);

            if (request.Emails != null)
            {
                foreach (var email in request.Emails)
                {
                    person.Emails.Add(email.ToEntity());
                }
            }

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
                Address displayAddress = person.Addresses.OrderByDescending(x => x.StartDate).FirstOrDefault(x => x.IsDisplayAddress == "Y");

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
                Address displayAddress = person.Addresses.OrderByDescending(x => x.StartDate).FirstOrDefault(x => x.IsDisplayAddress == "Y");

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

        public void PatchPerson(PatchPersonRequest request)
        {
            Person person = _databaseContext
                .Persons
                .Include(x => x.Addresses)
                .Include(x => x.PhoneNumbers)
                .Include(x => x.OtherNames)
                .Include(x => x.KeyContacts)
                .Include(x => x.GpDetails)
                .Include(x => x.TechUse)
                .Include(x => x.Disability)
                .Include(x => x.Emails)
                .Include(x => x.LastUpdated)
                .FirstOrDefault(x => x.Id == request.Id);

            if (person == null)
            {
                throw new UpdatePersonException("Person not found");
            }

            if (request.FirstName != null && request.LastName != null)
            {
                person.FullName = $"{request.FirstName} {request.LastName}";
            }
            else if (request.FirstName != null && request.LastName == null)
            {
                person.FullName = $"{request.FirstName} {person.LastName}";
            }
            else if (request.FirstName == null && request.LastName != null)
            {
                person.FullName = $"{person.FirstName} {request.LastName}";
            }

            person.AgeContext = request.ContextFlag ?? person.AgeContext;
            person.ReviewDate = request.ReviewDate ?? person.ReviewDate;
            person.DateOfBirth = request.DateOfBirth ?? person.DateOfBirth;
            person.DateOfDeath = request.DateOfDeath ?? person.DateOfDeath;
            person.EmailAddress = request.EmailAddress ?? person.EmailAddress;
            person.Ethnicity = request.Ethnicity ?? person.Ethnicity;
            person.FirstLanguage = request.FirstLanguage ?? person.FirstLanguage;
            person.FirstName = request.FirstName ?? person.FirstName;
            person.Gender = request.Gender ?? person.Gender;
            person.LastModifiedBy = request.CreatedBy ?? person.LastModifiedBy;
            person.LastName = request.LastName ?? person.LastName;
            person.NhsNumber = request.NhsNumber ?? person.NhsNumber;
            person.PreferredMethodOfContact = request.PreferredMethodOfContact ?? person.PreferredMethodOfContact;
            person.Religion = request.Religion ?? person.Religion;
            person.Restricted = request.Restricted ?? person.Restricted;
            person.SexualOrientation = request.SexualOrientation ?? person.SexualOrientation;
            person.Title = request.Title ?? person.Title;
            person.FluentInEnglish = request.FluentInEnglish ?? person.FluentInEnglish;
            person.InterpreterNeeded = request.InterpreterNeeded ?? person.InterpreterNeeded;
            person.CommunicationDifficulties = request.CommunicationDifficulties ?? person.CommunicationDifficulties;
            person.DifficultyMakingDecisions = request.DifficultyMakingDecisions ?? person.DifficultyMakingDecisions;
            person.CommunicationDifficultiesDetails = request.CommunicationDifficultiesDetails ?? person.CommunicationDifficultiesDetails;
            person.Employment = request.Employment ?? person.Employment;
            person.AllocatedTeam = request.AllocatedTeam ?? person.AllocatedTeam;
            person.HousingOfficer = request.HousingOfficer ?? person.HousingOfficer;
            person.AccessToHome = request.AccessToHome ?? person.AccessToHome;
            person.AccomodationType = request.AccomodationType ?? person.AccomodationType;
            person.TenureType = request.TenureType ?? person.TenureType;
            person.LivingSituation = request.LivingSituation ?? person.LivingSituation;
            person.CareProvider = request.CareProvider ?? person.CareProvider;
            person.PrimarySupportReason = request.PrimarySupportReason ?? person.PrimarySupportReason;
            person.ImmigrationStatus = request.ImmigrationStatus ?? person.ImmigrationStatus;
            person.MaritalStatus = request.MaritalStatus ?? person.MaritalStatus;
            person.GenderAssignedAtBirth = request.GenderAssignedAtBirth ?? person.GenderAssignedAtBirth;
            person.Pronoun = request.Pronoun ?? person.Pronoun;
            person.Nationality = request.Nationality ?? person.Nationality;
            person.HousingStaffInContact = request.HousingStaffInContact ?? person.HousingStaffInContact;
            person.CautionaryAlert = request.CautionaryAlert ?? person.CautionaryAlert;
            person.PossessionEvictionOrder = request.PossessionEvictionOrder ?? person.PossessionEvictionOrder;
            person.RentRecord = request.RentRecord ?? person.RentRecord;
            person.HousingBenefit = request.HousingBenefit ?? person.HousingBenefit;
            person.MentalHealthSectionStatus = request.MentalHealthSectionStatus ?? person.MentalHealthSectionStatus;
            person.DeafRegister = request.DeafRegister ?? person.DeafRegister;
            person.BlindRegister = request.BlindRegister ?? person.BlindRegister;
            person.PreferredLanguage = request.PreferredLanguage ?? request.PreferredLanguage;
            person.BlueBadge = request.BlueBadge ?? person.BlueBadge;
            person.OpenCase = request.OpenCase ?? person.OpenCase;
            person.CouncilTenureType = request.CouncilTenureType ?? person.CouncilTenureType;

            //replace Last Updated
            if (request.LastUpdated != null)
            {
                _databaseContext.LastUpdated.RemoveRange(person.LastUpdated);
                foreach (var entry in request.LastUpdated)
                {
                    person.LastUpdated.Add(entry.ToEntity(person.Id));
                }
            }

            //replace tech used
            if (request.TechUse != null)
            {
                _databaseContext.TechUse.RemoveRange(person.TechUse);
                foreach (var entry in request.TechUse)
                {
                    person.TechUse.Add(new dbTechUse { TechType = entry, PersonId = person.Id, });
                }
            }

            //replace disabilities
            if (request.Disabilities != null)
            {
                _databaseContext.Disabilities.RemoveRange(person.Disability);
                foreach (var entry in request.Disabilities)
                {
                    person.Disability.Add(new dbDisability { DisabilityType = entry, PersonId = person.Id, });
                }
            }
            //replace key contacts
            if (request.KeyContacts != null)
            {
                _databaseContext.KeyContacts.RemoveRange(person.KeyContacts);
                foreach (var contact in request.KeyContacts)
                {
                    person.KeyContacts.Add(contact.ToEntity(person.Id));
                }
            }

            //replace gp details
            if (request.GpDetails != null)
            {
                _databaseContext.GpDetails.RemoveRange(person.GpDetails);
                person.GpDetails.Add(request.GpDetails.ToEntity(person.Id));
            }

            //replace emails
            if (request.Emails != null)
            {
                _databaseContext.Emails.RemoveRange(person.Emails);
                foreach (var email in request.Emails)
                {
                    person.Emails.Add(email.ToEntity());
                }
            }

            //replace phone numbers
            if (request.PhoneNumbers != null)
            {
                _databaseContext.PhoneNumbers.RemoveRange(person.PhoneNumbers);
                foreach (var number in request.PhoneNumbers)
                {
                    person.PhoneNumbers.Add(number.ToEntity(person.Id, request.CreatedBy));
                }
            }

            //replace other names
            if (request.OtherNames != null)
            {
                _databaseContext.PersonOtherNames.RemoveRange(person.OtherNames);
                foreach (var otherName in request.OtherNames)
                {
                    person.OtherNames.Add(otherName.ToEntity(person.Id, request.CreatedBy));
                }
            }

            //check for changed address
            if (request.Address != null)
            {
                Address displayAddress = person.Addresses.OrderByDescending(x => x.StartDate).FirstOrDefault(x => x.IsDisplayAddress == "Y");

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

        public string GetPersonIdByNCReference(string nfReference)
        {
            PersonIdLookup lookup = _databaseContext.PersonLookups.Where(x => x.NCId == nfReference).FirstOrDefault();

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

        public Worker GetWorkerByEmail(string email)
        {
            var worker = _databaseContext.Workers
                    .Where(worker => worker.Email.ToLower() == email.ToLower())
                    .Include(x => x.Allocations)
                    .Include(x => x.WorkerTeams)
                    .ThenInclude(y => y.Team)
                    .FirstOrDefault();

            WorkerTeamFiltering.RemoveHistoricalWorkerTeamsFromAWorker(worker);

            return worker;
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
        public void UpdateWorker(UpdateWorkerRequest updateWorkerRequest)
        {
            var worker = _databaseContext.Workers.Include(x => x.WorkerTeams).FirstOrDefault(x => x.Id == updateWorkerRequest.WorkerId);

            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with Id {updateWorkerRequest.WorkerId} not found");
            }

            worker.LastModifiedBy = updateWorkerRequest.ModifiedBy;
            worker.FirstName = updateWorkerRequest.FirstName;
            worker.LastName = updateWorkerRequest.LastName;
            worker.ContextFlag = updateWorkerRequest.ContextFlag;
            worker.Role = updateWorkerRequest.Role;
            worker.DateStart = updateWorkerRequest.DateStart;
            worker.IsActive = true;

            var dateTime = DateTime.Now;

            if (updateWorkerRequest.Teams != null && updateWorkerRequest.Teams.Count > 0)
            {
                //boundary locked down to one team only, but ensure we only have one
                if (updateWorkerRequest.Teams.Count > 1)
                {
                    throw new Exception("Worker can have only one team");
                }

                //check that team has changed. If not, ignore
                if (!worker.WorkerTeams.Any(x => x.TeamId == updateWorkerRequest.Teams.FirstOrDefault().Id && x.EndDate == null && x.StartDate != null))
                {
                    //set end date to all active team relationships. This helps getting all old (incorrectly created) records up to date
                    foreach (var workerteam in worker.WorkerTeams?.Where(x => x.EndDate == null))
                    {
                        workerteam.EndDate = dateTime;
                        workerteam.LastModifiedBy = updateWorkerRequest.ModifiedBy;
                    }

                    var team = updateWorkerRequest.Teams.FirstOrDefault();

                    //make sure team exists
                    if (!_databaseContext.Teams.AsNoTracking().Any(x => x.Id == team.Id))
                    {
                        throw new GetTeamException($"Team with Name {team.Name} and ID {team.Id} not found");
                    }
                    //add new team with start date
                    worker.WorkerTeams.Add(new WorkerTeam { StartDate = dateTime, TeamId = team.Id, Worker = worker, CreatedBy = updateWorkerRequest.ModifiedBy });
                }
            }

            _databaseContext.SaveChanges();
        }

        private ICollection<Team> GetTeams(List<WorkerTeamRequest> request)
        {
            var teamsWorkerBelongsIn = new List<Team>();
            foreach (var requestTeam in request)
            {
                var team = _teamGateway.GetTeamByTeamId(requestTeam.Id);
                if (team == null)
                {
                    throw new GetTeamException($"Team with Name {requestTeam.Name} and ID {requestTeam.Id} not found");
                }

                teamsWorkerBelongsIn.Add(team);
            }

            return teamsWorkerBelongsIn;
        }

        public Team GetTeamByTeamName(string name)
        {
            var team = _databaseContext.Teams
                .Where(x => x.Name.ToUpper().Equals(name.ToUpper()))
                .Include(x => x.WorkerTeams)
                .ThenInclude(x => x.Worker)
                .ThenInclude(x => x.Allocations)
                .FirstOrDefault();

            WorkerTeamFiltering.RemoveHistoricalWorkerTeamsFromATeam(team);

            return team;
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

            var residentAllocations = _databaseContext.Allocations.Where(x => x.MarkedForDeletion == false && x.CaseStatus.ToUpper() == "OPEN" && x.Person.Id == request.MosaicId).ToList();

            var hasExistingTeamOnlyAllocation = residentAllocations.Any(x => x.TeamId == request.AllocatedTeamId && x.WorkerId == null);
            var hasExistingTeamAndWorkerAllocation = residentAllocations.Any(x => x.TeamId == request.AllocatedTeamId && x.WorkerId != null);

            // If person has allocation with the same team already and request is to allocate a team
            if (request.AllocatedWorkerId == null && hasExistingTeamOnlyAllocation)
            {
                throw new CreateAllocationException(
                    "Person is already allocated to this team");
            }

            // If person has worker allocation with the same team already and a worker and request is to allocate a worker
            if (request.AllocatedWorkerId != null && hasExistingTeamAndWorkerAllocation)
            {
                throw new CreateAllocationException(
                    "Person has already allocated worker in this team");
            }

            var existingAllocation = residentAllocations.FirstOrDefault(x => x.TeamId == request.AllocatedTeamId);

            if (existingAllocation != null && request.AllocationStartDate < existingAllocation.AllocationStartDate)
            {
                throw new CreateAllocationException(
                    "Worker Allocation date must be after Team Allocation date");
            }

            var response = new CreateAllocationResponse();

            // Team and worker allocation
            if (request.AllocatedWorkerId != null && !hasExistingTeamOnlyAllocation)
            {
                CreateTeamAndWorkerAllocation(request, person, null, allocatedBy, team);
                response = CreateTeamAndWorkerAllocation(request, person, worker, allocatedBy, team);
            }

            // Team allocation
            if (request.AllocatedWorkerId == null && !hasExistingTeamOnlyAllocation)
            {
                response = CreateTeamAndWorkerAllocation(request, person, null, allocatedBy, team);
            }

            // Worker allocation
            if (request.AllocatedWorkerId != null && hasExistingTeamOnlyAllocation)
            {
                var existingTeamAllocation = residentAllocations.FirstOrDefault(x => x.TeamId == request.AllocatedTeamId && x.WorkerId == null);
                request.RagRating = existingTeamAllocation.RagRating;
                response = CreateTeamAndWorkerAllocation(request, person, worker, allocatedBy, team);
            }

            return response;
        }

        private CreateAllocationResponse CreateTeamAndWorkerAllocation(CreateAllocationRequest request, Person person, DomainWorker worker, Worker allocatedBy, Team team)
        {
            var allocation = new AllocationSet
            {
                PersonId = person.Id,
                WorkerId = worker?.Id,
                TeamId = team.Id,
                AllocationStartDate = request.AllocationStartDate,
                CaseStatus = "Open",
                RagRating = request.RagRating,
                CarePackage = request.CarePackage,
                Summary = request.Summary,
                CreatedBy = allocatedBy.Email
            };

            _databaseContext.Allocations.Add(allocation);
            _databaseContext.SaveChanges();


            var response = new CreateAllocationResponse();
            //Add note
            try
            {
                var dt = DateTime.Now;

                var formName = worker?.Id == null ? "Team allocated" : "Worker allocated";

                var note = new AllocationCaseNote
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    MosaicId = person.Id.ToString(),
                    Timestamp = dt.ToString("dd/MM/yyyy H:mm:ss"), //in line with imported form data
                    WorkerEmail = allocatedBy.Email,
                    Note =
                        $"{dt.ToShortDateString()} | Allocation | {person.FirstName} {person.LastName} was allocated to the team {team.Name} {(worker == null ? "" : $" and {worker.FirstName} {worker.LastName}")} (by {allocatedBy.FirstName} {allocatedBy.LastName})",
                    FormNameOverall = "API_Allocation",
                    FormName = formName,
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

        public UpdateAllocationResponse UpdateRagRatingInAllocation(UpdateAllocationRequest request)
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
                allocation.LastModifiedBy = request.CreatedBy;

                allocation.RagRating = request.RagRating;
                _databaseContext.SaveChanges();

                try
                {
                    var note = new UpdateRagRatingCaseNote
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        MosaicId = person.Id.ToString(),
                        Timestamp = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss"),
                        WorkerEmail = createdBy.Email, //required for my cases search
                        FormNameOverall = "API_Allocation", //prefix API notes so they are easy to identify
                        FormName = "Rag Rating Updated",
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
                var matchingTeamAllocation = _databaseContext.Allocations.FirstOrDefault(x => x.TeamId == allocation.TeamId && x.PersonId == allocation.PersonId && x.WorkerId == null && x.CaseStatus.ToLower() == "open");

                if (allocation.WorkerId != null && matchingTeamAllocation == null && allocation.TeamId != null)
                {
                    var teamAllocationToInsert = new AllocationSet()
                    {
                        PersonId = allocation.PersonId,
                        TeamId = allocation.TeamId,
                        RagRating = allocation.RagRating,
                        AllocationStartDate = allocation.AllocationStartDate,
                        CreatedBy = allocation.CreatedBy,
                        CreatedAt = allocation.CreatedAt,
                        CaseStatus = allocation.CaseStatus
                    };
                    _databaseContext.Allocations.Add(teamAllocationToInsert);
                    _databaseContext.SaveChanges();
                }

                var (person, createdBy) = GetUpdateAllocationRequirements(allocation, request);


                //copy existing values in case adding note fails
                var tmpAllocation = (AllocationSet) allocation.Clone();
                SetDeallocationValues(allocation, (DateTime) request.DeallocationDate, request.CreatedBy);
                if (request.DeallocationScope != null && matchingTeamAllocation != null && request.DeallocationScope.ToLower() == "team")
                {
                    SetDeallocationValues(matchingTeamAllocation, (DateTime) request.DeallocationDate, request.CreatedBy);
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
                            FormName = "Team deallocated",
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
                };
                SetDeallocationValues(allocation, (DateTime) request.DeallocationDate, request.CreatedBy);
                _databaseContext.SaveChanges();

                if (allocation.WorkerId != null)
                {
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
                .Include(x => x.KeyContacts)
                .Include(x => x.GpDetails)
                .Include(x => x.TechUse)
                .Include(x => x.Disability)
                .Include(x => x.Emails)
                .Include(x => x.LastUpdated)
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

        public void DeleteRelationship(long relationshipId)
        {
            var relationship = _databaseContext.PersonalRelationships
                .Where(prt => prt.Id == relationshipId)
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

        private (Domain.Worker?, Team, Person, Worker) GetCreateAllocationRequirements(CreateAllocationRequest request)
        {
            var worker = _workerGateway.GetWorkerByWorkerId(request.AllocatedWorkerId);
            if (string.IsNullOrEmpty(worker?.Email) && request.AllocatedWorkerId != null)
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

            var worker = _workerGateway.GetWorkerByWorkerId(allocation.WorkerId ?? 0);

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
