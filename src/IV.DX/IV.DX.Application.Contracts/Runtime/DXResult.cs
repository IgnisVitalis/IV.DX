namespace IV.DX.Application.Contracts.Runtime
{
    public class DXResult
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public DXFlow Flow { get; }
        public DXOutcome Outcome { get; }

        protected DXResult(bool ok, string? error, DXFlow flow, DXOutcome outcome)
        {
            IsSuccess = ok;
            Error = error;
            Flow = flow;
            Outcome = outcome;
        }

        public static DXResult Ok(DXFlow flow = DXFlow.Continue)
            => new(true, null, flow, DXOutcome.Ok);

        public static DXResult NotFound(DXFlow flow = DXFlow.Stop)
            => new(true, null, flow, DXOutcome.NotFound);

        public static DXResult Fail(string error, DXFlow flow = DXFlow.Stop)
            => new(false, error, flow, DXOutcome.Error);

        public static DXResult OkContinue() => Ok(DXFlow.Continue);
        public static DXResult OkSkipProcess() => Ok(DXFlow.SkipProcess);
        public static DXResult OkStop() => Ok(DXFlow.Stop);
    }

    public sealed class DXResult<T> : DXResult
    {
        public T? Value { get; }
        public bool HasValue => Value is not null;

        protected DXResult(bool ok, T? value, string? error, DXFlow flow, DXOutcome outcome)
            : base(ok, error, flow, outcome)
        {
            Value = value;
        }

        public static DXResult<T> Ok(T value, DXFlow flow = DXFlow.Continue)
            => new(true, value, null, flow, DXOutcome.Ok);

        public static DXResult<T?> OkContinue(T value)
            => new(true, value, null, DXFlow.Continue, DXOutcome.Ok);

        public static DXResult<T?> OkSkipProcess(T value)
            => new(true, value, null, DXFlow.SkipProcess, DXOutcome.Ok);

        public static DXResult<T?> OkStop(T? value = default)
            => new(true, value, null, DXFlow.Stop, DXOutcome.Ok);

        public static DXResult<T?> NotFound(DXFlow flow = DXFlow.Stop)
            => new(true, default, null, flow, DXOutcome.NotFound);

        public static new DXResult<T?> Fail(string error, DXFlow flow = DXFlow.Stop)
            => new(false, default, error, flow, DXOutcome.Error);

        public static DXResult<T?> MapFrom<Y>(DXResult<Y> original, T? value)
            => new(original.IsSuccess, value, original.Error, original.Flow, original.Outcome);
    }
}