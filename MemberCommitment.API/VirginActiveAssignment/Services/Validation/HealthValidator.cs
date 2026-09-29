using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services.Exceptions;

namespace VirginActiveAssignment.Services.Validation
{
    public class HealthValidator : IValidation
    {
        public bool AppliesTo(RockCategoryEnum category) => category == RockCategoryEnum.Health;

        public void Validate(RockRequestModel request)
        {
            var errors = new List<string>();
            if (!IsTitleCorrectLength(request.Title))
            {
                errors.Add("Title must be at least 10 characters long.");
            }

            if (errors.Any())
            {
                throw new ValidationException(string.Join(Environment.NewLine, errors));
            }
        }

        private bool IsTitleCorrectLength(string title)
        {
            return !string.IsNullOrWhiteSpace(title) && title.Length >= 10;
        }
    }
}
