using StreamingDemo.Api.RankedHashtags.Projectors;
using StreamingDemo.Api.TwitterClient;

namespace StreamingDemo.Api.RankedHashtags.Services
{
    public class TweetsCounterBackgroundService: BackgroundService
    {
        private readonly ITwitterSamplesFeed _source;
        private readonly ITweetProjector _projector;
        private readonly ILogger<TweetsCounterBackgroundService> _logger;

        public TweetsCounterBackgroundService(ITwitterSamplesFeed source, ITweetProjector projector, ILogger<TweetsCounterBackgroundService> logger)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _projector = projector ?? throw new ArgumentNullException(nameof(projector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Tweets counting service has started");
            var exceptionCounter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var internalTokenSource = new CancellationTokenSource();
                    var internalToken = internalTokenSource.Token;
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalToken, stoppingToken);
                        await foreach (var tweet in _source.GetTweetsAsync(linkedCts.Token))
                        {
                            exceptionCounter = 0;
                        try
                        {
                            await _projector.ProjectAsync(tweet, linkedCts.Token);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "can't process tweet");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Something wrong with the Twitter");
                    if (exceptionCounter < 6)
                    {
                        exceptionCounter++;
                    }

                    var delay = 1 << exceptionCounter;
                    _logger.LogWarning("Delaying for {delay} seconds before re-try", delay);
                    await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
                }   
            }
            _logger.LogInformation("Tweets counting service has stopped");
        }
    }
}
