using HackerNewsChallenge.Api.Models.Stories;

namespace HackerNewsChallenge.Api.Services.Interfaces;

public interface IStoryService
{
    Task<PagedResult<StoryDto>> GetNewestStoriesAsync(
        int page,
        int pageSize,
        string? query,
        CancellationToken ct);
}
