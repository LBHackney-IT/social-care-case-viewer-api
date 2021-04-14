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
        private readonly ISccvDbContext _sccvDbContext;
        private readonly ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;

        public ProcessDataGateway(ISccvDbContext sccvDbContext, ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway)
        {
            _sccvDbContext = sccvDbContext;
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
        }

        public Tuple<IEnumerable<CareCaseData>, int> GetProcessData(ListCasesRequest request, string ncId)
        {
            var result = new List<BsonDocument>();
            FilterDefinition<BsonDocument> firstNameFilter;
            FilterDefinition<BsonDocument> lastNameFilter;

            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                result.AddRange(GetHistoricRecordsByPersonId(request.MosaicId, ncId));
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
                            return false;
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
                            return false;
                        })
                        .ToList();
                }
            }

            var totalCount = response.Count;

            response = SortData(request.SortBy, request.OrderBy, response)
                .Skip(request.Cursor)
                .Take(request.Limit)
                .ToList();

            return new Tuple<IEnumerable<CareCaseData>, int>(response, totalCount);
        }

        private IEnumerable<BsonDocument> GetHistoricRecordsByPersonId(string personId, string ncId)
        {
            var mosaicIdQuery = _sccvDbContext.getCollection().AsQueryable();
            var mosaicIDFilter = Builders<BsonDocument>.Filter.Regex("mosaic_id", new BsonRegularExpression("^" + personId + "$", "i"));
            mosaicIdQuery = mosaicIdQuery.Where(db => mosaicIDFilter.Inject());

            var casesAndVisits = mosaicIdQuery.ToList();

            if (!string.IsNullOrWhiteSpace(ncId))
            {
                //add records that are still using nc ID to the results
                var ncIdQuery = _sccvDbContext.getCollection().AsQueryable();
                var ncIdFilter = Builders<BsonDocument>.Filter.Regex("mosaic_id", new BsonRegularExpression("^" + ncId + "$", "i"));
                ncIdQuery = ncIdQuery.Where(db => ncIdFilter.Inject());

                casesAndVisits.AddRange(ncIdQuery.ToList());
            }

            var historicRecords = _socialCarePlatformAPIGateway.GetHistoricCaseNotesAndVisitsByPersonId(long.Parse(personId));
            if (historicRecords.Count > 0)
            {
                casesAndVisits.AddRange(ConvertHistoricRecordsToDomain(historicRecords));
            }

            return casesAndVisits;
        }

        private static IEnumerable<BsonDocument> ConvertHistoricRecordsToDomain(List<ResidentHistoricRecord> residentHistoricRecords)
        {
            var convertedHistoricResponse = new List<BsonDocument>();
            var showHistoricData = Environment.GetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA");

            foreach (var residentHistoricRecord in residentHistoricRecords)
            {
                // feature flag is for historic visits
                if (showHistoricData is "true" && residentHistoricRecord.RecordType == RecordType.Visit)
                {
                    convertedHistoricResponse.Add(ResponseFactory.HistoricalVisitsToDomain(residentHistoricRecord as ResidentHistoricRecordVisit));
                }
                if (residentHistoricRecord.RecordType == RecordType.CaseNote)
                {
                    convertedHistoricResponse.Add(ResponseFactory.HistoricalCaseNotesToDomain(residentHistoricRecord as ResidentHistoricRecordCaseNote));
                }
            }

            return convertedHistoricResponse;
        }

        public IOrderedEnumerable<CareCaseData> SortData(string sortBy, string orderBy, List<CareCaseData> response)
        {
            switch (sortBy)
            {
                case "firstName":
                    return (orderBy == "asc") ?
                        response.OrderBy(x => x.FirstName) :
                        response.OrderByDescending(x => x.FirstName);
                case "lastName":
                    return (orderBy == "asc") ?
                        response.OrderBy(x => x.LastName) :
                        response.OrderByDescending(x => x.LastName);
                case "caseFormUrl":
                    return (orderBy == "asc") ?
                        response.OrderBy(x => x.CaseFormUrl) :
                        response.OrderByDescending(x => x.CaseFormUrl);
                case "dateOfBirth":
                    return (orderBy == "asc") ?
                        response.OrderBy(x =>
                        {
                            _ = DateTime.TryParseExact(x.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                            return dt;
                        }) :
                        response.OrderByDescending(x =>
                        {
                            _ = DateTime.TryParseExact(x.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt);
                            return dt;
                        });
                case "officerEmail":
                    return (orderBy == "asc") ?
                        response.OrderBy(x => x.OfficerEmail) :
                        response.OrderByDescending(x => x.OfficerEmail);
                default:

                    return (orderBy == "asc") ?
                        response.OrderBy(x =>
                            {
                                return GetDateToSortBy(x);
                            }
                        ) :
                        response.OrderByDescending(x =>
                            {
                                return GetDateToSortBy(x);
                            }
                        );
            }

            static DateTime? GetDateToSortBy(CareCaseData x)
            {
                DateTime? dt = null;

                if (string.IsNullOrEmpty(x.DateOfEvent))
                {
                    bool success = DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeStamp);
                    if (success) dt = timeStamp;
                }
                else
                {
                    bool success = DateTime.TryParseExact(x.DateOfEvent, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOfEvent);
                    if (success) dt = dateOfEvent;
                }
                return dt;
            }
        }

        public async Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc)
        {
            var doc = BsonSerializer.Deserialize<BsonDocument>(caseNotesDoc.CaseFormData);
            await _sccvDbContext.getCollection().InsertOneAsync(doc)
                .ConfigureAwait(false);
            return doc["_id"].AsObjectId.ToString();
        }

        public CareCaseData GetCaseById(string recordId)
        {
            var collection = _sccvDbContext.getCollection();
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(recordId));
            var query = collection.AsQueryable().Where(db => filter.Inject());

            var result = query.ToList();
            if (result.FirstOrDefault() == null) throw new DocumentNotFoundException("Search did not return any results");

            var response = ResponseFactory.ToResponse(result).FirstOrDefault();

            return response;
        }
    }
}
