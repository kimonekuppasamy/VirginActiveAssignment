using System.Net;
using System.Text.Json;

namespace MemberCommitment.API.Tests.TestSupport
{
    public static class ProblemDetailsAssert
    {
        /// <summary>
        /// Asserts the response is an RFC 7807 Problem Details document with the expected status
        /// and returns its parsed body.
        /// </summary>
        public static async Task<JsonElement> IsProblem(HttpResponseMessage response, HttpStatusCode expectedStatus)
        {
            var raw = await response.Content.ReadAsStringAsync();

            Assert.True(expectedStatus == response.StatusCode,
                $"Expected {(int)expectedStatus} but got {(int)response.StatusCode}. Body: {raw}");
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var body = JsonDocument.Parse(raw).RootElement;
            Assert.Equal((int)expectedStatus, body.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("title").GetString()));
            Assert.DoesNotContain(" at VirginActiveAssignment.", raw);
            Assert.DoesNotContain("stackTrace", raw, StringComparison.OrdinalIgnoreCase);
            return body;
        }
    }
}
