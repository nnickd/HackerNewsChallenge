using HackerNewsChallenge.Api.Configuration;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection(HackerNewsOptions.SectionName));

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>();
builder.Services.AddScoped<IStoryService, StoryService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
