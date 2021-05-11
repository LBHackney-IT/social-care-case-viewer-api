using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/teams")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class TeamController : BaseController
    {
        private readonly ITeamsUseCase _teamsUseCase;

        public TeamController(ITeamsUseCase teamsUseCase)
        {
            _teamsUseCase = teamsUseCase;
        }

        /// <summary>
        /// Get a list of teams by context
        /// </summary>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListTeamsResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        public IActionResult ListTeams([FromQuery] ListTeamsRequest request)
        {
            return Ok(_teamsUseCase.ExecuteGet(request));
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
        public IActionResult CreateTeam([FromBody] CreateTeamRequest request)
        {
            var validator = new CreateTeamRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                var createdTeam = _teamsUseCase.ExecutePost(request);
                return CreatedAtAction("Team created successfully", createdTeam);
            }
            catch (PostTeamException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }
    }
}
