using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using StreamingDemo.Api.RankedHashtags.Projectors;
using StreamingDemo.Api.TwitterClient.Model;
using Xunit;

namespace StreamingDemo.Api.Tests.RankedHashtags.Projectors
{
    public class RedisTweetProjectorTests: TweetProjectorTestsBase
    {
        private static IMock<IConnectionMultiplexer> CreateConnectionMultiplexer(IMock<IDatabase>? redis = null)
        {
            redis ??= new Mock<IDatabase>();
            var multiplexer = new Mock<IConnectionMultiplexer>();
            multiplexer.Setup(t => t.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(redis.Object);
            return multiplexer;
        }
        [Fact()]
        public void Should_Create_RedisTweetProjector()
        {
            var redis = new Mock<IDatabase>();
            var projector = new RedisTweetProjector(CreateConnectionMultiplexer(redis).Object,  new Mock<ILogger<RedisTweetProjector>>().Object);
            projector.RankCount.Should().Be(10);
            redis.Verify(m=>m.KeyDelete(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
        }
        [Fact()]
        public void Should_Destroy_RedisTweetProjector()
        {
            var redis = new Mock<IDatabase>();
            using (var projector = new RedisTweetProjector(CreateConnectionMultiplexer(redis).Object, new Mock<ILogger<RedisTweetProjector>>().Object)) ;
            redis.Verify(m => m.KeyDelete(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Exactly(2));
        }
        [Fact()]
        public async Task Should_Rank_Tags()
        {
            var tweetProjector = new ConcurrentTweetProjector();
            var redis = new Mock<IDatabase>();
            redis.Setup(m => m.SortedSetIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<double>(),
                    It.IsAny<CommandFlags>()))
                .Returns(async (RedisKey _, RedisValue member, double _, CommandFlags _) =>
                {
                    await tweetProjector.ProjectAsync(new Tweet()
                        { Entities = new Entities { Hashtags = new[] { new HashtagEntity() { Tag = member! } } } });
                    return 1;
                });
            redis.Setup(m => m.SortedSetRangeByScoreWithScores(It.IsAny<RedisKey>(), It.IsAny<double>(), It.IsAny<double>(),
                    It.IsAny<Exclude>(), Order.Descending, 0L, 10L, CommandFlags.None))
                .Returns( (RedisKey _, double _, double _, Exclude _, Order _, long _, long _, CommandFlags _) =>
                    tweetProjector.CurrentStatistics.TopHashtags.Select(t=>new SortedSetEntry(t.Tag, t.Count)).ToArray()
                );
            var projector = new RedisTweetProjector(CreateConnectionMultiplexer(redis).Object, new Mock<ILogger<RedisTweetProjector>>().Object);
            await ProjectAsync(projector);
        }
    }
}