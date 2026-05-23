namespace IV.DX.Kernel
{
    public static class Constants
    { 
        public static string Id { get; } = "Id";
        public static string DXCustomUnitId(string typeName) => $"{typeName}{Id}";
        public static string TimeStamp { get; } = "TimeStamp";
        public static string DXUnitId { get; } = $"DXUnitId";
        public static string DXUnitType { get; } = "DXUnitType";
        public static string DXTitle { get; } = "DXTitle";
        public static string Announced { get; } = "Announced";
        public static string Deleted { get; } = "Deleted";
        public static string Mode { get; } = "Mode";
        public static Guid DXUnitDefinitionUnitId { get; } = new Guid("018fa545-a3ce-7500-aabb-5bbf4767a6b1");
        public static string DerivedDXUnitType { get; } = "DerivedDXUnitType";
        public static string[] SystemProperties
        {
            get
            {
                return new[] { Id, DXUnitId, DXUnitType, TimeStamp, DXTitle };
            }
        }
    }
}
