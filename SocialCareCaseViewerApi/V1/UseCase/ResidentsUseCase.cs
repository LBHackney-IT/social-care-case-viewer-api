using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ResidentsUseCase : IResidentsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMosaicAPIGateway _mosaicAPIGateway;

        public ResidentsUseCase(IDatabaseGateway databaseGateway, IMosaicAPIGateway mosaicAPIGateway)
        {
            _databaseGateway = databaseGateway;
            _mosaicAPIGateway = mosaicAPIGateway;
        }

        public GetPersonResponse? ExecuteGet(long id)
        {
            var person = _databaseGateway.GetPersonDetailsById(id);

            return person?.ToResponse();
        }

        public ResidentInformationList ExecuteGetAll(ResidentQueryParam rqp, int cursor, int limit)
        {
            //check mosaic id
            if (!string.IsNullOrWhiteSpace(rqp.MosaicId) && rqp.MosaicId.ToUpper().StartsWith("NC"))
            {
                string mosaicID = _databaseGateway.GetPersonIdByNCReference(rqp.MosaicId.ToUpper());

                if (!string.IsNullOrEmpty(mosaicID))
                {
                    rqp.MosaicId = mosaicID;
                }
            }

            if (string.IsNullOrEmpty(rqp.MosaicId)) return _mosaicAPIGateway.GetResidents(rqp, cursor, limit);
            //ensure we have valid mosaic id, otherwise return no results
            if (!long.TryParse(rqp.MosaicId, out _))
            {
                return new ResidentInformationList() { Residents = new List<ResidentInformation>() };
            }

            return _mosaicAPIGateway.GetResidents(rqp, cursor, limit);
        }

        public AddNewResidentResponse ExecutePost(AddNewResidentRequest request)
        {
            return _databaseGateway.AddNewResident(request);
        }

        public void ExecutePatch(UpdatePersonRequest request)
        {
            _databaseGateway.UpdatePerson(request);
        }
    }
}
