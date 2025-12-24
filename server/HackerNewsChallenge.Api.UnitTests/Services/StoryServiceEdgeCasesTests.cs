using FluentAssertions;
using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HackerNewsChallenge.Api.UnitTests.Services;

public sealed class StoryServiceEdgeCasesTests
{
    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetNewestStoriesAsync_PageOutOfRange_ReturnsEmptyItems()
    {
        var hn = new Mock<IHackerNewsClient>(MockBehavior.Strict);
        using var cache = CreateCache();

        var ids = new long[] { 1, 2, 3 };

        hn.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(ids);

        var sut = new StoryService(hn.Object, cache);

        var result = await sut.GetNewestStoriesAsync(
            page: 2,
            pageSize: 10,
            query: null,
            ct: CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(ids.Length);

        hn.Verify(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Once);
        hn.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetNewestStoriesAsync_Query_MatchesDespitePunctuation()
    {
        var hn = new Mock<IHackerNewsClient>(MockBehavior.Strict);
        using var cache = CreateCache();

        var ids = new long[] { 1, 2 };

        hn.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(ids);

        hn.Setup(x => x.GetItemAsync(1, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 1,
              Type = "story",
              Title = "Hello, world!",
              Time = 1
          });

        hn.Setup(x => x.GetItemAsync(2, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 2,
              Type = "story",
              Title = "Nothing to see here",
              Time = 2
          });

        var sut = new StoryService(hn.Object, cache);

        var result = await sut.GetNewestStoriesAsync(
            page: 1,
            pageSize: 10,
            query: "world",
            ct: CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Id.Should().Be(1);

        hn.VerifyAll();
    }
}