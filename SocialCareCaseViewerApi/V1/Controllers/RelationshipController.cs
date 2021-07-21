using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System;

namespace SocialCareCaseViewerApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class RelationshipController : BaseController
    {
        private readonly IRelationshipsUseCase _relationshipsUseCase;
        private readonly IPersonalRelationshipsUseCase _personalRelationshipsUseCase;

        public RelationshipController(IRelationshipsUseCase relationshipsUseCase, IPersonalRelationshipsUseCase personalRelationshipsUseCase)
        {
            _relationshipsUseCase = relationshipsUseCase;
            _personalRelationshipsUseCase = personalRelationshipsUseCase;
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

        /// <summary>
        /// Create a personal relationship
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">Successfully created personal relationship.</response>
        /// <response code="400">Invalid CreatePersonalRelationshipRequest received</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        [Route("relationships/personal")]
        public IActionResult CreatePersonalRelationship([FromBody] CreatePersonalRelationshipRequest request)
        {
            var validator = new CreatePersonalRelationshipRequestValidator();
            var validationResults = validator.Validate(request);

            if (!validationResults.IsValid)
            {
                return BadRequest(validationResults.ToString());
            }

            try
            {
                _personalRelationshipsUseCase.ExecutePost(request);

                return CreatedAtAction("CreatePersonalRelationship", "Successfully created personal relationship.");
            }
            catch (Exception e) when (
                e is PersonNotFoundException ||
                e is PersonalRelationshipTypeNotFoundException ||
                e is PersonalRelationshipAlreadyExistsException ||
                e is WorkerNotFoundException
            )
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete a personal relationship
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Successfully removed a relationship.</response>
        /// <response code="400">Invalid DeletePersonalRelationshipRequest received</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete]
        [Route("relationships/personal/{id:long}")]
        public IActionResult RemovePersonalRelationship(long id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            try
            {
                _personalRelationshipsUseCase.ExecuteDelete(id);
                return Ok();
            }
            catch (Exception e) when (
                e is PersonalRelationshipNotFoundException
            )
            {
                return BadRequest(e.Message);
            }
        }
    }
}
