using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetAllUseCase : IGetAllUseCase
    {
        private IDatabaseGateway _databaseGateway;
        private IMosaicAPIGateway _mosaicAPIGateway;
        public GetAllUseCase(IDatabaseGateway databaseGateway, IMosaicAPIGateway mosaicAPIGateway)
        {
            _databaseGateway = databaseGateway;
            _mosaicAPIGateway = mosaicAPIGateway;
        }

        public ResidentInformationList Execute(ResidentQueryParam rqp, int cursor, int limit)
        {
            //check mosaic id                        
            if (!string.IsNullOrWhiteSpace(rqp.MosaicId) && rqp.MosaicId.ToUpper().StartsWith("NC"))
<<<<<<< HEAD
            {
                string mosaicID = _databaseGateway.GetPersonIdByNCReference(rqp.MosaicId.ToUpper());

                if (!string.IsNullOrEmpty(mosaicID))
                {
                    rqp.MosaicId = mosaicID;
=======
            {
                string mosaicID = _databaseGateway.GetPersonIdByNCReference(rqp.MosaicId.ToUpper());

                if (!string.IsNullOrEmpty(mosaicID))
                {
                    rqp.MosaicId = mosaicID;
                }
            }

            if (!string.IsNullOrEmpty(rqp.MosaicId))
            {
                //ensure we have valid mosaic id, otherwise return no results
                if (!Int64.TryParse(rqp.MosaicId, out _))
                {
                    return new ResidentInformationList() { Residents = new List<ResidentInformation>() };
>>>>>>> 0de0d2242771dbb76fc74f838a7172d367007f53
                }
            }

            return _mosaicAPIGateway.GetResidents(rqp, cursor, limit);
        }
    }
}
