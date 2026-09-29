using MemberCommitment.API.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VirginActiveAssignment;

namespace MemberCommitment.API.Tests.Middleware
{
    /// <summary>
    /// 4. Correlation IDs, log levels, structured logs and request outcome logging.
    /// </summary>
    public class CorrelationIdMiddlewareTests
    {
        private const string HeaderName = "X-Correlation-Id";

        private readonly CapturingLoggerProvider _logs = new();
        private readonly ILoggerFactory _loggerFactory;

        public CorrelationIdMiddlewareTests()
        {
            _loggerFactory = LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Trace).AddProvider(_logs));
        }

        [Fact]
        public async Task InvokeAsync_HeaderPresent_UsesIncomingCorrelationId()
        {
            var context = CreateContext("incoming-correlation-id");

            await CreateMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

            Assert.Equal("incoming-correlation-id", context.Response.Headers[HeaderName].ToString());
        }

        [Fact]
        public async Task InvokeAsync_HeaderMissing_GeneratesCorrelationId()
        {
            var context = CreateContext(correlationId: null);

            await CreateMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

            Assert.False(string.IsNullOrWhiteSpace(context.Response.Headers[HeaderName].ToString()));
        }

        [Fact]
        public async Task InvokeAsync_EveryLogEntry_IncludesCorrelationId()
        {
            var downstreamLogger = _loggerFactory.CreateLogger("Downstream");
            var middleware = CreateMiddleware(_ =>
            {
                downstreamLogger.LogWarning("Rock {RockId} not found", Guid.NewGuid());
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(CreateContext("abc-123"));

            Assert.NotEmpty(_logs.Logs);
            Assert.All(_logs.Logs, entry => Assert.Equal("abc-123", entry.ScopeProperties["CorrelationId"]));
        }

        [Fact]
        public async Task InvokeAsync_LogsOutcomeAsStructuredEntryWithStatusCodeAndDuration()
        {
            await CreateMiddleware(ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status201Created;
                return Task.CompletedTask;
            }).InvokeAsync(CreateContext("abc-123"));

            var entry = CompletionLog();
            Assert.Contains("{StatusCode}", entry.OriginalFormat);
            Assert.Equal(201, entry.Properties["StatusCode"]);
            Assert.Contains(entry.Properties.Keys, k => k.Contains("duration", StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData(StatusCodes.Status200OK, LogLevel.Information)]
        [InlineData(StatusCodes.Status404NotFound, LogLevel.Warning)]
        [InlineData(StatusCodes.Status500InternalServerError, LogLevel.Error)]
        public async Task InvokeAsync_OutcomeLogLevel_ReflectsStatusCode(int statusCode, LogLevel expectedLevel)
        {
            await CreateMiddleware(ctx =>
            {
                ctx.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            }).InvokeAsync(CreateContext("abc-123"));

            Assert.Equal(expectedLevel, CompletionLog().Level);
        }

        private CorrelationIdMiddleware CreateMiddleware(RequestDelegate next) =>
            new(next, _loggerFactory.CreateLogger<CorrelationIdMiddleware>());

        private static DefaultHttpContext CreateContext(string? correlationId)
        {
            var context = new DefaultHttpContext();
            if (correlationId != null)
            {
                context.Request.Headers[HeaderName] = correlationId;
            }
            return context;
        }

        private CapturedLog CompletionLog() =>
            _logs.Logs.Last(l =>
                l.Category == typeof(CorrelationIdMiddleware).FullName &&
                l.Properties.ContainsKey("StatusCode"));
    }
}
