using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MemberCommitment.API.Tests.TestSupport;
using VirginActiveAssignment.Models.Enums;

namespace MemberCommitment.API.Tests.Integration
{
    /// <summary>
    /// Most critical behaviour: a member creates a rock, completes it, and a completed rock cannot be reopened.
    /// Exercises the full pipeline: API key, validation, persistence, state transitions and Problem Details.
    /// </summary>
    public class RockLifecycleTests : IClassFixture<ApiFactory>
    {
        private const string RocksUrl = "/api/members/1/rocks";
        private readonly HttpClient _client;

        public RockLifecycleTests(ApiFactory factory)
        {
            _client = factory.CreateAuthorisedClient();
        }

        [Fact]
        public async Task Rock_IsCreatedAsPending_CanBeCompleted_AndCannotBeReopened()
        {
            // Create
            var createResponse = await _client.PostAsJsonAsync(RocksUrl, RockRequests.Valid(RockCategoryEnum.Career));
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await ReadJson(createResponse);
            var rockId = created.GetProperty("rockId").GetGuid();
            Assert.Equal(nameof(RockStatusEnum.Pending), created.GetProperty("status").GetString());

            // Complete
            var completeResponse = await _client.PatchAsync($"{RocksUrl}/{rockId}?rockStatus={RockStatusEnum.Completed}", null);
            Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
            Assert.Equal(nameof(RockStatusEnum.Completed), await GetStatus(rockId));

            // Reopening is rejected and the status is unchanged
            var reopenResponse = await _client.PatchAsync($"{RocksUrl}/{rockId}?rockStatus={RockStatusEnum.Pending}", null);
            await ProblemDetailsAssert.IsProblem(reopenResponse, HttpStatusCode.UnprocessableEntity);
            Assert.Equal(nameof(RockStatusEnum.Completed), await GetStatus(rockId));
        }

        private async Task<string?> GetStatus(Guid rockId)
        {
            var rocks = await ReadJson(await _client.GetAsync(RocksUrl));
            return rocks.EnumerateArray()
                .Single(rock => rock.GetProperty("rockId").GetGuid() == rockId)
                .GetProperty("status").GetString();
        }

        private static async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
            JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }
}
