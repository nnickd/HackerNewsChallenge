using HackerNewsChallenge.Api.Configuration;
using HackerNewsChallenge.Api.Services;
using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection(HackerNewsOptions.SectionName));

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>();
builder.Services.AddScoped<IStoryService, StoryService>();

var app = builder.Build();

var spaRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "browser");

if (Directory.Exists(spaRoot))
{
    var fileProvider = new PhysicalFileProvider(spaRoot);

    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider
    });
}

app.UseHttpsRedirection();

app.MapControllers();

if (Directory.Exists(spaRoot))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(spaRoot)
    });
}

app.Run();