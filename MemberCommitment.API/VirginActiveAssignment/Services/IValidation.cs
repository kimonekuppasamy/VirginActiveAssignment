using VirginActiveAssignment.Models;
using VirginActiveAssignment.Models.Enums;

namespace VirginActiveAssignment.Services
{
    public interface IValidation
    {
        bool AppliesTo(RockCategoryEnum category);

        void Validate(RockRequestModel request);
    }
}
