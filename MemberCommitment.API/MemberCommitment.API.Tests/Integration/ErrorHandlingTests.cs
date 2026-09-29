using System.Net;
using System.Net.Http.Json;
using MemberCommitment.API.Tests.TestSupport;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;

namespace MemberCommitment.API.Tests.Integration
{
    /// <summary>
    /// 3. Exceptions thrown from endpoints are handled globally and mapped to 404 / 422.
    /// </summary>
    public class ErrorHandlingTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private readonly HttpClient _client;

        public ErrorHandlingTests(ApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateAuthorisedClient();
        }

        [Fact]
        public async Task NotFound_Returns404ProblemDetails()
        {
            var request = RockRequests.Valid();
            request.MemberId = StubProfileHandler.MissingMemberId;

            var response = await _client.PostAsJsonAsync($"/api/members/{StubProfileHandler.MissingMemberId}/rocks", request);

            await ProblemDetailsAssert.IsProblem(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task InvalidStateTransition_Returns422ProblemDetails()
        {
            var rockId = SeedRock(memberId: 3, RockStatusEnum.Completed);

            var response = await _client.PatchAsync($"/api/members/3/rocks/{rockId}?rockStatus={RockStatusEnum.Pending}", null);

            await ProblemDetailsAssert.IsProblem(response, HttpStatusCode.UnprocessableEntity);
        }

        private Guid SeedRock(int memberId, RockStatusEnum status)
        {
            var rock = new RockModel
            {
                RockId = Guid.NewGuid(),
                MemberId = memberId,
                Title = "Seeded rock",
                Category = RockCategoryEnum.Other,
                DueDate = DateTimeOffset.UtcNow.AddDays(10),
                Status = status
            };

            var cache = _factory.Services.GetRequiredService<IMemoryCache>();
            cache.Set($"RockMember_{memberId}", new List<RockModel> { rock });

            return rock.RockId;
        }
    }
}
