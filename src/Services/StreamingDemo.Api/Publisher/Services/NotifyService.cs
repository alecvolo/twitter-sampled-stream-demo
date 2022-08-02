using Microsoft.AspNetCore.SignalR;
using StreamingDemo.Api.Publisher.Features;
using StreamingDemo.Api.RankedHashtags.Projectors;

namespace StreamingDemo.Api.Publisher.Services;

public class NotifyService : BackgroundService
{
    private readonly ITweetsStatisticsStore _store;
    private readonly ILogger<NotifyService> _logger;
    private readonly IHubContext<TopHashtagsPublisherHub> _hub;
    public int MillisecondsDelay { get; set; } = 600;

    public NotifyService(ITweetsStatisticsStore store, IHubContext<TopHashtagsPublisherHub> hub, ILogger<NotifyService> logger)
    {
        _store = store;
        _logger = logger;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // to warm up
        await Task.Delay(MillisecondsDelay, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var data = _store.CurrentStatistics;
            _logger.LogDebug("Pushing update at: {Time}, {data}", DateTimeOffset.Now, data);
            await _hub.Clients.All.SendCoreAsync("Show", new object?[] { data.TweetsCount, data.TopHashtags }, stoppingToken);
            await Task.Delay(MillisecondsDelay, stoppingToken);
        }
    }
}