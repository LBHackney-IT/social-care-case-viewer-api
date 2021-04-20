using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IVisitsUseCase
    {
        List<Visit> ExecuteGetByPersonId(string personId);
    }
}
