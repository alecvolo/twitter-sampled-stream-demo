using System.Collections.Concurrent;
using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

public class ConcurrentTweetProjector : ITweetsStatisticsProcessor
{
    private ulong _tweetsCount;
    private ulong _leastTopCount = 0;
    public int RankCount { get; }
    private readonly ConcurrentDictionary<string, ulong> _hashtags = new(StringComparer.CurrentCultureIgnoreCase);
    public ConcurrentTweetProjector() : this(10)
    {
    }

    public ConcurrentTweetProjector(int rankCount)
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
                // AddOrUpdate - factory is not thread safe????? - yes - 
                // https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2.addorupdate?view=net-6.0#System_Collections_Concurrent_ConcurrentDictionary_2_AddOrUpdate__0_System_Func__0__1__System_Func__0__1__1__
                //If you call AddOrUpdate simultaneously on different threads, addValueFactory may be called multiple times,
                //but its key/value pair might not be added to the dictionary for every call.
                //For modifications and write operations to the dictionary, ConcurrentDictionary< TKey,TValue > uses fine - grained locking to ensure thread safety.
                //(Read operations on the dictionary are performed in a lock-free manner.)
                //The addValueFactory and updateValueFactory delegates may be executed multiple times to verify the value was added or updated as expected.
                //However, they are called outside the locks to avoid the problems that can arise from executing unknown code under a lock.
                //Therefore, AddOrUpdate is not atomic with regards to all other operations on the ConcurrentDictionary<TKey, TValue> class.
                // the source code:
                //while (true)  <-- why loop? because of try internal
                //{
                //    if (TryGetValueInternal(key, hashcode, out TValue? oldValue))
                //    {
                //        // key exists, try to update
                //        TValue newValue = updateValueFactory(key, oldValue); <-- factory is not thread safe???? Try update will return false if old value != current value
                //        if (TryUpdateInternal(key, hashcode, newValue, oldValue))
                //        {
                //            return newValue;
                //        }
                //    }
                //    else
                //    {
                //        // key doesn't exist, try to add
                //        if (TryAddInternal(key, hashcode, addValueFactory(key), updateIfExists: false, acquireLock: true, out TValue resultingValue))
                //        {
                //            return resultingValue;
                //        }
                //    }
                //}
                _hashtags.AddOrUpdate(entitiesHashtag.Tag, 1, (_, count) => ++count);
            }
        }
        return Task.CompletedTask;
    }
    public TweetsStatistics CurrentStatistics
    {
        get
        {
            // workaround, see for details https://stackoverflow.com/questions/29648849/net-concurrentdictionary-toarray-argumentexception
            var topHashTagCounts = _hashtags.Where(t=>t.Value >= _leastTopCount).OrderByDescending(t => t.Value).Take(RankCount).ToList();
            // remember the list count, to reduce sorting array
            _leastTopCount = topHashTagCounts.LastOrDefault().Value;
            return new()
            {
                TweetsCount = _tweetsCount,
                TopHashtags = topHashTagCounts.Select(t => new TagCount(t.Key, t.Value)).ToArray()
            };
        }
    }
}