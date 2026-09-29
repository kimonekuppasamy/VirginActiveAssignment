using Microsoft.AspNetCore.Mvc;

namespace VirginActiveAssignment.Middleware
{
    public class ApiKeyMiddleware
    {
        private const string HeaderName = "X-Api-Key";

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/openapi"))
            {
                await _next(context);
                return;
            }
            var apiKey = context.Request.Headers[HeaderName].FirstOrDefault();
            var validApiKey = _configuration["ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != validApiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Detail = "A valid API key is required.",
                        Instance = context.Request.Path
                    },
                    options: null,
                    contentType: "application/problem+json");

                return;
            }

            await _next(context);
        }
    }
}
