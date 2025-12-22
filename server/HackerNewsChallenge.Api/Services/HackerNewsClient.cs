using System.Net.Http.Json;
using HackerNewsChallenge.Api.Configuration;
using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HackerNewsChallenge.Api.Services;

public sealed class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _http;
    private readonly HackerNewsOptions _options;

    public HackerNewsClient(HttpClient http, IOptions<HackerNewsOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<long>> GetNewStoryIdsAsync(CancellationToken ct)
    {
        var itemIds = await _http.GetFromJsonAsync<IReadOnlyList<long>>($"{_options.BaseUrl}/v0/newstories.json", ct);

        if (itemIds == null)
        {
            return Array.Empty<long>();
        }

        return itemIds;
    }

    public async Task<HackerNewsItem?> GetItemAsync(long id, CancellationToken ct)
    {
        var item = await _http.GetFromJsonAsync<HackerNewsItem>($"{_options.BaseUrl}/v0/item/{id}.json", ct);

        return item;
    }
}
