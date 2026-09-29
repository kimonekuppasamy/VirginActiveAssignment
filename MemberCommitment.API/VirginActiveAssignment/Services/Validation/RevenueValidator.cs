using Microsoft.VisualBasic;
using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services.Exceptions;

namespace VirginActiveAssignment.Services.Validation
{
    public class RevenueValidator : IValidation
    {
        public bool AppliesTo(RockCategoryEnum category) => category == RockCategoryEnum.Revenue;

        public void Validate(RockRequestModel request)
        {
            var errors = new List<string>();

            var isDueDateValid = IsDueDateWithinCurrentQuarter(request.DueDate);

            if(!isDueDateValid)
            {
                errors.Add("Due date must be within the current quarter.");
            }

            if (errors.Any())
            {
                throw new ValidationException(string.Join(", ", errors));
            }
        }

        private bool IsDueDateWithinCurrentQuarter(DateTimeOffset dueDate)
        {
            var now = DateTimeOffset.UtcNow;

            int quarterStartMonth = ((now.Month - 1) / 3) * 3 + 1;

            var quarterStart = new DateTimeOffset(
                now.Year, quarterStartMonth, 1, 0, 0, 0, TimeSpan.Zero);

            var nextQuarterStart = quarterStart.AddMonths(3);

            return dueDate >= quarterStart && dueDate < nextQuarterStart;
        }
    }
}
