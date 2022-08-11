using System.Net.Http.Headers;
using System.Reflection;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using StreamingDemo.Api;
using StreamingDemo.Api.Infrastructure;
using StreamingDemo.Api.Publisher.Features;
using StreamingDemo.Api.Publisher.Services;
using StreamingDemo.Api.RankedHashtags.Projectors;
using StreamingDemo.Api.RankedHashtags.Services;
using StreamingDemo.Api.TwitterClient;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVersionedApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});
builder.Services.AddApiVersioning(
    options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = ApiVersionReader.Combine(new HeaderApiVersionReader("api-version"),
            new UrlSegmentApiVersionReader());
    }
);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(typeof(Program).GetTypeInfo().Assembly);

var projector = new ConcurrentTweetProjector();
builder.Services.AddHttpClient("Twitter", client =>
    {
        var twitterOptions = new TwitterOptions();
        builder.Configuration.GetSection("Twitter").Bind(twitterOptions);
        client.BaseAddress = new Uri(twitterOptions.Uri);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", twitterOptions.BearerToken);
    })
    .AddPolicyHandler((services, request) => HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            },
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                context.GetLogger()?.LogWarning("Delaying for {delay}ms, then making retry {retry}.", timespan.TotalMilliseconds, retryAttempt); 
            })
    );

builder.Services.AddSingleton<ITweetsStatisticsStore>(p => projector);
builder.Services.AddSingleton<ITweetProjector>(p => projector);

builder.Services.AddSingleton<ITwitterSamplesFeed, TwitterSamplesFeedSource>();

builder.Services.AddSignalR();
builder.Services.AddHostedService<TweetsCounterBackgroundService>();
builder.Services.AddHostedService<NotifyService>();
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (context, ex) =>
    {
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
        return env.IsDevelopment() || env.IsStaging();
    };

});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        configurePolicy => configurePolicy
        .SetIsOriginAllowed((host) => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});


builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var app = builder.Build();
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    Log.Logger.Information($"assigned port={port}");
    app.Urls.Add("http://*:" + port);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())  // in prod too
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

//app.UseAuthorization();

app.MapControllers();

app.MapHub<TopHashtagsPublisherHub>("/hubs/hashtags");

app.Run();

// for integration tests
public partial class Program { }