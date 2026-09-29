using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using VirginActiveAssignment.Services;
using VirginActiveAssignment.Services.Exceptions;

namespace MemberCommitment.API.Tests.Middleware
{
    /// <summary>
    /// 3. All unhandled exceptions return RFC 7807 Problem Details with the right status code.
    /// </summary>
    public class ExceptionHandlerTests
    {
        private readonly ExceptionHandler _handler = new(NullLogger<ExceptionHandler>.Instance);

        public static TheoryData<Exception, int> ExceptionMappings => new()
        {
            { new ValidationException("Title must not be empty"), StatusCodes.Status400BadRequest },
            { new NotFoundException("Member does not exist"), StatusCodes.Status404NotFound },
            { new InvalidStateTransitionException("Cannot transition from Completed to Pending"), StatusCodes.Status422UnprocessableEntity },
            { new Exception("Something broke"), StatusCodes.Status500InternalServerError }
        };

        [Theory]
        [MemberData(nameof(ExceptionMappings))]
        public async Task TryHandleAsync_ReturnsProblemDetailsWithMappedStatusCode(Exception exception, int expectedStatus)
        {
            var context = CreateContext();

            await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            Assert.Equal(expectedStatus, context.Response.StatusCode);
            Assert.StartsWith("application/problem+json", context.Response.ContentType);
            var body = JsonDocument.Parse(ReadBody(context)).RootElement;
            Assert.Equal(expectedStatus, body.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("title").GetString()));
        }

        [Fact]
        public async Task TryHandleAsync_UnhandledException_DoesNotReturnStackTrace()
        {
            Exception exception;
            try
            {
                throw new InvalidOperationException("Server=prod-db;Password=hunter2");
            }
            catch (Exception thrown)
            {
                exception = thrown;
            }
            var context = CreateContext();

            await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            var body = ReadBody(context);
            Assert.DoesNotContain("hunter2", body);
            Assert.DoesNotContain(" at ", body);
        }

        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static string ReadBody(HttpContext context)
        {
            context.Response.Body.Position = 0;
            return new StreamReader(context.Response.Body).ReadToEnd();
        }
    }
}
