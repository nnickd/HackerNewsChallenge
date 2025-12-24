using FluentAssertions;
using HackerNewsChallenge.Api.Models.HackerNews;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HackerNewsChallenge.Api.UnitTests.Services;

public sealed class StoryServiceSearchTests
{
    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetNewestStoriesAsync_WithQuery_FiltersAndPagesMatches()
    {
        var hn = new Mock<IHackerNewsClient>(MockBehavior.Strict);
        using var cache = CreateCache();

        var ids = new long[] { 1, 2, 3, 4, 5 };

        hn.Setup(x => x.GetNewStoryIdsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(ids);

        hn.Setup(x => x.GetItemAsync(1, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 1, Type = "story", Title = "Cloud systems", Time = 1 });

        hn.Setup(x => x.GetItemAsync(2, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 2, Type = "story", Title = "Local dev tools", Time = 2 });

        hn.Setup(x => x.GetItemAsync(3, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 3, Type = "story", Title = "Open source cloud", Time = 3 });

        hn.Setup(x => x.GetItemAsync(4, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 4, Type = "story", Title = "Databases 101", Time = 4 });

        hn.Setup(x => x.GetItemAsync(5, It.IsAny<CancellationToken>()))
          .ReturnsAsync(new HackerNewsItem { Id = 5, Type = "story", Title = "CLOUD security", Time = 5 });

        var sut = new StoryService(hn.Object, cache);

        var page1 = await sut.GetNewestStoriesAsync(page: 1, pageSize: 2, query: "cloud", ct: CancellationToken.None);

        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
        page1.Items.Select(x => x.Id).Should().ContainInOrder(1, 3);

        var page2 = await sut.GetNewestStoriesAsync(page: 2, pageSize: 2, query: "cloud", ct: CancellationToken.None);

        page2.Page.Should().Be(2);
        page2.PageSize.Should().Be(2);
        page2.Items.Select(x => x.Id).Should().ContainSingle().Which.Should().Be(5);

        hn.VerifyAll();
    }
}
