using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public interface IProcessDataUseCase
    {
        GetProcessDataResponse Execute(long? mosaicId, string officerEmail);
    }
}
