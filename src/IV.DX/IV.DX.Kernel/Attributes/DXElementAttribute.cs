namespace IV.DX.Kernel.Attributes
{
    public class DXElementAttribute : Attribute
    {
        public string Name { get; private set; }
        public DXElementAttribute(string name)
        {
            Name = name;
        }

        public static bool DeepEquals(DXElementAttribute item1, DXElementAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.Name == item2.Name;

            return result;
        }

        public DXElementAttribute DeepClone()
        {
            return new DXElementAttribute(Name);
        }
    }
}