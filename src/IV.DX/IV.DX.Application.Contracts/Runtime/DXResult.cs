namespace IV.DX.Application.Contracts.Runtime
{
    public class DXResult
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public DXFlow Flow { get; }

        protected DXResult(bool ok, string? error, DXFlow flow)
        {
            IsSuccess = ok;
            Error = error;
            Flow = flow;
        }

        public static DXResult Ok(DXFlow flow = DXFlow.Continue) => new(true, null, flow);
        public static DXResult OkContinue() => new(true, null, DXFlow.Continue);
        public static DXResult Fail(string error) => new(false, error, DXFlow.Stop);
        public static DXResult OkSkipProcess() => new(true, null, DXFlow.SkipProcess);
        public static DXResult OkStop() => new(true, null, DXFlow.Stop);
    }

    public sealed class DXResult<T> : DXResult
    {
        public T? Value { get; }

        protected DXResult(bool ok, T? value, string? error, DXFlow flow)
            : base(ok, error, flow) { Value = value; }

        public static DXResult<T> Ok(T value, DXFlow flow = DXFlow.Continue)
            => new(true, value, null, flow);

        public static DXResult<T> OkContinue(T value)
            => new(true, value, null, DXFlow.Continue);

        public static DXResult<T> OkSkipProcess(T value)
            => new(true, value, null, DXFlow.SkipProcess);

        public static DXResult<T> OkStop(T value)
            => new(true, value, null, DXFlow.Stop);

        public static new DXResult<T> Fail(string error)
            => new(false, default, error, DXFlow.Stop);

        public static new DXResult<T> MapFrom<Y>(DXResult<Y> original, T value)
        {
            return new DXResult<T>(original.IsSuccess, value, original.Error, original.Flow);
        }
    }
}
