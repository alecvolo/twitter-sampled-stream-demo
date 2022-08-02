using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

public interface ITweetProjector
{
    public Task ProjectAsync(Tweet tweet, CancellationToken cancellationToken = default);
    int RankCount { get; }
}

public interface ITweetsStatisticsProcessor : ITweetProjector, ITweetsStatisticsStore
{
}


public interface ITweetsStatisticsStore
{
    public TweetsStatistics CurrentStatistics { get; }
}