using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HackerNewsChallenge.Api.Models.Stories;
using HackerNewsChallenge.Api.IntegrationTests.TestHost;
using Xunit;

namespace HackerNewsChallenge.Api.IntegrationTests.Controllers;

public sealed class StoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNewest_Returns200_AndPagedStories()
    {
        var response = await _client.GetAsync("/api/stories/newest?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<StoryDto>>();
        body.Should().NotBeNull();
        body!.Page.Should().Be(1);
        body.PageSize.Should().Be(2);
        body.Items.Should().HaveCount(2);
        body.Items[0].Id.Should().Be(101);
        body.Items[1].Id.Should().Be(102);
    }

    [Fact]
    public async Task GetNewest_WithQuery_FiltersStories()
    {
        var response = await _client.GetAsync("/api/stories/newest?page=1&pageSize=10&query=angular");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<StoryDto>>();
        body.Should().NotBeNull();

        body!.Items.Should().NotBeEmpty();
        body.Items.Should().OnlyContain(s => s.Title.Contains("angular", StringComparison.OrdinalIgnoreCase));

        body.Items.Select(x => x.Id).Should().Contain(new long[] { 101, 103 });
        body.Items.Select(x => x.Id).Should().NotContain(102);
    }

    [Theory]
    [InlineData(0, 0, 1, 20)]
    [InlineData(-5, -10, 1, 20)]
    [InlineData(1, 999, 1, 50)]
    public async Task GetNewest_InvalidPaging_IsNormalized(
    int page,
    int pageSize,
    int expectedPage,
    int expectedPageSize)
    {
        var response = await _client.GetAsync(
            $"/api/stories/newest?page={page}&pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<StoryDto>>();
        body.Should().NotBeNull();

        body!.Page.Should().Be(expectedPage);
        body.PageSize.Should().Be(expectedPageSize);
    }
}
