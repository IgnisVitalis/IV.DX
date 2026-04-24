namespace IV.DX.Application.Contracts.Actions
{
    public abstract class DXUnitActionBase : DXActionBase
    {
        public sealed override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
        {
            var unitId = input.GetUnitId();
            var unitType = input.GetUnitType();
            return ExecuteAsync(unitId, unitType, input, ct);
        }

        protected abstract Task<DXActionResult> ExecuteAsync(
            Guid unitId,
            string unitType,
            DXActionParameters parameters,
            CancellationToken ct);
    }
}
