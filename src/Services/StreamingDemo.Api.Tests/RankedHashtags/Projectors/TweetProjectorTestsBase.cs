using FluentAssertions;
using StreamingDemo.Api.Infrastructure;
using StreamingDemo.Api.RankedHashtags.Projectors;
using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.Tests.RankedHashtags.Projectors;

public class TweetProjectorTestsBase
{

    private static Tweet CreateTweet() => CreateTweet(null);

    private static Tweet CreateTweet(params string[]? hashtags) =>
        new()
        {
            Entities = new Entities
            {
                Hashtags = hashtags?.Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => new HashtagEntity { Tag = t.Trim() }).ToArray()

            }
        };

    // run time = 19.80 sec
   // [Fact()]
    public async Task StatedTweetProjector_Should_Rank_Tags()
    {
        await ProjectAsync(new StatedTweetProjector());

    }

    // run time = 4 sec
    //[Fact()]
    public async Task ConcurrentTweetProjector_Should_Rank_Tags()
    {
        await ProjectAsync(new ConcurrentTweetProjector());

    }
    // run time = 7.4 sec
    //[Fact()]
    public async Task LockedTweetProjector_Should_Rank_Tags()
    {
        await ProjectAsync(new LockTweetProjector());

    }

    protected async Task ProjectAsync(ITweetsStatisticsProcessor projector)
    {
        var tags = Enumerable.Range(0, 99).Select(t => $"hashtag-{t:D4}").ToArray();

        var random = new Random();
        var tweets = Enumerable.Range(0, 1000000).Select(t =>
                CreateTweet(Enumerable.Range(0, 3).Select(_ => tags[random.Next(0, tags.Length - 1)]).ToArray()))
            .ToList();

        // select top hashtags unique count
        var expectedResult = tweets.Where(t => t.Entities?.Hashtags != null).SelectMany(t => t.Entities!.Hashtags!)
            .Select(t => t.Tag)
            .GroupBy(t => t, (t, g) => new TagCount(t!, (ulong)g.Count()), StringComparer.CurrentCultureIgnoreCase)
            .OrderByDescending(t => t.Count).Distinct(new TagCountComparer()).Take(projector.RankCount).ToList();
        // shift them, so we don't have top hashtag with the same count
        for (var i = (int)(expectedResult.First().Count - expectedResult.Last().Count); i >= 0; i--)
        {
            tweets.AddRange(expectedResult.Select(t => CreateTweet(t.Tag)));
        }
        expectedResult = tweets.Where(t => t.Entities?.Hashtags != null).SelectMany(t => t.Entities!.Hashtags!)
            .Select(t => t.Tag)
            .GroupBy(t => t, (t, g) => new TagCount(t!, (ulong)g.Count()), StringComparer.CurrentCultureIgnoreCase)
            .OrderByDescending(t => t.Count).Distinct(new TagCountComparer()).Take(projector.RankCount).ToList();

        await Parallel.ForEachAsync(tweets, async (tweet, token) => await projector.ProjectAsync(tweet, token));

        var result = projector.CurrentStatistics;
        result.Should().NotBeNull();
        result.TweetsCount.Should().Be((ulong)tweets.Count);
        result.TopHashtags.Should().HaveCount(projector.RankCount);
        result.TopHashtags.Should().BeEquivalentTo(expectedResult);
    }
}