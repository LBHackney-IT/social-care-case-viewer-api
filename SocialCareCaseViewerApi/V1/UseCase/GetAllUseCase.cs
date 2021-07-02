using System;
using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetAllUseCase : IGetAllUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public GetAllUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ResidentInformationList Execute(ResidentQueryParam rqp, int cursor, int limit)
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

            if (!string.IsNullOrEmpty(rqp.MosaicId))
            {
                //ensure we have valid mosaic id, otherwise return no results
                if (!long.TryParse(rqp.MosaicId, out _))
                {
                    return new ResidentInformationList() { Residents = new List<ResidentInformation>() };
                }
            }

            limit = limit < 10 ? 10 : limit;
            limit = limit > 100 ? 100 : limit;

            long? mosaicId = rqp.MosaicId != null ? Convert.ToInt64(rqp.MosaicId) : (long?)null;

            var residents = _databaseGateway.GetResidentsBySearchCriteria(
                cursor: cursor,
                limit: limit,
                id: mosaicId,
                firstName: rqp.FirstName,
                lastName: rqp.LastName,
                dateOfBirth: rqp.DateOfBirth,
                postcode: rqp.Postcode,
                address: rqp.Address,
                contextFlag: rqp.ContextFlag);

            var nextCursor = residents.Count == limit ? residents.Max(r => long.Parse(r.MosaicId)).ToString() : "";

            return new ResidentInformationList
            {
                Residents = residents,
                NextCursor = nextCursor
            };
        }
    }
}
