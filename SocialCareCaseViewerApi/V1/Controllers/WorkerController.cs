using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/workers")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class WorkerController : BaseController
    {
        public WorkerController()
        {

        }

        /// <summary>
        /// Create a worker
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Worker created successfully</response>
        /// <response code="400">Invalid CreateWorkerRequest received</response>
        /// <response code="404">Exception encountered</response>
        /// <response code="500">There was a problem updating the record</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost]
        public IActionResult CreateWorker([FromBody] CreateWorkerRequest request)
        {
            var validator = new CreateWorkerRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            return Ok("place holder");
        }

    }
}
