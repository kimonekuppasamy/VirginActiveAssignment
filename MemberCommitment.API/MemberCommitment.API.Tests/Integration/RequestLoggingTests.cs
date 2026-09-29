using MemberCommitment.API.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using VirginActiveAssignment;
using VirginActiveAssignment.Models.Enums;

namespace MemberCommitment.API.Tests.Integration
{
    /// <summary>
    /// 4. Logging across the full request pipeline.
    /// </summary>
    public class RequestLoggingTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private readonly HttpClient _client;

        public RequestLoggingTests(ApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateAuthorisedClient();
        }

        [Fact]
        public async Task FailingRequest_EveryLogEntryIncludesCorrelationIdAndOutcomeHasFinalStatusCode()
        {
            // Warm up so start-up logs are out of the way.
            await _client.GetAsync("/api/members/1/rocks");
            _factory.Logs.Clear();

            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/members/424242/rocks/{Guid.NewGuid()}?rockStatus={RockStatusEnum.Completed}");
            request.Headers.Add("X-Correlation-Id", "failing-request");
            await _client.SendAsync(request);

            var logs = _factory.Logs.Logs;
            Assert.All(logs, entry => Assert.Equal("failing-request", entry.ScopeProperties["CorrelationId"]));

            var outcome = logs.Last(l =>
                l.Category == typeof(CorrelationIdMiddleware).FullName &&
                l.Properties.ContainsKey("StatusCode"));
            Assert.Equal(404, outcome.Properties["StatusCode"]);
        }

        [Fact]
        public void ConsoleLogging_IsStructuredJson()
        {
            var options = _factory.Services.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;

            Assert.Equal(ConsoleFormatterNames.Json, options.FormatterName);
        }
    }
}
