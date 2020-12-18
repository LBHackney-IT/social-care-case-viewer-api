using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class ResponseFactory
    {
       

        public static AddNewResidentResponse ToResponse(this Person resident, AddressDomain address)
        {
            return new AddNewResidentResponse
            {
                PersonId = resident.Id,
                Title = resident.Title,
                FirstName = resident.FirstName,
                LastName = resident.LastName,
                Gender = resident.Gender,
                Nationality = resident.Nationality,
                NhsNumber = resident.NhsNumber,
                DateOfBirth = resident.DateOfBirth,
                AgeGroup = resident.AgeContext,
                Address = address
            };
        }

        public static List<CareCaseData> ToResponse(List<BsonDocument> submittedFormsData)
        {
            return submittedFormsData.Select(x => x.ToResponse()).ToList();
        }

        private static CareCaseData ToResponse(this BsonDocument formData)
        {
            var caseData = BsonSerializer.Deserialize<FormData>(formData);
            return caseData.ToResponse(formData);
        }
        private static CareCaseData ToResponse(this FormData formData, BsonDocument rawData)
        {
            var dummyFormData = formData;
            return new CareCaseData
            {
                RecordId = formData.RecordId,
                FirstName = formData.FirstName,
                LastName = formData.LastName,
                DateOfBirth = formData.DateOfBirth,
                OfficerEmail = formData.OfficerEmail,
                FormName = formData.FormName,
                CaseFormUrl = formData.CaseFormUrl,
                CaseFormTimestamp = formData.CaseFormTimestamp,
                DateOfEvent = formData.DateOfEvent,
                PersonId = formData.PersonId,
                CaseFormData = rawData.ToJson()
            };
        }
    }
}
