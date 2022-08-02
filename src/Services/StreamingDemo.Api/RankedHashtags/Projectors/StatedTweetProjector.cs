using System.Text;
using StreamingDemo.Api.Infrastructure;
using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

public class StatedTweetProjector : ITweetsStatisticsProcessor
{
    private ulong _tweetsCount;
    private static readonly int KeyFormattedLength = ulong.MaxValue.ToString().Length;

    public int RankCount { get; }
    private readonly Dictionary<string, ulong> _hashtags = new(StringComparer.CurrentCultureIgnoreCase);
    private readonly SortedDictionary<string, ulong> _rankedHashtags = new(new ReverseComparer<string>(StringComparer.CurrentCultureIgnoreCase)); // or SortedList???

    public StatedTweetProjector() : this(10)
    {
    }

    public StatedTweetProjector(int rankCount)
    {
        if (rankCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rankCount), "Should be more 0");
        }
        RankCount = rankCount;
    }

    public static string ToSortingKey(ulong number, string? name = null)
    {
        var sb = new StringBuilder();
        sb.Append(number.ToString().PadLeft(KeyFormattedLength, '0')).Append(name);
        return sb.ToString();
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
                        hashtageCount = 1;
                        _hashtags.Add(entitiesHashtag.Tag, 1);
                    }
                    else
                    {
                        _hashtags[entitiesHashtag.Tag] = ++hashtageCount;
                    }
                    var newKey = ToSortingKey(hashtageCount, entitiesHashtag.Tag);
                    var oldKey = ToSortingKey(hashtageCount - 1, entitiesHashtag.Tag);
                    // if we have it in the list, then just update the count
                    if (_rankedHashtags.ContainsKey(oldKey))
                    {
                        _rankedHashtags.Remove(oldKey);
                        _rankedHashtags.Add(newKey, hashtageCount);
                        continue;
                    }
                    // check if the list is not filled up
                    if (_rankedHashtags.Count < RankCount)
                    {
                        _rankedHashtags.Add(newKey, hashtageCount);
                        continue;
                    }

                    // get the count of the least top. or may be we just always add a new and then delete???
                    var lastTopCount = ulong.Parse((_rankedHashtags.Keys.LastOrDefault() ?? ToSortingKey(0))[..KeyFormattedLength]);
                    // if less than the least top just ignore it. 
                    if (lastTopCount > hashtageCount)
                    {
                        continue;
                    }
                    // delete out of top
                    _rankedHashtags.Add(newKey, hashtageCount);
                    foreach (var key in _rankedHashtags.Keys.Skip(RankCount).ToList())
                    {
                        _rankedHashtags.Remove(key);
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    public TweetsStatistics CurrentStatistics => new()
    {
        TweetsCount = _tweetsCount,
        TopHashtags = _rankedHashtags.Select(t => new TagCount(t.Key[KeyFormattedLength..], t.Value)).ToArray()
    };
}