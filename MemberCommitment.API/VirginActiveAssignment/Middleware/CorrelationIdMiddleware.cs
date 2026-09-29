namespace VirginActiveAssignment
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId =
                context.Request.Headers[HeaderName].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            context.Response.Headers[HeaderName] = correlationId;

            using var scope = _logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                });

            var start = DateTime.Now;
            try
            {
                await _next(context);
            }
            finally
            {
                var duration = DateTime.Now - start;
                var logLevel = context.Response.StatusCode switch
                {
                    >= 500 => LogLevel.Error,
                    >= 400 => LogLevel.Warning,
                    _ => LogLevel.Information
                };
                _logger.Log(
                    logLevel,
                    "Request completed. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration:{duration}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration.TotalMilliseconds);
            }
        }
    }
}