using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using System;
using System.Net;
using System.Net.Http;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MosaicAPIGateway : IMosaicAPIGateway
    {
        public ResidentInformationList GetResidents(ResidentQueryParam rqp, int cursor, int limit)
        {
            string firstName = rqp.FirstName;
            string lastName = rqp.LastName;
            string dateOfBirth = rqp.DateOfBirth;
            string mosaicId = rqp.MosaicId;
            string contextFlag = rqp.ContextFlag;

            string apiUrl = Environment.GetEnvironmentVariable("MOSAIC_API_URL");
            string token = Environment.GetEnvironmentVariable("MOSAIC_API_TOKEN");

            var queryURI = new Uri($"{apiUrl}residents?first_name={firstName}&last_name={lastName}&date_of_birth={dateOfBirth}&mosaic_id={mosaicId}&context_flag={contextFlag}&cursor={cursor}&limit={limit}");

            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", token);

            HttpResponseMessage httpResponseMessage = _httpClient.GetAsync(queryURI).Result;

            var statusCode = httpResponseMessage.StatusCode;

            if (statusCode != HttpStatusCode.OK)
            {
                throw new MosaicApiException($"Fetching data from mosaic API failed");
            }

            string result = httpResponseMessage.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<ResidentInformationList>(result);
        }
    }
}
