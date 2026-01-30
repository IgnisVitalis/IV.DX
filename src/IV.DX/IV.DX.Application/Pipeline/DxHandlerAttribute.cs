namespace IV.DX.Application.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DXHandlerAttribute : Attribute
    {
        public DXHandlerAttribute(string? category = null, bool unique = false)
        {
            Category = category; Unique = unique;
        }
        public string? Category { get; }
        public bool Unique { get; }
    }
}
