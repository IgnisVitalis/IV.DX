namespace IV.DX.Kernel
{
    public static class DXMigrationContext
    {
        private static bool _isMigrating;

        public static bool IsMigrating => _isMigrating;

        public static void Start()
        {
            if (!_isMigrating)
                _isMigrating = true;
        }

        public static void Stop()
        {
            if (_isMigrating)
                _isMigrating = false;
        }
    }
}
