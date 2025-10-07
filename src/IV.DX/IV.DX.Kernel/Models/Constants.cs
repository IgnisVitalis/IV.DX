namespace IV.DX.Kernel
{
    public static class Constants
    {
        public static string SystemPropertyPrefix { get; } = "S_";
        public static string SystemPropertyTypeName { get; } = $"{SystemPropertyPrefix}Type";
        public static string ID { get; } = "ID";
        public static string TimeStamp { get; } = "TimeStamp";
        public static string ObjectID { get; } = "ObjectID";
        public static string Announced { get; } = "Announced";
        public static string Deleted { get; } = "Deleted";
        public static string Mode { get; } = "Mode";
    }
}