using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services.Exceptions;

namespace VirginActiveAssignment.Services.Validation
{
    public class CareerValidator : IValidation
    {
        public bool AppliesTo(RockCategoryEnum category) => category == RockCategoryEnum.Career;

        public void Validate(RockRequestModel request)
        {
            var errors = new List<string>();
            if (!HasNotes(request.Note))
            {
                errors.Add("Notes are required for this type");
            }

            if (errors.Any())
            {
                throw new ValidationException(string.Join(Environment.NewLine, errors));
            }
        }

        private bool HasNotes(string notes)
        {
            return !string.IsNullOrWhiteSpace(notes);
        }
    }
}
