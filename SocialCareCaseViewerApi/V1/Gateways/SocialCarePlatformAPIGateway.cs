using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;

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
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", Environment.GetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_TOKEN"));
            }
        }

        public CaseNote GetCaseNoteById(string id)
        {
            string path = $"/case-notes/{id}";
            return GetDataFromSocialCarePlatformAPI<CaseNote>(path);
        }

        public ListCaseNotesResponse GetCaseNotesByPersonId(string id)
        {
            string path = $"residents/{id}/case-notes";
            return GetDataFromSocialCarePlatformAPI<ListCaseNotesResponse>(path);
        }

        public ListVisitsResponse GetVisitsByPersonId(string id)
        {
            string path = $"residents/{id}/visits";
            return GetDataFromSocialCarePlatformAPI<ListVisitsResponse>(path);
        }

        private T GetDataFromSocialCarePlatformAPI<T>(string path)
        {
            try
            {
                string result;

                T data = default;

                string relativePath = $"{path}";

                Uri uri = new Uri(relativePath, UriKind.Relative);

                var responseMessage = _httpClient.GetAsync(uri).Result;

                if ((int) responseMessage.StatusCode == StatusCodes.Status200OK)
                {
                    try
                    {
                        result = responseMessage.Content.ReadAsStringAsync().Result;

                        data = JsonConvert.DeserializeObject<T>(result);
                    }
                    catch (Exception)
                    {
                        throw new SocialCarePlatformApiException($"Unable to deserialize {typeof(T).Name} object");
                    }
                }
                else
                {
                    throw new SocialCarePlatformApiException($"{(int) responseMessage.StatusCode}");
                }

                return data;
            }
            catch (SocialCarePlatformApiException)
            {
                throw;
            }
        }
    }
}
