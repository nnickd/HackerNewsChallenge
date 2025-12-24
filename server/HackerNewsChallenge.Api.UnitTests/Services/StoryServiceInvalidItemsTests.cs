using FluentAssertions;
using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HackerNewsChallenge.Api.UnitTests.Services;

public sealed class StoryServiceInvalidItemsTests
{
    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetNewestStoriesAsync_SkipsInvalidStories()
    {
        var hn = new Mock<IHackerNewsClient>(MockBehavior.Strict);
        using var cache = CreateCache();

        var ids = new long[] { 1, 2, 3, 4, 5 };

        hn.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(ids);

        hn.Setup(x => x.GetItemAsync(1, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 1,
              Type = "story",
              Title = "Valid story",
              Time = 1
          });

        hn.Setup(x => x.GetItemAsync(2, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 2,
              Type = "story",
              Title = "Dead story",
              Dead = true,
              Time = 2
          });

        hn.Setup(x => x.GetItemAsync(3, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 3,
              Type = "story",
              Title = "Deleted story",
              Deleted = true,
              Time = 3
          });

        hn.Setup(x => x.GetItemAsync(4, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 4,
              Type = "comment",
              Text = "Not a story",
              Time = 4
          });

        hn.Setup(x => x.GetItemAsync(5, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem
          {
              Id = 5,
              Type = "story",
              Title = "",
              Time = 5
          });

        var sut = new StoryService(hn.Object, cache);

        var result = await sut.GetNewestStoriesAsync(
            page: 1,
            pageSize: 10,
            query: null,
            ct: CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Id.Should().Be(1);

        hn.VerifyAll();
    }
}
