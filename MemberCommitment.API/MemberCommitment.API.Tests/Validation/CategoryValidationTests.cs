using MemberCommitment.API.Tests.TestSupport;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services.Exceptions;
using VirginActiveAssignment.Services.Validation;

namespace MemberCommitment.API.Tests.Validation
{
    /// <summary>
    /// 2. Additional validation rules per category.
    /// </summary>
    public class CategoryValidationTests
    {
        [Fact]
        public void Revenue_DueDateWithinCurrentQuarter_DoesNotThrow()
        {
            var request = RockRequests.Valid(RockCategoryEnum.Revenue);
            request.DueDate = QuarterDates.FutureDateInCurrentQuarter();

            var exception = Record.Exception(() => new RevenueValidator().Validate(request));

            Assert.Null(exception);
        }

        [Fact]
        public void Revenue_DueDateOutsideCurrentQuarter_Throws()
        {
            var request = RockRequests.Valid(RockCategoryEnum.Revenue);
            request.DueDate = QuarterDates.NextQuarterStart();

            Assert.Throws<ValidationException>(() => new RevenueValidator().Validate(request));
        }

        [Fact]
        public void Health_TitleShorterThanTenCharacters_Throws()
        {
            var request = RockRequests.Valid(RockCategoryEnum.Health);
            request.Title = "Gym 3x/wk"; // 9 characters

            Assert.Throws<ValidationException>(() => new HealthValidator().Validate(request));
        }

        [Fact]
        public void Health_TitleAtLeastTenCharacters_DoesNotThrow()
        {
            var request = RockRequests.Valid(RockCategoryEnum.Health);
            request.Title = "Gym 3x/wk!"; // 10 characters

            var exception = Record.Exception(() => new HealthValidator().Validate(request));

            Assert.Null(exception);
        }

        [Fact]
        public void Career_NoteMissing_Throws()
        {
            var request = RockRequests.Valid(RockCategoryEnum.Career);
            request.Note = null;

            Assert.Throws<ValidationException>(() => new CareerValidator().Validate(request));
        }

        [Fact]
        public void Other_HasNoRulesBeyondBaseValidation()
        {
            // Would fail Health (short title), Career (no note) and Revenue (outside quarter).
            var request = RockRequests.Valid(RockCategoryEnum.Other);
            request.Title = "Read";
            request.Note = null;
            request.DueDate = DateTimeOffset.UtcNow.AddYears(2);

            var exception = Record.Exception(() => new OtherValidator().Validate(request));

            Assert.Null(exception);
        }
    }
}
