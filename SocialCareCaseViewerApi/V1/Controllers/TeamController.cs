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
    public class TeamController : Controller
    {
        private readonly ITeamsUseCase _teamsUseCase;
        private readonly IResidentUseCase _residentUseCase;

        public TeamController(ITeamsUseCase teamsUseCase)
        {
            _teamsUseCase = teamsUseCase;
        }

        /// <summary>
        /// Get a team by team id
        /// </summary>
        /// <response code="200">Successful request and team returned</response>
        /// <response code="404">No team found for request</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("id/{id:int}")]
        public IActionResult GetTeamById(int id)
        {
            var team = _teamsUseCase.ExecuteGetById(id);

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        /// <summary>
        /// Get a team by team name
        /// </summary>
        /// <response code="200">Successful request and team returned</response>
        /// <response code="404">No team found for request</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("name/{name}")]
        public IActionResult GetTeamByName(string name)
        {
            var team = _teamsUseCase.ExecuteGetByName(name);

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        /// <summary>
        /// Get a list of teams by context
        /// </summary>
        /// <response code="200">Successful request and teams returned</response>
        /// <response code="400">One or more request parameters are invalid or missing</response>
        /// <response code="404">No teams found for request</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ListTeamsResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        public IActionResult GetTeams([FromQuery] GetTeamsRequest request)
        {
            var validator = new GetTeamsRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            var teams = _teamsUseCase.ExecuteGet(request);

            return Ok(teams);
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
                return CreatedAtAction(nameof(CreateTeam), createdTeam);
            }
            catch (PostTeamException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }

        /// <summary>
        /// Get a team by team name
        /// </summary>
        /// <response code="200">Successful request and team returned</response>
        /// <response code="404">No team found for request</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [HttpGet]
        [Route("{name}/allocations")]
        public IActionResult GetTeamAllocationsByName(string name, int? cursor = 0, int? limit = 20)
        {
            var team = _teamsUseCase.ExecuteGetByName(name);
            return Ok(_residentUseCase.GetUnallocatedList(team.Id, (int) cursor, (int) limit));
        }
    }
}
