using System.Net;
using System.Text;

namespace MemberCommitment.API.Tests.TestSupport
{
    /// <summary>
    /// Stands in for jsonplaceholder.typicode.com so tests never leave the process.
    /// </summary>
    public sealed class StubProfileHandler : HttpMessageHandler
    {
        public const int ExistingMemberId = 1;
        public const int MissingMemberId = 404;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;

            var response = path switch
            {
                "/users/1" => Json(HttpStatusCode.OK,
                    """{ "id": 1, "name": "Leanne Graham", "username": "Bret", "email": "leanne@example.com" }"""),

                _ => Json(HttpStatusCode.NotFound, "{}")
            };

            return Task.FromResult(response);
        }

        private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }
}
