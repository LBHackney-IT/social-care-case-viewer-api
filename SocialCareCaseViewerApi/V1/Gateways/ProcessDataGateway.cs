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
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
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

        public Tuple<IEnumerable<CareCaseData>, int> GetProcessData(ListCasesRequest request, string? ncId)
        {
            var result = new List<BsonDocument>();
            FilterDefinition<BsonDocument>? firstNameFilter;
            FilterDefinition<BsonDocument>? lastNameFilter;

            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                var historicRecords = GetHistoricRecordsByPersonId(request.MosaicId, ncId).ToList();
                if (historicRecords.Count > 0)
                {
                    result.AddRange(historicRecords);
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
            //if document does not exist in the DB, then thrown a corresponding error.
            if (result == null)
            {
                throw new DocumentNotFoundException("document not found");
            }

            var response = result.ToResponse();

            if (request.StartDate != null)
            {
                if (DateTime.TryParseExact(((DateTime)request.StartDate).ToString(CultureInfo.InvariantCulture), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
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
                if (DateTime.TryParseExact(((DateTime) request.EndDate).ToString(CultureInfo.InvariantCulture), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
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

        private IEnumerable<BsonDocument> GetHistoricRecordsByPersonId(string personId, string? ncId)
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

            var historicVisits = new List<BsonDocument>();
            try
            {
                var visits = _socialCarePlatformAPIGateway
                    .GetVisitsByPersonId(personId)
                    .ToList();

                if (visits.Count > 0)
                {
                    historicVisits = visits
                        .Select(ResponseFactory.HistoricalVisitsToDomain)
                        .ToList();
                }

            }
            catch (NullReferenceException)
            {
                historicVisits = new List<BsonDocument>();
            }
            catch (SocialCarePlatformApiException)
            {
                historicVisits = new List<BsonDocument>();
            }

            var historicCases = new List<BsonDocument>();
            try
            {
                var caseNotes = _socialCarePlatformAPIGateway
                    .GetCaseNotesByPersonId(personId)
                    .CaseNotes;

                if (caseNotes?.Count > 0)
                {
                    historicCases = caseNotes
                        .Select(ResponseFactory.HistoricalCaseNotesToDomain)
                        .ToList();
                }
            }
            catch (NullReferenceException)
            {
                historicCases = new List<BsonDocument>();
            }
            catch (SocialCarePlatformApiException)
            {
                historicCases = new List<BsonDocument>();
            }

            if (historicVisits.Count > 0)
            {
                casesAndVisits.AddRange(historicVisits);
            }
            if (historicCases.Count > 0)
            {
                casesAndVisits.AddRange(historicCases);
            }

            return casesAndVisits;
        }

        public IOrderedEnumerable<CareCaseData> SortData(string sortBy, string orderBy, List<CareCaseData> response)
        {
            return sortBy switch
            {
                "firstName" => (orderBy == "asc")
                    ? response.OrderBy(x => x.FirstName)
                    : response.OrderByDescending(x => x.FirstName),
                "lastName" => (orderBy == "asc")
                    ? response.OrderBy(x => x.LastName)
                    : response.OrderByDescending(x => x.LastName),
                "caseFormUrl" => (orderBy == "asc")
                    ? response.OrderBy(x => x.CaseFormUrl)
                    : response.OrderByDescending(x => x.CaseFormUrl),
                "dateOfBirth" => (orderBy == "asc")
                    ? response.OrderBy(x =>
                    {
                        _ = DateTime.TryParseExact(x.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var dt);
                        return dt;
                    })
                    : response.OrderByDescending(x =>
                    {
                        _ = DateTime.TryParseExact(x.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var dt);
                        return dt;
                    }),
                "officerEmail" => (orderBy == "asc")
                    ? response.OrderBy(x => x.OfficerEmail)
                    : response.OrderByDescending(x => x.OfficerEmail),
                _ => (orderBy == "asc")
                    ? response.OrderBy(GetDateToSortBy)
                    : response.OrderByDescending(GetDateToSortBy)
            };

            static DateTime? GetDateToSortBy(CareCaseData x)
            {
                if (string.IsNullOrEmpty(x.DateOfEvent))
                {
                    var success = DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timeStamp);
                    if (success) return timeStamp;

                    var successForDataImportTimestampFormat = DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataImportTimestamp);
                    if (successForDataImportTimestampFormat) return dataImportTimestamp;

                    var successForNonIso24HrTimestampFormat = DateTime.TryParseExact(x.CaseFormTimestamp, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var nonIso24HrTimestamp);
                    if (successForNonIso24HrTimestampFormat) return nonIso24HrTimestamp;
                }
                else
                {
                    var success = DateTime.TryParseExact(x.DateOfEvent, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfEvent);
                    if (success) return dateOfEvent;

                    var successForIsoDateTimeFormat = DateTime.TryParseExact(x.DateOfEvent, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfEventIsoDateTimeFormat);
                    if (successForIsoDateTimeFormat) return dateOfEventIsoDateTimeFormat;

                    var successForIsoDateFormat = DateTime.TryParseExact(x.DateOfEvent, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfEventIsoDateFormat);
                    if (successForIsoDateFormat) return dateOfEventIsoDateFormat;
                }

                return null;
            }
        }

        public async Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc)
        {
            var doc = BsonSerializer.Deserialize<BsonDocument>(caseNotesDoc.CaseFormData);
            await _sccvDbContext.getCollection().InsertOneAsync(doc)
                .ConfigureAwait(false);
            return doc["_id"].AsObjectId.ToString();
        }

        public CareCaseData? GetCaseById(string recordId)
        {
            var collection = _sccvDbContext.getCollection();
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(recordId));
            var query = collection.AsQueryable().Where(db => filter.Inject());

            var result = query.ToList();

            var response = result.ToResponse().FirstOrDefault();

            return response;
        }
    }
}
