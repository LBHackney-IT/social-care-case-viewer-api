using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class ProcessDataGateway : IProcessDataGateway
    {
        private ISccvDbContext _sccvDbContext;
        private ISccvDbContextTemp _sccvDbContextTemp;

        public ProcessDataGateway(ISccvDbContext sccvDbContext, ISccvDbContextTemp sccvDbContextTemp)
        {
            _sccvDbContext = sccvDbContext;
            _sccvDbContextTemp = sccvDbContextTemp;
        }
        public IEnumerable<CareCaseData> GetProcessData(ListCasesRequest request)
        {
            long mosaicId = 0;
            if (!string.IsNullOrEmpty(request.MosaicId))
            {

                if (!Int64.TryParse(request.MosaicId, out mosaicId))
                {
                    throw new Exception();
                }
            }
            var officerEmailFilter = !string.IsNullOrWhiteSpace(request.WorkerEmail) ?
                Builders<BsonDocument>.Filter.Eq("worker_email", request.WorkerEmail) : null;
            var mosaicIdFilter = !string.IsNullOrWhiteSpace(request.MosaicId) ?
                Builders<BsonDocument>.Filter.Eq("mosaic_id", mosaicId.ToString()) : null;
            var firstNameFilter = !string.IsNullOrWhiteSpace(request.FirstName) ?
                Builders<BsonDocument>.Filter.Eq("first_name", request.FirstName) : null;
            var lastNameFilter = !string.IsNullOrWhiteSpace(request.LastName) ?
                Builders<BsonDocument>.Filter.Regex("last_name".ToUpper(), request.LastName.ToUpper()) : null;
            var dateOfBirthFilter = !string.IsNullOrWhiteSpace(request.DateOfBirth) ?
                Builders<BsonDocument>.Filter.Eq("date_of_birth".ToString(), request.DateOfBirth) : null;
            var postCodeFilter = !string.IsNullOrWhiteSpace(request.Postcode) ?
                Builders<BsonDocument>.Filter.Regex("postcode".ToUpper(), request.Postcode.ToUpper()) : null;

            var result = _sccvDbContext
                .getCollection()
                .AsQueryable()
                // .Where(db =>
                //     string.IsNullOrEmpty(request.WorkerEmail) || officerEmailFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(request.MosaicId) || mosaicIdFilter.Inject())
                .Where(db => firstNameFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(request.LastName) || lastNameFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(request.DateOfBirth) || dateOfBirthFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(request.Postcode) || postCodeFilter.Inject())
                .ToList();
            //if document does not exist in the DB, then thrown a corresponsing error.
            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }
            return ResponseFactory.ToResponse(result);
        }

        public IEnumerable<CareCaseData> GetProcessData(long? mosaicId, string officerEmail, string firstName, string lastName, string dateOfBirth, string postcode)
        {
            var officerEmailFilter = !string.IsNullOrWhiteSpace(officerEmail) ?
                Builders<BsonDocument>.Filter.Eq("worker_email", officerEmail) : null;
            var mosaicIdFilter = mosaicId == null ?
                Builders<BsonDocument>.Filter.Eq("mosaic_id", mosaicId.ToString()) : null;
            var firstNameFilter = !string.IsNullOrWhiteSpace(firstName) ?
                Builders<BsonDocument>.Filter.Eq("first_name", firstName) : null;
            var lastNameFilter = !string.IsNullOrWhiteSpace(lastName) ?
                Builders<BsonDocument>.Filter.Regex("last_name".ToUpper(), lastName.ToUpper()) : null;
            var dateOfBirthFilter = !string.IsNullOrWhiteSpace(dateOfBirth) ?
                Builders<BsonDocument>.Filter.Eq("date_of_birth".ToString(), dateOfBirth) : null;
            var postCodeFilter = !string.IsNullOrWhiteSpace(postcode) ?
                Builders<BsonDocument>.Filter.Regex("postcode".ToUpper(), postcode.ToUpper()) : null;

            var result = _sccvDbContextTemp
                .getCollection()
                .AsQueryable()
                // .Where(db =>
                //     string.IsNullOrEmpty(officerEmail) || officerEmailFilter.Inject())
                // .Where(db =>
                //     mosaicId == null || mosaicIdFilter.Inject())
                .Where(db => firstNameFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(lastName) || lastNameFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(dateOfBirth) || dateOfBirthFilter.Inject())
                // .Where(db =>
                //     string.IsNullOrEmpty(postcode) || postCodeFilter.Inject())
                .ToList();

            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }
            return ResponseFactory.ToResponse(result);
        }

        public async Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc)
        {
            var doc = BsonSerializer.Deserialize<BsonDocument>(caseNotesDoc.CaseFormData);
            await _sccvDbContext.getCollection().InsertOneAsync(doc)
                .ConfigureAwait(false);
            return doc["_id"].AsObjectId.ToString();
        }
    }
}
