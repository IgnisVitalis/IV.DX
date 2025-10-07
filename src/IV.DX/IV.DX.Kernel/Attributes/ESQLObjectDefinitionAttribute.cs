namespace IV.DX.Kernel.Attributes
{
    public class ESQLObjectDefinitionAttribute : Attribute
    {
        public string ObjectName { get; set; }
        public ESQLObjectDefinitionAttribute(string objectName)
        {
            ObjectName = objectName;
        }

        public static bool DeepEquals(ESQLObjectDefinitionAttribute item1, ESQLObjectDefinitionAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.ObjectName == item2.ObjectName;

            return result;
        }

        public ESQLObjectDefinitionAttribute DeepClone()
        {
            return new ESQLObjectDefinitionAttribute(ObjectName);
        }
    }
}