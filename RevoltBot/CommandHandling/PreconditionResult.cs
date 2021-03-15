namespace RevoltBot.CommandHandling
{
    public class PreconditionResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        public PreconditionResult(bool isSuccess, string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        public static PreconditionResult FromSuccess() => new PreconditionResult(true, null);
        public static PreconditionResult FromSuccess(string message) => new PreconditionResult(true, message);

        public static PreconditionResult FromError() => new PreconditionResult(false, null);
        public static PreconditionResult FromError(string message) => new PreconditionResult(false, message);
    }
}