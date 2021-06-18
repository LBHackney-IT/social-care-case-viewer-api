using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
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
        private readonly IRelationshipsV1UseCase _relationshipsV1UseCase;

        public RelationshipController(IRelationshipsV1UseCase relationshipsV1UseCase)
        {
            _relationshipsV1UseCase = relationshipsV1UseCase;
        }

        /// <summary>
        /// Get a list of relationships by person id
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Successful request. Relationships returned</response>
        /// <response code="404">Person not found</response>
        /// <response code="500">There was a problem getting the relationships</response>
        [ProducesResponseType(typeof(ListRelationshipsV1Response), StatusCodes.Status200OK)]
        [HttpGet]
        [Route("residents/{personId}/relationships")]
        public IActionResult ListRelationships([FromQuery] ListRelationshipsV1Request request)
        {
            try
            {
                return Ok(_relationshipsV1UseCase.ExecuteGet(request));
            }
            catch (GetRelationshipsException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
