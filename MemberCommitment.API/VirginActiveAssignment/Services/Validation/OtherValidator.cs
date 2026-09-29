using Microsoft.VisualBasic;
using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services.Exceptions;

namespace VirginActiveAssignment.Services.Validation
{
    public class OtherValidator : IValidation
    {
        // Base rules apply to every category
        public bool AppliesTo(RockCategoryEnum category) => true;

        public void Validate(RockRequestModel request)
        {
            var errors = new List<string>();
            if (!HasEmptyTitle(request.Title))
            {
                errors.Add("Title must not be empty");
            }
            if(!ValidDueDate(request.DueDate))
            {
                errors.Add("dueDate must not be in the past");
            }

            if(!ValidCategory(request.Category))
            {
                errors.Add("Invalid category");
            }
            if(!HasMemberId(request.MemberId))
            {
                errors.Add("memberId must not be empty");
            }

            if (errors.Any())
            {
                throw new ValidationException(string.Join(Environment.NewLine, errors));
            }
        }

        private bool HasEmptyTitle(string title)
        {
            return !string.IsNullOrWhiteSpace(title);
        }
        private bool ValidDueDate(DateTimeOffset dueDate)
        {
            return dueDate >= DateTimeOffset.Now;
        }
        private bool ValidCategory(RockCategoryEnum category)
        {
            return Enum.IsDefined(typeof(RockCategoryEnum), category);
        }
        private bool HasMemberId(int memberId)
        {
            return memberId > 0;
        }
    }
}
