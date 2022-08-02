using System.Net.Http.Headers;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StreamingDemo.Api.TwitterClient;
using Xunit;
using Xunit.Abstractions;

namespace StreamingDemo.Api.Tests.TwitterClient;

public class TwitterSamplesFeedSourceTests
{
    private readonly ITestOutputHelper _output;
    private const string TweetStreamUrl = "https://api.twitter.com/2/tweets/sample/stream";

    public TwitterSamplesFeedSourceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Theory()]
    [InlineAutoMoqData("{\"data\":{\"entities\":{\"annotations\":[{\"start\":38,\"end\":52,\"probability\":0.9596,\"type\":\"Person\",\"normalized_text\":\"Joseph R. Biden\"}],\"mentions\":[{\"start\":3,\"end\":14,\"username\":\"RepBoebert\",\"id\":\"1342989756611907584\"}]},\"id\":\"1552820343936614400\",\"text\":\"RT @RepBoebert: It turns out the R in Joseph R. Biden stand for recession.\"}}")]
    [InlineAutoMoqData("{\"data\":{\"id\":\"1552817688963125249\",\"text\":\"RT @TEQUIEROBUCARA: El segundo peor presidente de la historia queriendo hablando de ética, aunque el peor gobierno fue Duque\"}}\r\n")]
    [InlineAutoMoqData("{\"data\":{\"id\":\"1553125940930220034\",\"created_at\":\"2022-07-29T21:10:54+00:00\",\"text\":\"RT @nikhilraj111425: Afet hi\\u00E7 duraksamadan anlatmaya ba\\u015Flad\\u0131. \\u201C\\u0130lk defa birbirimizi orada g\\u00F6rd\\u00FCk. #buca  #Alsancak #Bayrakl\\u0131 #\\u00E7i\\u011Fli #Alia\\u011Fa\\u2026\",\"entities\":{\"hashtags\":[{\"start\":98,\"end\":103,\"tag\":\"buca\"},{\"start\":105,\"end\":114,\"tag\":\"Alsancak\"},{\"start\":115,\"end\":124,\"tag\":\"Bayrakl\\u0131\"},{\"start\":125,\"end\":131,\"tag\":\"\\u00E7i\\u011Fli\"},{\"start\":132,\"end\":139,\"tag\":\"Alia\\u011Fa\"}]}}}")]
    public void ParseJsonObjectTest(string json, [Frozen] Mock<ILogger<TwitterSamplesFeedSource>> loggerMock)
    {
        var source = new TwitterSamplesFeedSource(new Mock<IHttpClientFactory>().Object, loggerMock.Object);
        var jsonObject = source.ParseJsonObject(json);
        jsonObject.Should().NotBeNull();
        jsonObject.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTweetsAsyncTest()
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
        builder.AddUserSecrets(typeof(TwitterSamplesFeedSourceTests).Assembly, true);
        builder.AddEnvironmentVariables();
        var config = builder.Build();
        var services = new ServiceCollection();
        services.AddHttpClient("Twitter", client =>
        {
            client.BaseAddress = new Uri(TweetStreamUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config["TwitterBearerToken"]);
        });
        var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

        using var logger = _output.BuildLoggerFor<TwitterSamplesFeedSource>();
        var source = new TwitterSamplesFeedSource(httpClientFactory, logger);

        var cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(TimeSpan.FromSeconds(15));
        await foreach (var tweet in source.GetTweetsAsync(cancellationSource.Token))
        {
            logger.Log(LogLevel.Trace, JsonSerializer.Serialize(tweet));
        }
    }

}
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute(params object[] values) : base(() => new Fixture().Customize(new AutoMoqCustomization()))
    {

    }
}
public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] objects) : base(new AutoMoqDataAttribute(), objects) { }
}
