using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using System;
using System.Net.Http;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class SocialCarePlatformAPIGateway : ISocialCarePlatformAPIGateway
    {
        private readonly HttpClient _httpClient;

        public SocialCarePlatformAPIGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;

            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", "API-Key " + Environment.GetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_TOKEN"));
            }
        }

        public CaseNote GetCaseNoteById(string id)
        {
            try
            {
                string relativePath = $"casenotes/{id}";

                Uri uri = new Uri(relativePath, UriKind.Relative);

                var caseNotesResponseMessage = _httpClient.GetAsync(uri).Result;

                if ((int) caseNotesResponseMessage.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    throw new SocialCarePlatformApiException("Unauthorized");
                }

                try
                {
                    string result = caseNotesResponseMessage.Content.ReadAsStringAsync().Result;

                    return JsonConvert.DeserializeObject<CaseNote>(result);
                }
                catch (Exception)
                {
                    throw new SocialCarePlatformApiException("Unable to deserialize object");
                }
            }
            catch (SocialCarePlatformApiException)
            {
                throw;
            }
        }

        public ListCaseNotesResponse GetCaseNotesByPersonId(string id)
        {
            try
            {
                string relativePath = $"casenotes/person/{id}";

                Uri uri = new Uri(relativePath, UriKind.Relative);

                var caseNotesResponseMessage = _httpClient.GetAsync(uri).Result;

                if ((int) caseNotesResponseMessage.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    throw new SocialCarePlatformApiException("Unauthorized");
                }

                try
                {
                    string result = caseNotesResponseMessage.Content.ReadAsStringAsync().Result;

                    return JsonConvert.DeserializeObject<ListCaseNotesResponse>(result);
                }
                catch (Exception)
                {
                    throw new SocialCarePlatformApiException("Unable to deserialize object");
                }
            }
            catch (SocialCarePlatformApiException)
            {
                throw;
            }
        }
    }
}
