namespace IV.DX.Application.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class DXNameAttribute : Attribute
    {
        public string Name { get; }
        public DXNameAttribute(string name) => Name = name;
    }
}