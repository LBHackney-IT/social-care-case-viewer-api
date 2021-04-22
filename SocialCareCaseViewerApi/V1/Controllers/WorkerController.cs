using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/workers")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class WorkerController : BaseController
    {
        private readonly IWorkersUseCase _workersUseCase;

        public WorkerController(IWorkersUseCase workersUseCase)
        {
            _workersUseCase = workersUseCase;
        }

        /// <summary>
        /// Create a worker
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Worker created successfully</response>
        /// <response code="400">Invalid CreateWorkerRequest received</response>
        /// <response code="404">Exception encountered</response>
        /// <response code="500">There was a problem updating the record</response>
        [ProducesResponseType(typeof(WorkerResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public IActionResult CreateWorker([FromBody] CreateWorkerRequest request)
        {
            var validator = new CreateWorkerRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var createdWorker = _workersUseCase.ExecutePost(request);
                return CreatedAtAction("Create Worker", createdWorker);
            }
            catch (PostWorkerException e)
            {
                return NotFound(e.Message);
            }
        }

    }
}
