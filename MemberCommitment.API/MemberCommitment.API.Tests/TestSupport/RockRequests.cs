using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;

namespace MemberCommitment.API.Tests.TestSupport
{
    public static class RockRequests
    {
        /// <summary>
        /// A request that satisfies the base rules and the extra rules for every category.
        /// </summary>
        public static RockRequestModel Valid(RockCategoryEnum category = RockCategoryEnum.Other) => new()
        {
            MemberId = 1,
            Title = "Close ten new corporate accounts",
            Category = category,
            DueDate = QuarterDates.FutureDateInCurrentQuarter(),
            Note = "This drives the regional growth target."
        };
    }

    public static class QuarterDates
    {
        public static DateTimeOffset CurrentQuarterStart()
        {
            var now = DateTimeOffset.UtcNow;
            var quarterStartMonth = ((now.Month - 1) / 3) * 3 + 1;
            return new DateTimeOffset(now.Year, quarterStartMonth, 1, 0, 0, 0, TimeSpan.Zero);
        }

        public static DateTimeOffset NextQuarterStart() => CurrentQuarterStart().AddMonths(3);

        /// <summary>
        /// Halfway between now and the end of the quarter, so it is always in the future and inside the quarter.
        /// </summary>
        public static DateTimeOffset FutureDateInCurrentQuarter()
        {
            var now = DateTimeOffset.UtcNow;
            return now + (NextQuarterStart() - now) / 2;
        }
    }
}
