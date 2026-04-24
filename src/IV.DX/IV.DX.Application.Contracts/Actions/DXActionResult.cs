namespace IV.DX.Application.Contracts.Actions
{
    public class DXActionResult
    {
        public bool IsSuccess { get; }
        public string? Message { get; }
        public string? Error { get; }
        public DXActionParameters Output { get; }

        protected DXActionResult(bool isSuccess, string? message, string? error)
        {
            IsSuccess = isSuccess;
            Message = message;
            Error = error;
            Output = new DXActionParameters();
        }

        public static DXActionResult Ok(string? message = null)
            => new(true, message, null);

        public static DXActionResult Fail(string error)
            => new(false, null, error);
    }
}
