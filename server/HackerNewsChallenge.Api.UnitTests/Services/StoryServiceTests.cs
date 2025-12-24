using FluentAssertions;
using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HackerNewsChallenge.Api.UnitTests.Services;

public sealed class StoryServiceTests
{
    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetNewestStoriesAsync_NoQuery_ReturnsPagedNewestStories()
    {
        var hn = new Mock<IHackerNewsClient>(MockBehavior.Strict);
        using var cache = CreateCache();

        var ids = new long[] { 1, 2, 3 };

        hn.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(ids);

        hn.Setup(x => x.GetItemAsync(1, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 1, Type = "story", Title = "One", Time = 1 });

        hn.Setup(x => x.GetItemAsync(2, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 2, Type = "story", Title = "Two", Time = 2 });

        hn.Setup(x => x.GetItemAsync(3, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 3, Type = "story", Title = "Three", Time = 3 });

        var sut = new StoryService(hn.Object, cache);

        var result = await sut.GetNewestStoriesAsync(page: 1, pageSize: 2, query: null, ct: CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Id).Should().ContainInOrder(1, 2);

        result.Total.Should().Be(ids.Length);

        hn.VerifyAll();
    }
}
