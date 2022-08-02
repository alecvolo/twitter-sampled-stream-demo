using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StreamingDemo.Api.Publisher.Features
{
    [AllowAnonymous]
    public class TopHashtagsPublisherHub : Hub
    {
        [AllowAnonymous]
        public async Task SendTimeToClients(ulong count, string[] tags)
        {
            await Clients.All.SendAsync("Show", count, tags);
        }
    }
}
