using IV.DX.Kernel.Models;

namespace IV.DX.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class DXInParameterAttribute : Attribute
    {
        public string Key { get; }
        public DXActionParameterTypeEnum Type { get; }
        public bool Required { get; set; }
        public bool IsMulti { get; set; }
        public string? DefaultValue { get; set; }

        public DXInParameterAttribute(string key, DXActionParameterTypeEnum type)
        {
            Key = key;
            Type = type;
        }
    }
}
