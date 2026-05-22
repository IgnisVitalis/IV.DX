using IV.DX.Kernel;

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
            DXMigrationContext.Start();
        }

        public static void StopMaintenanceCore()
        {
            IsCoreInitializing = false;
            DXMigrationContext.Stop();
        }
    }
}
