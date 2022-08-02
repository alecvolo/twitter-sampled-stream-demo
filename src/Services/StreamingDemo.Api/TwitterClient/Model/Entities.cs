using System.Text.Json.Serialization;

namespace StreamingDemo.Api.TwitterClient.Model;

/// <summary>
/// This object and its children fields contain details about text that has a special meaning in the parent model.
/// most fields are not included
/// </summary>
public record Entities
{

    /// <summary>
    /// Contains details about text recognized as a Hashtag.
    /// </summary>
    [JsonPropertyName("hashtags")]
    public HashtagEntity[]? Hashtags { get; set; }

}