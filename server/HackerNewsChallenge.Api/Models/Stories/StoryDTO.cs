namespace HackerNewsChallenge.Api.Models.Stories;

public sealed class StoryDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? By { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? Score { get; set; }
    public int? CommentCount { get; set; }
}
