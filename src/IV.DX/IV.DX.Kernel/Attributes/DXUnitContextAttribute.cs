namespace IV.DX.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class DXUnitContextAttribute : Attribute
    {
        public string ContextTypeName { get; }

        public DXUnitContextAttribute(string contextTypeName)
        {
            ContextTypeName = contextTypeName;
        }
    }
}
