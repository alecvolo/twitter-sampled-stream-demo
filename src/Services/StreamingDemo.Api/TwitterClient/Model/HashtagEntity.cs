using System.Text.Json.Serialization;

namespace StreamingDemo.Api.TwitterClient.Model;

/// <summary>
/// Contains details about text recognized as a Hashtag.
/// </summary>
public record HashtagEntity
{
    /// <summary>
    /// The start position (zero-based) of the recognized entity within the Tweet.
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>
    /// The end position (zero-based) of the recognized entity within the Tweet.
    /// </summary>
    [JsonPropertyName("end")]
    public int End { get; set; }
    /// <summary>
    /// The text of the Hashtag.
    /// </summary>
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = null!;
}