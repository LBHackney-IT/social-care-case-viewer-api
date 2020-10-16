using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Domain.Address;
using AddressResponse = SocialCareCaseViewerApi.V1.Boundary.Response.Address;
using DbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using ResidentInformationResponse = SocialCareCaseViewerApi.V1.Boundary.Response.ResidentInformation;

namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static ResidentInformationResponse ToResponse(this ResidentInformation domain)
        {
            return new ResidentInformationResponse
            {
                PersonId = domain.PersonId,
                Title = domain.Title,
                FirstName = domain.FirstName,
                LastName = domain.LastName,
                DateOfBirth = domain.DateOfBirth,
                Gender = domain.Gender,
                Nationality = domain.Nationality,
                AddressList = domain.AddressList?.ToResponse(),
                NhsNumber = domain.NhsNumber
            };
        }
        public static List<ResidentInformationResponse> ToResponse(this IEnumerable<ResidentInformation> people)
        {
            return people.Select(p => p.ToResponse()).ToList();
        }

        private static List<AddressResponse> ToResponse(this List<Address> addresses)
        {
            return addresses.Select(add => new AddressResponse
            {
                AddressLine1 = add.AddressLine1,
                AddressLine2 = add.AddressLine2,
                AddressLine3 = add.AddressLine3,
                PostCode = add.PostCode
            }).ToList();
        }

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
            return new CareCaseData
            {
                FirstName = formData.FirstName,
                LastName = formData.LastName,
                DateOfBirth = formData.DateOfBirth,
                OfficerEmail = formData.OfficerEmail,
                FormName = formData.FormName,
                CaseFormUrl = formData.CaseFormUrl,
                PersonId = formData.PersonId,
                CaseFormData = rawData
            };
        }
    }
}
