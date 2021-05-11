using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/teams")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class TeamController : BaseController
    {
        public TeamController()
        {

        }

        /// <summary>
        /// Create a team
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Team created successfully</response>
        /// <response code="400">Invalid CreateTeamRequest received</response>
        /// <response code="422">Could not process request</response>
        [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status201Created)]
        [HttpPost]
        public IActionResult CreateWorker([FromBody] CreateTeamRequest request)
        {
            var validator = new CreateTeamRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var createdTeam = new TeamResponse {Id = 1, Name = "placeholder", Context = "A"};
                return CreatedAtAction("Team created successfully", createdTeam);
            }
            catch (PostTeamException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }
    }
}
