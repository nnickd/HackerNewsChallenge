using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Models.Stories;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

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

    private const int MaxConcurrency = 10;

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

        var windowSize = pageSize * 4;

        while (idx < ids.Count && items.Count < pageSize)
        {
            var windowIds = ids.Skip(skip).Take(windowSize).ToArray();
            if (windowIds.Length == 0) break;

            var fetchedItems = await FetchWindowAsync(windowIds, ct);

            foreach (var item in fetchedItems)
            {
                if (items.Count >= pageSize) break;
                items.Add(item);
            }

            idx += windowIds.Length;
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

        var idx = 0;
        var windowSize = pageSize * 6;
        
        while (idx < ids.Count && items.Count < pageSize)
        {
            var windowIds = ids.Skip(idx).Take(windowSize).ToArray();
            if (windowIds.Length == 0) break;

            var fetchedItems = await FetchWindowAsync(windowIds, ct);

            foreach (var item in fetchedItems)
            {
                if (item?.Title is null) continue;
                if (!TitleMatchesQuery(item.Title, queryParts)) continue;

                if (matchesSkipped < matchesToSkip)
                {
                    matchesSkipped++;
                    continue;
                }

                items.Add(item);
                if (items.Count >= pageSize) break;
            }

            idx += windowIds.Length;
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

    private async Task<List<HackerNewsItem>> FetchWindowAsync(IReadOnlyList<long> ids, CancellationToken ct)
    {
        var bag = new ConcurrentBag<HackerNewsItem>();

        await Parallel.ForEachAsync(
            ids,
            new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrency, CancellationToken = ct },
            async (id, token) =>
            {
                var item = await GetCachedItemAsync(id, token);
                if (item is not null)
                {
                    bag.Add(item);
                }
            }
        );

        var bagDict = bag.ToDictionary(x => x.Id);
        var sortedItems = new List<HackerNewsItem>();

        foreach (var id in ids)
        {
            if (bagDict.TryGetValue(id, out var item))
            {
                sortedItems.Add(item); 
            }
        }

        return sortedItems;
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