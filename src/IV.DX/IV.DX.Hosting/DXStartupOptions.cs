namespace IV.DX.Hosting
{
    internal sealed class DXStartupOptions
    {
        public bool SecurityEnabled { get; set; }
        public List<string> CustomDataPaths { get; } = new();
    }
}
