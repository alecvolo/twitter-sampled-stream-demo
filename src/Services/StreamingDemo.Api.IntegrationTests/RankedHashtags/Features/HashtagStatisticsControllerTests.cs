using System.Net.Http.Json;
using FluentAssertions;
using StreamingDemo.Api.IntegrationTests.Fixtures;
using StreamingDemo.Api.RankedHashtags.Projectors;
using Xunit;

namespace StreamingDemo.Api.IntegrationTests.RankedHashtags.Features;

public class HashtagStatisticsControllerTests: IntegrationTest
{
    private const string ApiUrl = "/api/v1.0/statistics";
    public HashtagStatisticsControllerTests(ApiWebApplicationFactory fixture) : base(fixture)
    {
    }
    [Fact()]
    public async Task Should_Get_Stats()
    {
        await Task.Delay(TimeSpan.FromSeconds(10)); // to collect some data
        var data = await Client.GetFromJsonAsync<TweetsStatistics>($"{ApiUrl}/top-hashtags");
        data.Should().NotBeNull();
        data!.TweetsCount.Should().BeGreaterThan(0);
    }

}