using System.Collections.Concurrent;
using FluentAssertions;
using StreamingDemo.Api.RankedHashtags.Projectors;
using Xunit;

namespace StreamingDemo.Api.Tests.RankedHashtags.Projectors;

public class ConcurrentTweetProjectorTests : TweetProjectorTestsBase
{
    [Fact()]
    public void Should_Create_ConcurrentTweetProjector()
    {
        var projector = new ConcurrentTweetProjector();
        projector.RankCount.Should().Be(10);
    }

    [Theory()]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(13)]
    public void Should_Create_ConcurrentTweetProjector_With_Custom_RankCount(int rankCount)
    {
        var projector = new ConcurrentTweetProjector(rankCount);
        projector.RankCount.Should().Be(rankCount);
    }
    [Theory()]
    [InlineData(-10)]
    [InlineData(0)]
    public void Should_Throw_ArgumentOutOfRangeException(int rankCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentTweetProjector(rankCount));
    }
    // run time = 4 sec
    [Fact()]
    public async Task Should_Rank_Tags()
    {
        await ProjectAsync(new ConcurrentTweetProjector());
    }
    [Fact()]
    public void ConcurrentDictionary_ToArray_Test()
    {
        //https://stackoverflow.com/questions/29648849/net-concurrentdictionary-toarray-argumentexception
        var ulongs = new ConcurrentDictionary<ulong, ulong>();
        for (ulong i = 0L; i < 7*1000000L; i++)
        {
            ulongs.AddOrUpdate(i, 1,(_,_)=> 1);
        }

        var shouldFail = ulongs.OrderByDescending(t => t.Value).Take(10).Select(t => t.Key).ToArray();

        var ordered = ulongs.OrderByDescending(t => t.Value).Take(10);
        var result = new List<KeyValuePair<ulong, ulong>>();
        foreach (var keyValuePair in ordered)
        {
            result.Add(keyValuePair);
        }

        result.Should().HaveCount(10);

    }

}