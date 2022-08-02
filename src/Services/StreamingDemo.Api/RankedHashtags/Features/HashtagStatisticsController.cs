using MediatR;
using Microsoft.AspNetCore.Mvc;
using StreamingDemo.Api.RankedHashtags.Projectors;

namespace StreamingDemo.Api.RankedHashtags.Features
{
    [Route("api/v{version:apiVersion}/statistics")]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class HashtagStatisticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HashtagStatisticsController> _logger;

        public HashtagStatisticsController(IMediator mediator, ILogger<HashtagStatisticsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet]
        [Route("top-hashtags")]
        public async Task<ActionResult<TweetsStatistics>> GetTopHashtags(CancellationToken cancellationToken = default)
        {
            return Ok(await _mediator.Send(new GetTopHastags.Query(), cancellationToken));
        }
    }
}
