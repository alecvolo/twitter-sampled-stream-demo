using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

public class LockTweetProjector : ITweetsStatisticsProcessor
{
    private ulong _tweetsCount;
    private static readonly int KeyFormattedLength = ulong.MaxValue.ToString().Length;

    public int RankCount { get; }
    private readonly Dictionary<string, ulong> _hashtags = new(StringComparer.CurrentCultureIgnoreCase);

    public LockTweetProjector() : this(10)
    {
    }

    public LockTweetProjector(int rankCount)
    {
        if (rankCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rankCount), "Should be more 0");
        }
        RankCount = rankCount;
    }

    public Task ProjectAsync(Tweet tweet, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _tweetsCount);
        if (tweet.Entities?.Hashtags != null)
        {
            foreach (var entitiesHashtag in tweet.Entities.Hashtags)
            {
                lock (this)
                {
                    if (!_hashtags.TryGetValue(entitiesHashtag.Tag, out var hashtageCount))
                    {
                        _hashtags.Add(entitiesHashtag.Tag, 1);
                    }
                    else
                    {
                        _hashtags[entitiesHashtag.Tag] = ++hashtageCount;
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    public TweetsStatistics CurrentStatistics => new()
    {
        TweetsCount = _tweetsCount,
        TopHashtags = _hashtags.OrderByDescending(t => t.Value).Take(RankCount).Select(t => new TagCount(t.Key, t.Value)).ToArray()
    };
}