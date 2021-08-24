using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;
using CaseStatusField = SocialCareCaseViewerApi.V1.Domain.CaseStatusField;
using CaseStatusFieldSelectedOption = SocialCareCaseViewerApi.V1.Domain.CaseStatusFieldSelectedOption;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class CaseStatusGateway : ICaseStatusGateway
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IProcessDataGateway _processDataGateway;
        private readonly ISystemTime _systemTime;

        public CaseStatusGateway(
            DatabaseContext databaseContext,
            IProcessDataGateway processDataGateway,
            ISystemTime systemTime
        )
        {
            _databaseContext = databaseContext;
            _processDataGateway = processDataGateway;
            _systemTime = systemTime;
        }

        public List<CaseStatus> GetCaseStatusesByPersonId(long personId)
        {
            IEnumerable<Infrastructure.CaseStatus> caseStatuses = _databaseContext.CaseStatuses
                .Where(cs => cs.PersonId == personId)
                .Where(cs => cs.EndDate == null || cs.EndDate > DateTime.Today)
                .Include(cs => cs.Type)
                .Include(cs => cs.SelectedOptions)
                .ThenInclude(csso => csso.FieldOption)
                .ThenInclude(fo => fo.TypeField);

            return caseStatuses.Select(cs => new CaseStatus
            {
                Id = cs.Id,
                Type = cs.Type.Name,
                StartDate = cs.StartDate.ToString("s"),
                EndDate = cs.EndDate?.ToString("s"),
                Notes = cs.Notes,
                Fields = cs.SelectedOptions.Select(o => new CaseStatusField
                {
                    Name = o.FieldOption.TypeField.Name,
                    Description = o.FieldOption.TypeField.Description,
                    SelectedOption = new CaseStatusFieldSelectedOption
                    {
                        Name = o.FieldOption.Name,
                        Description = o.FieldOption.Description
                    }
                }).ToList()
            }).ToList();
        }
    }
}
