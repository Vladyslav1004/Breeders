using Breeders.Api.Exceptions;
using Breeders.Api.Models.DTOs;
using Breeders.Api.Services.Interfaces;
using Breeders.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Breeders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LittersController : ControllerBase
{
    private readonly ILitterService _litterService;

    public LittersController(ILitterService litterService)
    {
        _litterService = litterService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LitterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<LitterResponse>>> GetLitters(
        [FromHeader(Name = "X-Breeder-Id")] string? breederIdHeader,
        [FromQuery] LitterQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var breederId = breederIdHeader.ParseBreederId();

        var result = await _litterService.GetLittersAsync(
            breederId,
            parameters,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{litterId:guid}/publish")]
    [ProducesResponseType(typeof(LitterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LitterResponse>> Publish(Guid litterId, [FromHeader(Name = "X-Breeder-Id")] string? breederIdHeader, CancellationToken cancellationToken)
    {
        var breederId = breederIdHeader.ParseBreederId();

        var result = await _litterService.PublishAsync(
            litterId,
            breederId,
            cancellationToken);

        return Ok(result);
    }
}