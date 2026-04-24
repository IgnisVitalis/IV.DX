namespace IV.DX.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DXActionAttribute : Attribute
    {
        public string Module { get; }
        public string Key { get; }

        public DXActionAttribute(string module, string key)
        {
            Module = module;
            Key = key;
        }
    }
}
