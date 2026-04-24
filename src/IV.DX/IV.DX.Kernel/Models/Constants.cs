namespace IV.DX.Kernel
{
    public static class Constants
    { 
        public static string ID { get; } = "ID";
        public static string DXCustomUnitID(string typeName) => $"{typeName}{ID}";
        public static string TimeStamp { get; } = "TimeStamp";
        public static string DXUnitID { get; } = $"DXUnitID";
        public static string DXUnitType { get; } = "DXUnitType";
        public static string DXTitle { get; } = "DXTitle";
        public static string Announced { get; } = "Announced";
        public static string Deleted { get; } = "Deleted";
        public static string Mode { get; } = "Mode";
        public static Guid DXUnitDefinitionUnitID { get; } = new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5");
        public static string DerivedDXUnitType { get; } = "DerivedDXUnitType";
        public static string[] SystemProperties
        {
            get
            {
                return new[] { ID, DXUnitID, DXUnitType, TimeStamp, DXTitle };
            }
        }
    }
}
