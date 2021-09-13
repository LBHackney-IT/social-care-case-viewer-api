using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
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

        public CaseRecordsUseCase(IProcessDataGateway processDataGateway, IDatabaseGateway databaseGateway, IMongoGateway mongoGateway)
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



            if (request.MosaicId != null)
            {
                var builder = Builders<CaseSubmission>.Filter;
                var filter = builder.Empty;
                filter &= Builders<CaseSubmission>.Filter.ElemMatch(x => x.Residents,
                    r => r.Id == long.Parse(request.MosaicId));
                filter &= Builders<CaseSubmission>.Filter.Eq(x =>
                    x.SubmissionState, SubmissionState.Submitted);

                var caseSubmissions = _mongoGateway
                    .LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], filter, null)
                    .Item1
                    .Select(x => x.ToCareCaseData(request))
                    .ToList();

                allCareCaseData.AddRange(caseSubmissions);
                totalCount += caseSubmissions.Count;
            }

            var careCaseData = SortData(request.SortBy ?? "", request.OrderBy ?? "desc", allCareCaseData)
            .Skip(request.Cursor)
            .Take(request.Limit)
            .ToList();

            int? nextCursor = request.Cursor + request.Limit;

            //support page size 1
            if (nextCursor == totalCount || careCaseData.Count < request.Limit) nextCursor = null;

            return new CareCaseDataList
            {
                Cases = careCaseData.ToList(),
                NextCursor = nextCursor
            };
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
