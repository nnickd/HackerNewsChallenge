namespace HackerNewsChallenge.Api.Models.HackerNews
{
    public sealed class HackerNewsItem
    {
        public long Id { get; set; }
        public bool? Deleted { get; set; }
        public string? Type { get; set; }
        public string? By { get; set; }
        public long Time { get; set; }
        public string? Text { get; set; }
        public bool? Dead { get; set; }
        public long? Parent { get; set; }
        public long? Poll { get; set; }
        public List<long>? Kids { get; set; }
        public string? Url { get; set; }
        public int? Score { get; set; }
        public string? Title { get; set; }
        public List<long>? Parts { get; set; }
        public int? Descendants { get; set; }
    }
}