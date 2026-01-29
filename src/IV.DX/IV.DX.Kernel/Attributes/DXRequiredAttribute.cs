namespace IV.DX.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class DXRequiredAttribute : Attribute
    {
        public bool IsRequired { get; }

        public DXRequiredAttribute(bool isRequired = true)
        {
            IsRequired = isRequired;
        }
    }
}
