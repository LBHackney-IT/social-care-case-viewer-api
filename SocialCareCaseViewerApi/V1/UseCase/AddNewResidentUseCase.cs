using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class AddNewResidentUseCase : IAddNewResidentUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public AddNewResidentUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }
        public AddNewResidentResponse Execute(AddNewResidentRequest request)
        {
            return _databaseGateway.AddNewResident(request);
        }
    }
}
