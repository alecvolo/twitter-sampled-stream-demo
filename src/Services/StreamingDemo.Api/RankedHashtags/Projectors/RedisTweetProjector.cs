using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

//todo use Radis to calculate top hashtags
public class RedisTweetProjector: ITweetsStatisticsProcessor
{
    public Task ProjectAsync(Tweet tweet, CancellationToken cancellationToken = default)
    {
        // for each hashtag - ZADD leaderboard 1 <hashtag>
        throw new NotImplementedException();
    }

    public int RankCount => throw new NotImplementedException();

    public TweetsStatistics CurrentStatistics
    {
        get
        {
            // ZREVRANGE leaderboard 0 RankCount
            throw new NotImplementedException();
        }
    }
}