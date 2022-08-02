using MediatR;
using StreamingDemo.Api.RankedHashtags.Projectors;

namespace StreamingDemo.Api.RankedHashtags.Features
{
    public static class GetTopHastags
    {
        public record Query: IRequest<TweetsStatistics>;

        public class Handler: IRequestHandler<Query, TweetsStatistics>
        {
            private readonly ITweetsStatisticsStore _store;
            private readonly ILogger<Handler> _logger;

            public Handler(ITweetsStatisticsStore store, ILogger<Handler> logger)
            {
                _store = store;
                _logger = logger;
            }
            public Task<TweetsStatistics> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = _store.CurrentStatistics;
                return Task.FromResult(result);
            }
        }
    }
}
