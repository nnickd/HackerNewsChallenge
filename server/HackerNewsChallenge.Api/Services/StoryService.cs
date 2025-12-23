using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Models.Stories;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HackerNewsChallenge.Api.Services;

public sealed class StoryService : IStoryService
{
    private const string NewStoryIdsCacheKey = "hn:newstories:ids";
    private static readonly TimeSpan NewStoryIdsTtl = TimeSpan.FromMinutes(2);

    private const string HackerNewsItemCacheKeyPrefix = "hn:item";
    private static readonly TimeSpan HackerNewsItemTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MissingItemTtl = TimeSpan.FromMinutes(10);

    private static readonly Regex TokenTrimRegex = new(@"^[^\w]+|[^\w]+$", RegexOptions.Compiled);

    private readonly IHackerNewsClient _hn;
    private readonly IMemoryCache _cache;

    public StoryService(IHackerNewsClient hn, IMemoryCache cache)
    {
        _hn = hn;
        _cache = cache;
    }

    public async Task<PagedResult<StoryDto>> GetNewestStoriesAsync(
        int page,
        int pageSize,
        string? query,
        CancellationToken ct)
    {
        var ids = await GetCachedNewStoryIdsAsync(ct);

        return string.IsNullOrWhiteSpace(query)
             ? await GetPagedNewestStoriesAsync(page, pageSize, ids, ct)
             : await SearchNewestStoriesAsync(page, pageSize, ids, query!, ct);
    }

    private async Task<PagedResult<StoryDto>> GetPagedNewestStoriesAsync(int page, int pageSize, IReadOnlyList<long> ids, CancellationToken ct)
    {
        var items = new List<HackerNewsItem>();
        int skip = (page - 1) * pageSize;
        var idx = skip;

        while (idx < ids.Count && items.Count < pageSize)
        {
            var item = await GetCachedItemAsync(ids[idx], ct);
            if (item is not null)
            {
                items.Add(item);
            }

            idx++;
        }

        return new PagedResult<StoryDto>()
        {
            Page = page,
            PageSize = pageSize,
            Total = ids.Count,
            Items = items.Select(MapToDto).ToList()
        };
    }

    private async Task<PagedResult<StoryDto>> SearchNewestStoriesAsync(int page, int pageSize, IReadOnlyList<long> ids, string query, CancellationToken ct)
    {
        var items = new List<HackerNewsItem>();
        int matchesToSkip = (page - 1) * pageSize;
        var matchesSkipped = 0;
        var queryParts = query
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => TokenTrimRegex.Replace(x, ""))
                        .Where(x => x.Length > 0)
                        .ToArray();

        for (var idx = 0; idx < ids.Count && items.Count < pageSize; idx++)
        {
            var item = await GetCachedItemAsync(ids[idx], ct);
            if (item?.Title is null) continue;

            var matches = TitleMatchesQuery(item.Title, queryParts);

            if (!matches) continue;

            if (matchesSkipped < matchesToSkip)
            {
                matchesSkipped++;
                continue;
            }

            items.Add(item);
        }

        return new PagedResult<StoryDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = matchesSkipped + items.Count,
            Items = items.Select(MapToDto).ToList()
        };
    }

    private static bool TitleMatchesQuery(string title, string[] queryParts)
    {
        var titleParts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matches = false;
        foreach (var tp in titleParts)
        {
            var titlePart = TokenTrimRegex.Replace(tp, "");
            if (titlePart.Length == 0) continue;

            foreach (var queryPart in queryParts)
            {
                matches = queryPart.Length <= 3
                        ? titlePart.Equals(queryPart, StringComparison.OrdinalIgnoreCase)
                        : titlePart.Contains(queryPart, StringComparison.OrdinalIgnoreCase);
                if (matches) break;
            }

            if (matches) break;
        }

        return matches;
    }

    private async Task<IReadOnlyList<long>> GetCachedNewStoryIdsAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue<IReadOnlyList<long>>(NewStoryIdsCacheKey, out var cached) && cached is not null)
            return cached;

        var ids = await _hn.GetNewStoryIdsAsync(ct);

        _cache.Set(NewStoryIdsCacheKey, ids, NewStoryIdsTtl);
        return ids;
    }

    private async Task<HackerNewsItem?> GetCachedItemAsync(long id, CancellationToken ct)
    {
        var key = GetItemCacheKey(id);

        if (_cache.TryGetValue(key, out var obj))
        {
            var cached = obj as HackerNewsItem;
            return IsValidStory(cached) ? cached : null;
        }

        var item = await _hn.GetItemAsync(id, ct);

        if (!IsValidStory(item))
        {
            _cache.Set(key, (HackerNewsItem?)null, MissingItemTtl);
            return null;
        }

        _cache.Set(key, item, HackerNewsItemTtl);
        return item;
    }


    private static bool IsValidStory(HackerNewsItem? item)
    {
        return item is not null
            && item.Type == "story"
            && item.Dead != true
            && item.Deleted != true
            && item.Title?.Trim().Length > 0;
    }

    private static StoryDto MapToDto(HackerNewsItem item)
    {
        var story = new StoryDto
        {
            Id = item.Id,
            Title = item.Title ?? "",
            Link = item.Url ?? $"https://news.ycombinator.com/item?id={item.Id}",
            By = item.By ?? "",
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(item.Time),
            Score = item.Score ?? 0,
            CommentCount = item.Descendants ?? 0
        };

        return story;
    }

    private static string GetItemCacheKey(long id)
    {
        return $"{HackerNewsItemCacheKeyPrefix}:{id}";
    }
}