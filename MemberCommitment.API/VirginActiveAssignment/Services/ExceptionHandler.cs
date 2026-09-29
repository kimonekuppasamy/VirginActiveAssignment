
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VirginActiveAssignment.Services.Exceptions;

namespace VirginActiveAssignment.Services
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                ValidationException =>
                    (StatusCodes.Status400BadRequest, "Validation Error"),

                NotFoundException =>
                    (StatusCodes.Status404NotFound, "Not Found"),

                InvalidStateTransitionException =>
                    (StatusCodes.Status422UnprocessableEntity, "Invalid State Transition"),

                _ =>
                    (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            if (statusCode == 500)
            {
                _logger.LogError(exception, "Unhandled exception");
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Request failed with status code {StatusCode}",
                    statusCode);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == 500
                    ? "An unexpected error occurred."
                    : exception.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken);

            return true;
        }
    }
}
