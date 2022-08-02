using StreamingDemo.Api.TwitterClient.Model;

namespace StreamingDemo.Api.TwitterClient;

public interface ITwitterSamplesFeed
{
    IAsyncEnumerable<Tweet> GetTweetsAsync(CancellationToken cancellationToken = default);
}