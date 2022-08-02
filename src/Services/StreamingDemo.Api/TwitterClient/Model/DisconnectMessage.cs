using System.Text.Json.Serialization;

namespace StreamingDemo.Api.TwitterClient.Model;
public class DisconnectMessage
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("stream_name")]
    public string StreamName { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

}