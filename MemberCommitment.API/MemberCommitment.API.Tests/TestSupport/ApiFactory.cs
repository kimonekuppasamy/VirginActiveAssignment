using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VirginActiveAssignment.Services;

namespace MemberCommitment.API.Tests.TestSupport
{
    /// <summary>
    /// Hosts the real API pipeline in memory with a test API key, a stubbed profile service
    /// and a log provider the tests can inspect.
    /// </summary>
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public const string TestApiKey = "test-api-key-from-configuration";

        public CapturingLoggerProvider Logs { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiKey"] = TestApiKey
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient<MemberService>()
                    .ConfigurePrimaryHttpMessageHandler(() => new StubProfileHandler());

                services.AddSingleton<ILoggerProvider>(Logs);
            });
        }

        public HttpClient CreateAuthorisedClient()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
            return client;
        }
    }
}
