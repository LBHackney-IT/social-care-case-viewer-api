using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseRecordsUseCase : ICaseRecordsUseCase
    {
        private readonly IProcessDataGateway _processDataGateway;
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMongoGateway _mongoGateway;

        public CaseRecordsUseCase(IProcessDataGateway processDataGateway, IDatabaseGateway databaseGateway,
            IMongoGateway mongoGateway)
        {
            _processDataGateway = processDataGateway;
            _databaseGateway = databaseGateway;
            _mongoGateway = mongoGateway;
        }

        public CareCaseDataList GetResidentCases(ListCasesRequest request)
        {
            string? ncId = null;

            //grab both mosaic id and nc reference id
            if (!string.IsNullOrWhiteSpace(request.MosaicId))
            {
                string ncIdTmp = _databaseGateway.GetNCReferenceByPersonId(request.MosaicId);

                if (!string.IsNullOrEmpty(ncIdTmp))
                {
                    ncId = ncIdTmp;
                }

                string mosaicIdTmp = _databaseGateway.GetPersonIdByNCReference(request.MosaicId);

                if (!string.IsNullOrEmpty(mosaicIdTmp))
                {
                    ncId = request.MosaicId;
                    request.MosaicId = mosaicIdTmp;
                }
            }

            var (response, totalCount) = _processDataGateway.GetProcessData(request, ncId);
            var allCareCaseData = response.ToList();
            long? deletedRecordsCount = null;

            if (request.MosaicId != null || request.WorkerEmail != null || request.FormName != null || request.FirstName != null || request.LastName != null)
            {
                var filter = GenerateFilterDefinition(request);

                if (request.IncludeDeletedRecordsCount)
                {
                    var deletedRecordsFilter = GenerateFilterDefinition(request, addDeletedRecordsFilter: false);

                    deletedRecordsFilter &= Builders<CaseSubmission>.Filter.Eq(x => x.Deleted, true);

                    deletedRecordsCount = _mongoGateway.GetRecordsCountByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], deletedRecordsFilter);

                }

                var caseSubmissions = _mongoGateway
                    .LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], filter, null)
                    .Item1
                    .Select(x => x.ToCareCaseData(request))
                    .ToList();

                allCareCaseData.AddRange(caseSubmissions);
                totalCount += caseSubmissions.Count;
            }
            if (request.ExcludeAuditTrailEvents)
            {
                var caseExclusionList = new List<string>
                {
                    "Person updated",
                    "Person added",
                    "Person created",
                    "Warning Note Added",
                    "Warning Note Expired",
                    "Warning Note Created",
                    "Warning Note Reviewed",
                    "Warning Note Ended",
                    "Worker allocated",
                    "Team allocated",
                    "Worker deallocated"
                };
                allCareCaseData = allCareCaseData.Where(x => (!caseExclusionList.Contains(x.FormName))).ToList();
            }

            var combinedCases = new List<CareCaseData>();

            if (request.PinnedFirst)
            {
                var pinnedCases = allCareCaseData
                    .Where(x => !String.IsNullOrEmpty(x.PinnedAt))
                    .OrderByDescending(x => x.PinnedAt);
                var regularCases = allCareCaseData.Where(x => String.IsNullOrEmpty(x.PinnedAt));
                var careCaseData = SortData(request.SortBy ?? "", request.OrderBy ?? "desc", regularCases);

                pinnedCases.Concat(careCaseData);

                combinedCases = pinnedCases
                    .Concat(careCaseData)
                    .Skip(request.Cursor)
                    .Take(request.Limit)
                    .ToList();
            }
            else
            {
                combinedCases = SortData(request.SortBy ?? "", request.OrderBy ?? "desc", allCareCaseData)
                    .Skip(request.Cursor)
                    .Take(request.Limit)
                    .ToList();
            }

            int? nextCursor = request.Cursor + request.Limit;

            //support page size 1
            if (nextCursor == totalCount || combinedCases.Count < request.Limit) nextCursor = null;

            return new CareCaseDataList
            {
                Cases = combinedCases.ToList(),
                NextCursor = nextCursor,
                TotalCount = allCareCaseData.Count,
                DeletedRecordsCount = deletedRecordsCount
            };
        }

        public static FilterDefinition<CaseSubmission> GenerateFilterDefinition(ListCasesRequest request, bool addDeletedRecordsFilter = true)
        {
            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;

            if (request.MosaicId != null)
            {
                var bsonQuery = "{'Residents._id':" + request.MosaicId + "}";

                filter &= MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonQuery);
            }
            if (request.WorkerEmail != null)
            {
                var bsonQuery = "{'CreatedBy.Email':" + "\"" + request.WorkerEmail + "\"" + "}";

                filter &= MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonQuery);
            }

            if (request.FormName == "Case Note")
            {
                filter &= Builders<CaseSubmission>.Filter.Eq(x =>
                    x.FormId, "adult-case-note") | Builders<CaseSubmission>.Filter.Eq(x =>
                    x.FormId, "child-case-note");
            }

            if (request.FirstName != null)
            {
                var bsonQuery = "{'Residents.FirstName':" + "/^" + request.FirstName.ToLower() + "$/i}";

                filter &= MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonQuery);
            }

            if (request.LastName != null)
            {
                var bsonQuery = "{'Residents.LastName':" + "/^" + request.LastName.ToLower() + "$/i}";

                filter &= MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonQuery);
            }

            filter &= Builders<CaseSubmission>.Filter.Eq(x =>
                x.SubmissionState, SubmissionState.Submitted);

            if (addDeletedRecordsFilter)
            {
                if (!request.IncludeDeletedRecords)
                {
                    filter &= Builders<CaseSubmission>.Filter.Ne(x => x.Deleted, true);
                }
            }

            return filter;
        }

        public CareCaseData? Execute(string recordId)
        {
            return _processDataGateway.GetCaseById(recordId);
        }

        public Task<string> Execute(CreateCaseNoteRequest request)
        {
            CaseNotesDocument doc = request.ToEntity();

            return _processDataGateway.InsertCaseNoteDocument(doc);
        }

        private static IEnumerable<CareCaseData> SortData(string sortBy, string orderBy, IEnumerable<CareCaseData> response)
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
    }
}
