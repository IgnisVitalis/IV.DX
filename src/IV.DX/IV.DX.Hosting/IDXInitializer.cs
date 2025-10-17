namespace IV.DX.Hosting
{
    public interface IDXInitializer
    {
        void InitCoreData();
        void InitCustomData(string configPath);
        void InitCache();
    }
}
