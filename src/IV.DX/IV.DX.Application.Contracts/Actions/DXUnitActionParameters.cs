namespace IV.DX.Application.Contracts.Actions
{
    public static class DXUnitActionParameters
    {
        public static Guid GetUnitId(this DXActionParameters p) => p.Get<Guid>("UnitId");
        public static string GetUnitType(this DXActionParameters p) => p.Get<string>("UnitType") ?? string.Empty;

        public static DXActionParameters SetUnitId(this DXActionParameters p, Guid id) => p.Set("UnitId", id);
        public static DXActionParameters SetUnitType(this DXActionParameters p, string type) => p.Set("UnitType", type);
    }
}
