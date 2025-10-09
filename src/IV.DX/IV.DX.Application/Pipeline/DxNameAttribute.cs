namespace IV.DX.Application.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class DxNameAttribute : Attribute
    {
        public string Name { get; }
        public DxNameAttribute(string name) => Name = name;
    }
}