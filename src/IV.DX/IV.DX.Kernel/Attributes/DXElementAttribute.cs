namespace IV.DX.Kernel.Attributes
{
    public class DXElementAttribute : Attribute
    {
        public string BlockName { get; private set; }
        public DXElementAttribute(string blockName)
        {
            BlockName = blockName;
        }

        public static bool DeepEquals(DXElementAttribute item1, DXElementAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.BlockName == item2.BlockName;

            return result;
        }

        public DXElementAttribute DeepClone()
        {
            return new DXElementAttribute(BlockName);
        }
    }
}