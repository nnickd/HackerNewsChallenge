using HackerNewsChallenge.Api.Models.Stories;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StoriesController : ControllerBase
{
    private readonly IStoryService _stories;

    public StoriesController(IStoryService stories)
    {
        _stories = stories;
    }

    [HttpGet("newest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StoryDto>>> GetNewest(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? query = null,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;

        const int maxPageSize = 50;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var result = await _stories.GetNewestStoriesAsync(page, pageSize, query, ct);
        return Ok(result);
    }
}
