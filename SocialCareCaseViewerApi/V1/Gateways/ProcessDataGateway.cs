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
        private ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;

        public ProcessDataGateway(ISccvDbContext sccvDbContext, ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway)
        {
            _sccvDbContext = sccvDbContext;
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public Tuple<IEnumerable<CareCaseData>, int> GetProcessData(ListCasesRequest request, string ncId)
        {
            List<BsonDocument> result;
            FilterDefinition<BsonDocument> firstNameFilter;
            FilterDefinition<BsonDocument> lastNameFilter;

            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                var mosaicIdQuery = _sccvDbContext.getCollection().AsQueryable();
                var mosaicIDFilter = Builders<BsonDocument>.Filter.Regex("mosaic_id", new BsonRegularExpression("^" + request.MosaicId + "$", "i"));
                mosaicIdQuery = mosaicIdQuery.Where(db => mosaicIDFilter.Inject());

                result = mosaicIdQuery.ToList();

                if (!string.IsNullOrWhiteSpace(ncId))
                {
                    //add records that are still using nc ID to the results
                    var ncIdQuery = _sccvDbContext.getCollection().AsQueryable();
                    var ncIdFilter = Builders<BsonDocument>.Filter.Regex("mosaic_id", new BsonRegularExpression("^" + ncId + "$", "i"));
                    ncIdQuery = ncIdQuery.Where(db => ncIdFilter.Inject());

                    result.AddRange(ncIdQuery.ToList());
                }

                //add historical case notes to the case history records when using mosaic id search
                var showHistoricData = Environment.GetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA");

                if (showHistoricData != null && showHistoricData.Equals("true"))
                {
                    //fail silently for now until platform API has been finalised
                    //TOOD: please do not use as production code
                    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Remove when production ready")]
                    try
                    {
                        var notesResponse = _socialCarePlatformAPIGateway.GetCaseNotesByPersonId(request.MosaicId);
                        if (notesResponse.CaseNotes.Count > 0)
                        {
                            result.AddRange(ResponseFactory.HistoricalCaseNotesToDomain(notesResponse.CaseNotes));
                        }
                    }
                    catch { }

                    //add historical visits to the case history records when using mosaic id search
                    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Remove when production ready")]
                    try
                    {
                        var visitsResponse = _socialCarePlatformAPIGateway.GetVisitsByPersonId(request.MosaicId);

                        if (visitsResponse.Visits.Count > 0)
                        {
                            result.AddRange(ResponseFactory.HistoricalVisitsToDomain(visitsResponse.Visits));
                        }
                    }
                    catch { }
                }
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

            response = SortData(request.SortBy, request.OrderBy, response)
                .Skip(request.Cursor)
                .Take(request.Limit)
                .ToList();

            return new Tuple<IEnumerable<CareCaseData>, int>(response, totalCount);
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
