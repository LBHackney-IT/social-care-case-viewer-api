using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class SearchGateway : ISearchGateway
    {
        private readonly DatabaseContext _databaseContext;

        public SearchGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public (List<ResidentInformation>, int, int?) GetPersonRecordsBySearchQuery(PersonSearchRequest query)
        {
            var totalCount = 0;
            var matchingResults = new List<SearchResult>();
            matchingResults = FilterByNameAgainstAllRecords(query);

            if (matchingResults.Count > 0) totalCount = matchingResults.FirstOrDefault().TotalRecords;

            var dbRecords = _databaseContext.Persons
                .Where(p => matchingResults.Select(x => x.PersonId).ToList().Contains(p.Id))
                .Include(p => p.Addresses)
                .Select(x => x.ToResidentInformationResponse()).ToList();

            var sortedDbRecords = new List<ResidentInformation>();

            foreach (var result in matchingResults)
            {
                sortedDbRecords.Add(dbRecords.First(x => x.MosaicId == result.PersonId.ToString()));
            }

            return (sortedDbRecords, totalCount, GetNextOffset(query.Cursor ?? 0, totalCount, 20));
        }
        private static int? GetNextOffset(int currentOffset, int totalRecords, int limit)
        {
            int nextOffset = currentOffset + limit;

            if (nextOffset < totalRecords)
            {
                return nextOffset;
            }
            else
            {
                return null;
            }
        }
        private List<SearchResult> FilterByNameAgainstAllRecords(PersonSearchRequest request)
        {
            var postcode = request.Postcode == null ? "" : $"{request.Postcode}%";
            var dateOfBirthRangeStart = request.DateOfBirth == null ? "" : $"{request.DateOfBirth}T00:00:00.000Z";
            var dateOfBirthRangeEnd = request.DateOfBirth == null ? "" : $"{request.DateOfBirth}T23:59:59.000Z";
            var firstName = request.FirstName ?? "";
            var lastName = request.LastName ?? "";
            var cursor = request.Cursor ?? 0;

            var sb = new StringBuilder();

            sb.Append(@"SET pg_trgm.word_similarity_threshold TO 0.4;
                        SELECT Person.person_id as PersonId,
                        COUNT('x') OVER(PARTITION BY 0) AS TotalRecords");

            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            {
                sb.Append(@", (");
                if (!string.IsNullOrEmpty(firstName))
                {
                    sb.Append(@" cast(word_similarity(Person.first_name, {0}) as real)");
                    sb.Append(
                        @" + cast(""like""(lower(Person.first_name), lower({0}) || '%')::int as real)");
                }
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    sb.Append(@" + ");
                }
                if (!string.IsNullOrEmpty(lastName))
                {
                    sb.Append(@" cast(word_similarity(Person.last_name, {1}) as real)");
                    sb.Append(
                        @" + cast(""like""(lower(Person.last_name), lower({1}) || '%')::int as real)");
                }
                sb.Append(@") as Score");
            }
            else
            {
                sb.Append(@", cast(1.0 as real) as Score");
            }

            sb.Append(@" FROM dbo.dm_persons Person
                         LEFT JOIN dbo.dm_addresses Address ON Person.person_id = Address.person_id AND Address.is_display_address = 'Y'
                         WHERE Person.marked_for_deletion = false");

            if (!string.IsNullOrEmpty(firstName))
            {
                sb.Append(@" AND {0} <% Person.first_name");
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                sb.Append(@" AND {1} <% Person.last_name");
            }

            if (!string.IsNullOrEmpty(request.DateOfBirth))
            {
                sb.Append(@" AND (Person.date_of_birth BETWEEN {2}::timestamp AND {3}::timestamp)");
            }
            if (!string.IsNullOrEmpty(request.Postcode))
            {
                sb.Append(@" AND (REPLACE(Address.post_code, ' ', '') ILIKE REPLACE({4}, ' ', ''))");
            }

            sb.Append(@" GROUP BY Person.person_id, Person.first_name, Person.last_name
                            ORDER BY
                            Score DESC,
                            Person.first_name,
                            Person.last_name
                            LIMIT 20 OFFSET {5};");

            return _databaseContext.Set<SearchResult>().FromSqlRaw(sb.ToString(), firstName, lastName, dateOfBirthRangeStart, dateOfBirthRangeEnd, postcode, cursor).ToList();
        }
    }
}
