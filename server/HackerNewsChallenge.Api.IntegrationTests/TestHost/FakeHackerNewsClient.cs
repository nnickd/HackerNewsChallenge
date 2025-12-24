using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services.Interfaces;

namespace HackerNewsChallenge.Api.IntegrationTests.TestHost;

public sealed class FakeHackerNewsClient : IHackerNewsClient
{
    private static readonly long[] _ids = new long[] { 101, 102, 103 };

    public Task<IReadOnlyList<long>> GetNewStoryIdsAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<long>>(_ids);

    public Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct)
    {
        var item = id switch
        {
            101 => Story(101, "Angular signals deep dive"),
            102 => Story(102, "Dotnet performance tips"),
            103 => Story(103, "Angular routing best practices"),
            _ => null
        };

        return Task.FromResult(item);
    }

    private static HackerNewsItem Story(long id, string title)
        => new()
        {
            Id = id,
            Type = "story",
            Title = title,
            Url = $"https://example.com/{id}",
            Time = 1700000000,
            Dead = false,
            Deleted = false,
            Score = 10,
            Descendants = 5,
            By = "fake"
        };
}
