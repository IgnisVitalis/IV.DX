namespace IV.DX.Application.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DxHandlerAttribute : Attribute
    {
        public DxHandlerAttribute(string? category = null, bool unique = false)
        {
            Category = category; Unique = unique;
        }
        public string? Category { get; }
        public bool Unique { get; }
    }
}
