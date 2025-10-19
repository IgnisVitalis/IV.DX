namespace IV.DX.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DXUnitAttribute : Attribute
    {
        public string ObjectName { get; set; }
        public DXUnitAttribute(string objectName)
        {
            ObjectName = objectName;
        }

        public static bool DeepEquals(DXUnitAttribute item1, DXUnitAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.ObjectName == item2.ObjectName;

            return result;
        }

        public DXUnitAttribute DeepClone()
        {
            return new DXUnitAttribute(ObjectName);
        }
    }
}