using HackerNewsChallenge.Api.Configuration;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection(HackerNewsOptions.SectionName));

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>();

builder.Services.AddScoped<IStoryService, StoryService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
