using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public interface IProcessDataUseCase
    {
        CareCaseDataList Execute(ListCasesRequest request);
        CareCaseDataList Execute(long mosaicId, string firstName, string lastName, string officerEmail, string caseNoteType,
            DateTime? providedStartDate, DateTime? providedEndDate);
        Task<string> Execute(CaseNotesDocument caseNotesDoc);
    }
}
