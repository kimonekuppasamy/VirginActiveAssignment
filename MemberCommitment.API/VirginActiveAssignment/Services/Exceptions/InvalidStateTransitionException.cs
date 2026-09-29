namespace VirginActiveAssignment.Services.Exceptions
{
    public class InvalidStateTransitionException : Exception
    {
        public InvalidStateTransitionException(string message)
            : base(message)
        {
        }
    }
}
