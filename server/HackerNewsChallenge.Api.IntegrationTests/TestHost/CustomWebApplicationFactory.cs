using HackerNewsChallenge.Api.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HackerNewsChallenge.Api.IntegrationTests.TestHost;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IHackerNewsClient));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IHackerNewsClient, FakeHackerNewsClient>();
        });
    }
}
