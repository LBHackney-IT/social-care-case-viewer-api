using System.Collections.Generic;
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
        private readonly IGetWorkersUseCase _getWorkersUseCase;

        public WorkerController(IWorkersUseCase workersUseCase, IGetWorkersUseCase getWorkersUseCase)
        {
            _workersUseCase = workersUseCase;
            _getWorkersUseCase = getWorkersUseCase;
        }

        /// <summary>
        /// Get a list of workers by worker id, worker email or team id
        /// </summary>
        /// <response code="404">No workers found for inserted worker id, worker email or team id</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(List<WorkerResponse>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        public IActionResult GetWorkers([FromQuery] GetWorkersRequest request)
        {
            var workers = _getWorkersUseCase.Execute(request);

            if (workers.Count == 0)
            {
                return NotFound();
            }
            return Ok(workers);
        }

        /// <summary>
        /// Create a worker
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Worker created successfully</response>
        /// <response code="400">Invalid CreateWorkerRequest received</response>
        /// <response code="422">Could not process request</response>
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
                return CreatedAtAction("Worker created successfully", createdWorker);
            }
            catch (PostWorkerException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }
    }
}
