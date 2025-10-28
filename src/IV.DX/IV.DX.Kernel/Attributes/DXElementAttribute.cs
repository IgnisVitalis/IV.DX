namespace IV.DX.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DXElementAttribute : Attribute
    {
        public string Type { get; private set; }
        public DXElementAttribute(string type)
        {
            Type = type;
        }

        public static bool DeepEquals(DXElementAttribute item1, DXElementAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.Type == item2.Type;

            return result;
        }

        public DXElementAttribute DeepClone()
        {
            return new DXElementAttribute(Type);
        }
    }
}