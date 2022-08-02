using System.Net.Mime;
using System.Text.Json.Serialization;

namespace StreamingDemo.Api.TwitterClient.Model;

public class Tweet
{
    /// <summary>
    /// Unique identifier of this Tweet.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Creation time of the Tweet.
    /// To return this field, add <see cref="TweetFields.CreatedAt"/> in the request's query parameter.
    /// </summary>
    [JsonPropertyName("created_at")]
    //[JsonConverter(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// The content of the Tweet.
    /// To return this field, add <see cref="MediaTypeNames.Text"/> in the request's query parameter.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;

    /// <summary>
    /// Contains details about text that has a special meaning in a Tweet.
    /// To return this field, add <see cref="Model.Entities"/> in the request's query parameter.
    /// </summary>
    [JsonPropertyName("entities")]
    public Entities? Entities { get; set; }

}