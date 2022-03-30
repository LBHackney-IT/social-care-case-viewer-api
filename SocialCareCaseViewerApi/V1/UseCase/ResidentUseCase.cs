using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ResidentUseCase : IResidentUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public ResidentUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public AddNewResidentResponse AddNewResident(AddNewResidentRequest request)
        {
            return _databaseGateway.AddNewResident(request);
        }

        public GetPersonResponse? GetResident(GetPersonRequest request)
        {
            var resident = _databaseGateway.GetPersonDetailsById(request.Id);

            return resident != null ? ResponseFactory.ToResponse(resident) : null;
        }

        public void PatchResident(PatchPersonRequest request)
        {
            _databaseGateway.PatchPerson(request);
        }

        public void UpdateResident(UpdatePersonRequest request)
        {
            _databaseGateway.UpdatePerson(request);
        }

        public ResidentInformationList GetResidentsByQuery(ResidentQueryParam rqp, int cursor, int limit)
        {
            long? mosaicId = null;
            int totalCount = 0;
            List<ResidentInformation> matchingResidents = new List<ResidentInformation>();

            if (!string.IsNullOrEmpty(rqp.MosaicId))
            {
                //remove prefix and other letters
                rqp.MosaicId = Regex.Replace(rqp.MosaicId, "[^0-9.]", "");
                mosaicId = long.TryParse(rqp.MosaicId, out long tmp) ? (long?) tmp : null;

                if (mosaicId == null)
                {
                    return new ResidentInformationList() { Residents = new List<ResidentInformation>() };
                }
            }

            List<ResidentInformation> residents = new List<ResidentInformation>();

            //if Mosaic ID is provided, use that as a search criteria ignoring other parameters
            if (mosaicId != null)
            {
                //check for individual. Ignore if provided id contains leading zeros as those won't be system Ids
                if (!rqp.MosaicId.StartsWith("0"))
                {
                    var resident = _databaseGateway.GetPersonDetailsById(mosaicId.Value);

                    if (resident != null)
                    {
                        residents.Add(resident.ToResidentInformationResponse());
                    }
                }

                //check for matching emergency Ids
                //This enables overlapping Mosaic and emergency IDs to be included in the results set
                var residentsWithMatchingEmergencyIds = _databaseGateway.GetPersonIdsByEmergencyId(rqp.MosaicId);

                //if IDs found, get the person records
                if (residentsWithMatchingEmergencyIds.Any())
                {
                    residents.AddRange(
                        _databaseGateway.GetPersonsByListOfIds(residentsWithMatchingEmergencyIds)
                        .Select(x => x.ToResidentInformationResponse())
                        .ToList());
                }

                totalCount = residents.Count;
            }
            else
            {
                limit = limit < 10 ? 10 : limit;
                limit = limit > 100 ? 100 : limit;

                (matchingResidents, totalCount) = _databaseGateway.GetResidentsBySearchCriteria(
                    cursor: cursor,
                    limit: limit,
                    id: mosaicId,
                    firstName: rqp.FirstName,
                    lastName: rqp.LastName,
                    dateOfBirth: rqp.DateOfBirth,
                    postcode: rqp.Postcode,
                    address: rqp.Address,
                    contextFlag: rqp.ContextFlag);


                residents.AddRange(matchingResidents);
            }

            var nextCursor = residents.Count == limit ? residents.Max(r => long.Parse(r.MosaicId)).ToString() : "";

            return new ResidentInformationList
            {
                Residents = residents,
                NextCursor = nextCursor,
                TotalCount = totalCount
            };
        }
    }
}
