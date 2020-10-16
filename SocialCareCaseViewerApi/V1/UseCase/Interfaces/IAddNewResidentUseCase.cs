using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IAddNewResidentUseCase
    {
        public AddNewResidentResponse Execute(AddNewResidentRequest request);
    }
}
