using HackerNewsChallenge.Api.Models.HackerNews;

namespace HackerNewsChallenge.Api.Services.Interfaces;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<long>> GetNewStoryIdsAsync(CancellationToken ct);
    Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct);
}
