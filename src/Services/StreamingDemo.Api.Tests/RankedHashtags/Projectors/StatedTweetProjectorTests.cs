using FluentAssertions;
using StreamingDemo.Api.RankedHashtags.Projectors;
using Xunit;

namespace StreamingDemo.Api.Tests.RankedHashtags.Projectors;

public class StatedTweetProjectorTests: TweetProjectorTestsBase
{
    [Fact()]
    public void Should_Create_ConcurrentTweetProjector()
    {
        var projector = new StatedTweetProjector();
        projector.RankCount.Should().Be(10);
    }

    [Theory()]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(13)]
    public void Should_Create_ConcurrentTweetProjector_With_Custom_RankCount(int rankCount)
    {
        var projector = new StatedTweetProjector(rankCount);
        projector.RankCount.Should().Be(rankCount);
    }
    [Theory()]
    [InlineData(-10)]
    [InlineData(0)]
    public void Should_Throw_ArgumentOutOfRangeException(int rankCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatedTweetProjector(rankCount));
    }
    // run time = 19.80 sec
    [Fact()]
    public async Task Should_Rank_Tags()
    {
        await ProjectAsync(new StatedTweetProjector());
    }
}