namespace HackerNewsChallenge.Api.Configuration;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com";
}
