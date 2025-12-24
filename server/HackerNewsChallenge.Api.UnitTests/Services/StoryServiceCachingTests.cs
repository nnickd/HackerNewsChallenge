using FluentAssertions;
using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HackerNewsChallenge.Api.UnitTests.Services;

public sealed class StoryServiceCachingTests
{
    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetNewestStoriesAsync_UsesCache_ForIdsAndItems()
    {
        var hn = new Mock<IHackerNewsClient>(MockBehavior.Strict);
        using var cache = CreateCache();

        var ids = new long[] { 1 };

        hn.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(ids);

        hn.Setup(x => x.GetItemAsync(1, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 1,
              Type = "story",
              Title = "Cached story",
              Time = 1
          });

        var sut = new StoryService(hn.Object, cache);

        var first = await sut.GetNewestStoriesAsync(
            page: 1,
            pageSize: 10,
            query: null,
            ct: CancellationToken.None);

        var second = await sut.GetNewestStoriesAsync(
            page: 1,
            pageSize: 10,
            query: null,
            ct: CancellationToken.None);

        first.Items.Should().HaveCount(1);
        second.Items.Should().HaveCount(1);
        second.Items.Single().Id.Should().Be(first.Items.Single().Id);

        hn.Verify(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Once);

        hn.Verify(x => x.GetItemAsync(1, It.IsAny<CancellationToken>()), Times.Once);

        hn.VerifyNoOtherCalls();
    }
}
