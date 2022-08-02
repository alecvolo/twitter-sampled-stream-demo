using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Polly;
using StreamingDemo.Api.Infrastructure;
using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.TwitterClient;

public class TwitterSamplesFeedSource : ITwitterSamplesFeed
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<TwitterSamplesFeedSource> _logger;

    public TwitterSamplesFeedSource(IHttpClientFactory clientFactory, ILogger<TwitterSamplesFeedSource> logger)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async IAsyncEnumerable<Tweet> GetTweetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "?tweet.fields=created_at,entities");
        var context = new Polly.Context().WithLogger<TwitterSamplesFeedSource>(_logger);
        request.SetPolicyExecutionContext(context);
        while (!cancellationToken.IsCancellationRequested)
        {
                _logger.LogInformation("Connecting to the twitter's stream");
                var client = _clientFactory.CreateClient("Twitter");
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                //    await client.GetStreamAsync("?tweet.fields=created_at,entities", cancellationToken);
                using var reader = new StreamReader(stream);
                _logger.LogInformation("Connected to the twitter's stream");
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {

                    //We are ready to read the stream
                    var currentLine = await reader.ReadLineAsync();
                    var jsonObject = ParseJsonObject(currentLine);
                    //todo check for errors
                    var tweet = jsonObject?["data"]?.Deserialize<Tweet>();
                    if (tweet != null)
                    {
                        //https://github.com/dotnet/csharplang/discussions/765
                        yield return tweet;
                    }
                }
                _logger.LogInformation("Got disconnecting from the twitter's stream");
        }
    }
    public JsonObject? ParseJsonObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }
        try
        {
            var feed = JsonNode.Parse(json);
            if (feed is JsonObject)
            {
                return feed.AsObject();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Parsing failed {json}", json);
        }
        return null;
    }
}