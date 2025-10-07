namespace IV.DX.Contracts.Common.Attributes
{
    public class ESQLBlockDefinitionAttribute : Attribute
    {
        public string BlockName { get; private set; }
        public ESQLBlockDefinitionAttribute(string blockName)
        {
            BlockName = blockName;
        }

        public static bool DeepEquals(ESQLBlockDefinitionAttribute item1, ESQLBlockDefinitionAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.BlockName == item2.BlockName;

            return result;
        }

        public ESQLBlockDefinitionAttribute DeepClone()
        {
            return new ESQLBlockDefinitionAttribute(BlockName);
        }
    }
}