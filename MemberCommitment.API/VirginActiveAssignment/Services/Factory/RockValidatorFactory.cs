using VirginActiveAssignment.Models.Enums;

namespace VirginActiveAssignment.Services.Factory
{
    public class RockValidatorFactory
    {
        private readonly IEnumerable<IValidation> _validators;

        public RockValidatorFactory(IEnumerable<IValidation> validators)
        {
            _validators = validators;
        }

        public IEnumerable<IValidation> GetValidators(RockCategoryEnum category)
        {
            return _validators.Where(validator => validator.AppliesTo(category));
        }
    }
}
