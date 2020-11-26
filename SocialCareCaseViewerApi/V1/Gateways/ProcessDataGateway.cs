using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            List<BsonDocument> result;
            long mosaicId = 0;
            if (!string.IsNullOrEmpty(request.MosaicId))
            {
                if (!Int64.TryParse(request.MosaicId, out mosaicId))
                {
                    throw new Exception();
                }
                var filter = "{$or: [{ mosaic_id: " + mosaicId.ToString() + "}, { mosaic_id: /" + mosaicId.ToString() + "/}]}";

                result = _sccvDbContext.getCollection().Find(filter).ToList();
            }
            else
            {
                var firstNameFilter = !string.IsNullOrWhiteSpace(request.FirstName) ?
                    Builders<BsonDocument>.Filter.Regex("first_name", BsonRegularExpression.Create(new Regex(request.FirstName, RegexOptions.IgnoreCase))) : null;
                var lastNameFilter = !string.IsNullOrWhiteSpace(request.LastName) ?
                    Builders<BsonDocument>.Filter.Regex("last_name", BsonRegularExpression.Create(new Regex(request.LastName, RegexOptions.IgnoreCase))) : null;
                var workerEmailFilter = !string.IsNullOrWhiteSpace(request.WorkerEmail) ?
                    Builders<BsonDocument>.Filter.Regex("worker_email", BsonRegularExpression.Create(new Regex(request.WorkerEmail, RegexOptions.IgnoreCase))) : null;
                var caseNoteTypeFilter = !string.IsNullOrWhiteSpace(request.CaseNoteType) ?
                    Builders<BsonDocument>.Filter.Regex("case_note_type", BsonRegularExpression.Create(new Regex(request.CaseNoteType, RegexOptions.IgnoreCase))) : null;

                var query = _sccvDbContext.getCollection().AsQueryable();

                if (firstNameFilter != null) query = query.Where(db => firstNameFilter.Inject());
                if (lastNameFilter != null) query = query.Where(db => lastNameFilter.Inject());
                if (workerEmailFilter != null) query = query.Where(db => workerEmailFilter.Inject());
                if (caseNoteTypeFilter != null) query = query.Where(db => caseNoteTypeFilter.Inject());


                result = query.ToList();
            }
            //if document does not exist in the DB, then thrown a corresponsing error.
            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }
            return ResponseFactory.ToResponse(result);
        }

        public IEnumerable<CareCaseData> GetProcessData(long mosaicId, string firstName, string lastName, string officerEmail, string caseNoteType)
        {
            List<BsonDocument> result;
            if (mosaicId != 0)
            {

                var filter = "{$or: [{ mosaic_id: " + mosaicId.ToString() + "}, { mosaic_id: /" + mosaicId.ToString() + "/}]}";

                result = _sccvDbContext.getCollection().Find(filter).ToList();
            }
            else
            {
                var firstNameFilter = !string.IsNullOrWhiteSpace(firstName) ?
                    Builders<BsonDocument>.Filter.Regex("first_name", BsonRegularExpression.Create(new Regex(firstName, RegexOptions.IgnoreCase))) : null;
                var lastNameFilter = !string.IsNullOrWhiteSpace(lastName) ?
                    Builders<BsonDocument>.Filter.Regex("last_name", BsonRegularExpression.Create(new Regex(lastName, RegexOptions.IgnoreCase))) : null;
                var officerEmailFilter = !string.IsNullOrWhiteSpace(officerEmail) ?
                    Builders<BsonDocument>.Filter.Eq("worker_email", BsonRegularExpression.Create(new Regex(officerEmail, RegexOptions.IgnoreCase))) : null;
                var caseNoteTypeFilter = !string.IsNullOrWhiteSpace(caseNoteType) ?
                    Builders<BsonDocument>.Filter.Regex("case_note_type", BsonRegularExpression.Create(new Regex(caseNoteType, RegexOptions.IgnoreCase))) : null;

                var query = _sccvDbContextTemp.getCollection().AsQueryable();

                if (firstNameFilter != null) query = query.Where(db => firstNameFilter.Inject());
                if (lastNameFilter != null) query = query.Where(db => lastNameFilter.Inject());
                if (officerEmailFilter != null) query = query.Where(db => officerEmailFilter.Inject());
                if (caseNoteTypeFilter != null) query = query.Where(db => caseNoteTypeFilter.Inject());

                result = query.ToList();
            }
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
