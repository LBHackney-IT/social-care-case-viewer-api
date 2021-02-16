using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;


namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class ProcessDataGateway : IProcessDataGateway
    {
        private ISccvDbContext _sccvDbContext;
        private DatabaseContext _databaseContext;

        public ProcessDataGateway(ISccvDbContext sccvDbContext, DatabaseContext databaseContext)
        {
            _sccvDbContext = sccvDbContext;
            _databaseContext = databaseContext;
        }
        public Tuple<IEnumerable<CareCaseData>, int> GetProcessData(ListCasesRequest request)
        {
            List<BsonDocument> result;
            FilterDefinition<BsonDocument> firstNameFilter;
            FilterDefinition<BsonDocument> lastNameFilter;

            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                var query = _sccvDbContext.getCollection().AsQueryable();
                var mosaicIDFilter = Builders<BsonDocument>.Filter.Regex("mosaic_id", new BsonRegularExpression("^" + request.MosaicId + "$", "i"));
                query = query.Where(db => mosaicIDFilter.Inject());

                result = query.ToList();

                //grab details from person data, so person record history can be displayed on the case notes 
                //TODO: add support for modified data
                Person person = _databaseContext.Persons.FirstOrDefault(x => x.Id == Convert.ToInt64(request.MosaicId));

                result.Add(new BsonDocument(
                        new List<BsonElement>
                        {
                            new BsonElement("worker_email", person.CreatedBy),
                            new BsonElement("timestamp", person.CreatedAt.ToString()),
                            new BsonElement("form_name_overall", "API_Audit_Person_Created"),
                            new BsonElement("form_name", "Person added")
                        }
                    ));
            }
            else
            {
                if (!request.ExactNameMatch)
                {
                    firstNameFilter = !string.IsNullOrWhiteSpace(request.FirstName) ?
                        Builders<BsonDocument>.Filter.Regex("first_name", BsonRegularExpression.Create(new Regex(request.FirstName, RegexOptions.IgnoreCase))) : null;
                    lastNameFilter = !string.IsNullOrWhiteSpace(request.LastName) ?
                        Builders<BsonDocument>.Filter.Regex("last_name", BsonRegularExpression.Create(new Regex(request.LastName, RegexOptions.IgnoreCase))) : null;
                }
                else
                {
                    firstNameFilter = !string.IsNullOrWhiteSpace(request.FirstName) ?
                        Builders<BsonDocument>.Filter.Regex("first_name", new BsonRegularExpression("^" + request.FirstName + "$", "i")) : null;
                    lastNameFilter = !string.IsNullOrWhiteSpace(request.LastName) ?
                        Builders<BsonDocument>.Filter.Regex("last_name", new BsonRegularExpression("^" + request.LastName + "$", "i")) : null;
                }
                var workerEmailFilter = !string.IsNullOrWhiteSpace(request.WorkerEmail) ?
                    Builders<BsonDocument>.Filter.Regex("worker_email", BsonRegularExpression.Create(new Regex(request.WorkerEmail, RegexOptions.IgnoreCase))) : null;
                var caseNoteTypeFilter = !string.IsNullOrWhiteSpace(request.FormName) ?
                    Builders<BsonDocument>.Filter.Regex("form_name", BsonRegularExpression.Create(new Regex(request.FormName, RegexOptions.IgnoreCase))) : null;

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

            var response = ResponseFactory
                .ToResponse(result);

            if (request.StartDate != null)
            {
                if (DateTime.TryParseExact(request.StartDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    response = response
                        .Where(x =>
                        {
                            if (DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                return date.Date >= startDate.Date;
                            }
                            else
                            {
                                return false;
                            }
                        })
                        .ToList();
                }
            }

            if (request.EndDate != null)
            {
                if (DateTime.TryParseExact(request.EndDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
                {
                    response = response
                        .Where(x =>
                        {
                            if (DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                            {
                                return date.Date <= endDate.Date;
                            }
                            else
                            {
                                return false;
                            }
                        })
                        .ToList();
                }
            }

            int totalCount = response.Count;

            //sort by date of event by default, then by datestamp
            response = response
                .OrderByDescending(x =>
                {
                    _ = DateTime.TryParseExact(x.DateOfEvent, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                    return dt;
                })
                .ThenByDescending(x =>
                {
                    _ = DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                    return dt;
                })
                .Skip(request.Cursor)
                .Take(request.Limit)
                .ToList();

            return new Tuple<IEnumerable<CareCaseData>, int>(response, totalCount);
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
