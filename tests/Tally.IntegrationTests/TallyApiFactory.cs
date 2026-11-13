using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Tally.IntegrationTests;

public sealed class TallyApiFactory : WebApplicationFactory<Program>
{
    public const string ApiKey = "test-api-key";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?> { ["ApiKey"] = ApiKey }));
    }
}
