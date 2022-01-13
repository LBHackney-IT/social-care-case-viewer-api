using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [Route("api/v1/healthcheck")]
    [ApiController]
    [Produces("application/json")]
    public class HealthCheckController : Controller
    {
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), 200)]
        public IActionResult HealthCheck()
        {
            var result = new Dictionary<string, bool> { { "success", true } };

            return Ok(result);
        }

        [HttpGet]
        [Route("error")]
        public void ThrowError()
        {
            ThrowOpsErrorUsecase.Execute();
        }

        [HttpGet]
        [Route("historical-data-database")]
        public IActionResult HistoricalDataConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("HISTORICAL_DATA_CONNECTION_STRING"));

                connection.Open();

                var npgsqlCommand = connection.CreateCommand();
                npgsqlCommand.CommandText = "select version();";
                npgsqlCommand.ExecuteNonQuery();

                connection.Dispose();

                return Ok();
            }
            catch
            {
                throw new Exception("Unable to connect to historical data database");
            }
        }
    }
}
