using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class CaseStatusGateway : ICaseStatusGateway
    {
        private readonly DatabaseContext _databaseContext;

        public CaseStatusGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public CaseStatus GetCasesStatusByCaseStatusId(long id)
        {
            return _databaseContext.CaseStatuses
                .Where(cs => cs.Id == id)
                .Include(cs => cs.Type)
                .Include(cs => cs.SelectedOptions)
                .ThenInclude(csso => csso.FieldOption)
                .ThenInclude(fo => fo.TypeField)
                .FirstOrDefault()
                ?.ToDomain();
        }

        public List<CaseStatus> GetCaseStatusesByPersonId(long personId)
        {
            var caseStatuses = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.EndDate == null || cs.EndDate > DateTime.Today)
                .Include(cs => cs.Type)
                .Include(cs => cs.SelectedOptions)
                .ThenInclude(csso => csso.FieldOption)
                .ThenInclude(fo => fo.TypeField);

            return caseStatuses.Select(caseStatus => caseStatus.ToDomain()).ToList();
        }
        public CaseStatus GetCaseStatusesByPersonIdDate(long personId, DateTime date)
        {
            var caseStatus = _databaseContext.CaseStatuses.Where(cs => cs.PersonId == personId)
                .Where(cs => cs.StartDate <= date)
                .Where(cs => cs.EndDate == null || cs.EndDate >= date)
                .Include(cs => cs.Type)
                .Include(cs => cs.SelectedOptions)
                .ThenInclude(csso => csso.FieldOption)
                .ThenInclude(fo => fo.TypeField)
                .FirstOrDefault();

            return caseStatus?.ToDomain();
        }

        public CaseStatusType GetCaseStatusTypeWithFields(string type)
        {
            var response = _databaseContext.CaseStatusTypes
                .Where(cs => cs.Name == type)
                .Include(cs => cs.Fields)
                .ThenInclude(sf => sf.Options);

            return response.FirstOrDefault();
        }

        public CaseStatus CreateCaseStatus(CreateCaseStatusRequest request)
        {

            var statusType = _databaseContext.CaseStatusTypes
                .FirstOrDefault(f => f.Name == request.Type);

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

            return caseStatus.ToDomain();
        }

        public CaseStatus UpdateCaseStatus(long caseStatusId, UpdateCaseStatusRequest request)
        {
            var caseStatus = _databaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatusId);
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {caseStatusId} not found");
            }

            if (request.EndDate != null)
            {
                caseStatus.EndDate = request.EndDate;
            }

            _databaseContext.SaveChanges();

            return caseStatus.ToDomain();
        }
    }
}
