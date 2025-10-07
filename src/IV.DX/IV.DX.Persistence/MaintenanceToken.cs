namespace IV.DX.Persistence
{
    internal static class MaintenanceToken
    {
        public static bool IsCoreInitializing { get; private set; }

        static MaintenanceToken()
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
