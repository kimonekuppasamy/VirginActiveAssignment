using System.Net;
using System.Net.Http.Json;
using MemberCommitment.API.Tests.TestSupport;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services.Exceptions;
using VirginActiveAssignment.Services.Validation;

namespace MemberCommitment.API.Tests.Validation
{
    /// <summary>
    /// 1. Validate all incoming requests and return clear, structured errors.
    /// </summary>
    public class BaseValidationTests : IClassFixture<ApiFactory>
    {
        private readonly OtherValidator _validator = new();
        private readonly HttpClient _client;

        public BaseValidationTests(ApiFactory factory)
        {
            _client = factory.CreateAuthorisedClient();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_TitleEmptyOrWhitespace_Throws(string title)
        {
            var request = RockRequests.Valid();
            request.Title = title;

            Assert.Throws<ValidationException>(() => _validator.Validate(request));
        }

        [Fact]
        public void Validate_DueDateInPast_Throws()
        {
            var request = RockRequests.Valid();
            request.DueDate = DateTimeOffset.UtcNow.AddDays(-1);

            Assert.Throws<ValidationException>(() => _validator.Validate(request));
        }

        [Fact]
        public void Validate_CategoryNotOneOfTheFourValidValues_Throws()
        {
            var request = RockRequests.Valid();
            request.Category = (RockCategoryEnum)99;

            Assert.Throws<ValidationException>(() => _validator.Validate(request));
        }

        [Fact]
        public void Validate_MemberIdEmpty_Throws()
        {
            var request = RockRequests.Valid();
            request.MemberId = default;

            Assert.Throws<ValidationException>(() => _validator.Validate(request));
        }

        [Fact]
        public async Task CreateRock_ValidationFailure_Returns400WithStructuredErrorBody()
        {
            var request = RockRequests.Valid();
            request.DueDate = DateTimeOffset.UtcNow.AddDays(-1);

            var response = await _client.PostAsJsonAsync("/api/members/1/rocks", request);

            await ProblemDetailsAssert.IsProblem(response, HttpStatusCode.BadRequest);
        }
    }
}
