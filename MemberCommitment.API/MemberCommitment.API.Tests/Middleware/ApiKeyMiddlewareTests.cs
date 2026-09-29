using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VirginActiveAssignment.Middleware;

namespace MemberCommitment.API.Tests.Middleware
{
    /// <summary>
    /// 5. Clients must pass a valid API key (read from configuration) in the X-Api-Key header.
    /// </summary>
    public class ApiKeyMiddlewareTests
    {
        private const string HeaderName = "X-Api-Key";
        private const string ConfiguredKey = "key-from-configuration";

        private bool _nextCalled;

        [Fact]
        public async Task InvokeAsync_KeyMatchesConfiguration_CallsNext()
        {
            var context = CreateContext(ConfiguredKey);

            await CreateMiddleware(ConfiguredKey).InvokeAsync(context);

            Assert.True(_nextCalled);
        }

        [Fact]
        public async Task InvokeAsync_MissingKey_Returns401()
        {
            var context = CreateContext(apiKey: null);

            await CreateMiddleware(ConfiguredKey).InvokeAsync(context);

            Assert.False(_nextCalled);
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_InvalidKey_Returns401()
        {
            var context = CreateContext("wrong-key");

            await CreateMiddleware(ConfiguredKey).InvokeAsync(context);

            Assert.False(_nextCalled);
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        private ApiKeyMiddleware CreateMiddleware(string configuredKey)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiKey"] = configuredKey })
                .Build();

            return new ApiKeyMiddleware(_ =>
            {
                _nextCalled = true;
                return Task.CompletedTask;
            }, configuration);
        }

        private static DefaultHttpContext CreateContext(string? apiKey)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            if (apiKey != null)
            {
                context.Request.Headers[HeaderName] = apiKey;
            }
            return context;
        }
    }
}
