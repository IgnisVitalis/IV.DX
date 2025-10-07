namespace IV.DX.Persistence
{
    internal static class DXMaintenanceToken
    {
        public static bool IsCoreInitializing { get; private set; }

        static DXMaintenanceToken()
        {

        }

        public static void StartMaintenanceCore()
        {
            IsCoreInitializing = true;
        }

        public static void StopMaintenanceCore()
        {
            IsCoreInitializing = false;
        }
    }
}
