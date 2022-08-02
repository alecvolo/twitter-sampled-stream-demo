using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace StreamingDemo.Api.IntegrationTests.Fixtures;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public IConfiguration Configuration { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets(typeof(ApiWebApplicationFactory).Assembly, true)
                .AddEnvironmentVariables()
                .Build();

            config.AddConfiguration(Configuration);
        });
    }
}