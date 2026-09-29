using VirginActiveAssignment.Models.Enums;
using VirginActiveAssignment.Services;

namespace MemberCommitment.API.Tests
{
    /// <summary>
    /// A rock starts as Pending and can only move to Completed or Missed. Completed and Missed are final.
    /// </summary>
    public class RockStatusTransitionsTests
    {
        [Theory]
        [InlineData(RockStatusEnum.Pending, RockStatusEnum.Completed)]
        [InlineData(RockStatusEnum.Pending, RockStatusEnum.Missed)]
        public void IsValidTransition_FromPending_IsAllowed(RockStatusEnum current, RockStatusEnum next)
        {
            Assert.True(RockStatusTransitions.IsValidTransition(current, next));
        }

        [Fact]
        public void IsValidTransition_PendingToPending_IsRejected()
        {
            Assert.False(RockStatusTransitions.IsValidTransition(RockStatusEnum.Pending, RockStatusEnum.Pending));
        }

        [Theory]
        [InlineData(RockStatusEnum.Completed, RockStatusEnum.Pending)]
        [InlineData(RockStatusEnum.Completed, RockStatusEnum.Missed)]
        [InlineData(RockStatusEnum.Completed, RockStatusEnum.Completed)]
        [InlineData(RockStatusEnum.Missed, RockStatusEnum.Pending)]
        [InlineData(RockStatusEnum.Missed, RockStatusEnum.Completed)]
        [InlineData(RockStatusEnum.Missed, RockStatusEnum.Missed)]
        public void IsValidTransition_FromFinalStatus_IsRejected(RockStatusEnum current, RockStatusEnum next)
        {
            Assert.False(RockStatusTransitions.IsValidTransition(current, next));
        }
    }
}
