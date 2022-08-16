using StackExchange.Redis;
using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

public class RedisTweetProjector: ITweetsStatisticsProcessor, IDisposable
{
    private readonly ILogger<RedisTweetProjector> _logger;
    private readonly IDatabase _redis;
    private readonly string _hashTagsKey = Guid.NewGuid().ToString("N");
    private ulong _tweetsCount;

    public int RankCount { get; }
    public RedisTweetProjector(IConnectionMultiplexer connection, ILogger<RedisTweetProjector> logger) : this(10, connection, logger)
    {
    }

    public RedisTweetProjector(int rankCount, IConnectionMultiplexer connection, ILogger<RedisTweetProjector> logger)
    {
        if (rankCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rankCount), "Should be more 0");
        }
        RankCount = rankCount;
        _ = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        try
        {
            _redis = connection.GetDatabase();
            _logger.LogInformation("Connected to redis {connectionInfo}", connection.Configuration);
            _redis.KeyDelete(_hashTagsKey, CommandFlags.FireAndForget);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Can't connect to Redis");
            throw;
        }
    }
    public async Task ProjectAsync(Tweet tweet, CancellationToken cancellationToken = default)
    {
        try
        {
            Interlocked.Increment(ref _tweetsCount);
            if (tweet.Entities?.Hashtags != null)
            {
                foreach (var entitiesHashtag in tweet.Entities.Hashtags)
                {
                    await _redis.SortedSetIncrementAsync(_hashTagsKey, entitiesHashtag.Tag, 1);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Problem with Redis");
        }
    }

    public TweetsStatistics CurrentStatistics
    {
        get
        {
            try
            {
                var result = _redis.SortedSetRangeByScoreWithScores(_hashTagsKey, order: Order.Descending, take:10);
                return new()
                {
                    TweetsCount = _tweetsCount,
                    TopHashtags = result.Select(t => new TagCount(t.Element!, (ulong)t.Score)).ToArray()
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Problem with Redis");
                throw;
            }
        }
    }

    ~RedisTweetProjector()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        try
        {
            _redis.KeyDelete(_hashTagsKey, CommandFlags.FireAndForget);
        }
        catch
        {
            // ignored
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}