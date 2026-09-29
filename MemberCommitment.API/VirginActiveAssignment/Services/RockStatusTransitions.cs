using VirginActiveAssignment.Models.Enums;

namespace VirginActiveAssignment.Services
{
    public static class RockStatusTransitions
    {

        public static bool IsValidTransition(
            RockStatusEnum currentStatus,
            RockStatusEnum newStatus)
        {
            return currentStatus switch
            {
                RockStatusEnum.Pending =>
                    newStatus == RockStatusEnum.Completed ||
                    newStatus == RockStatusEnum.Missed,

                _ => false
            };
        }

    }
}
