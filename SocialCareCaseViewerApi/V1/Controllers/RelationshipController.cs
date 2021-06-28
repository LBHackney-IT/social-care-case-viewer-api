using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class RelationshipController : BaseController
    {
        private readonly IRelationshipsUseCase _relationshipsUseCase;

        public RelationshipController(IRelationshipsUseCase relationshipsUseCase)
        {
            _relationshipsUseCase = relationshipsUseCase;
        }

        /// <summary>
        /// Get a list of relationships by person id
        /// </summary>
        /// <response code="200">Successful request. Relationships returned</response>
        /// <response code="404">Person not found</response>
        /// <response code="500">There was a problem getting the relationships</response>
        [ProducesResponseType(typeof(ListRelationshipsResponse), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId:long}/relationships")]
        public IActionResult ListRelationships(long personId)
        {
            try
            {
                return Ok(_relationshipsUseCase.ExecuteGet(personId));
            }
            catch (GetRelationshipsException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
