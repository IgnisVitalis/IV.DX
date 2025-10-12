using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Attributes
{
    public class DXColumnAttribute : Attribute
    {
        public string Name { get; private set; }
        public string DXExpression { get; private set; }
        public DXLoadingType TypeOfEntityLoading { get; private set; }

        public DXColumnAttribute(
            string name,
            string dxExpression = null,
            DXLoadingType typeOfEntityLoading = DXLoadingType.Full)
        {
            Name = name;
            DXExpression = string.IsNullOrEmpty(dxExpression) ?
                                    Name : dxExpression;
            TypeOfEntityLoading = typeOfEntityLoading;
        }

        public static bool DeepEquals(DXColumnAttribute item1, DXColumnAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.Name == item2.Name && item1.DXExpression == item2.DXExpression && item1.TypeOfEntityLoading == item2.TypeOfEntityLoading;

            return result;
        }

        public static bool DeepEquals(IDictionary<string, DXColumnAttribute> dict1, IDictionary<string, DXColumnAttribute> dict2)
        {
            if (dict1 == null || dict2 == null)
                return false;

            var result = true;

            foreach (var item1 in dict1)
            {
                if (dict2.ContainsKey(item1.Key))
                {
                    var item2Value = dict2[item1.Key];

                    result = result && DeepEquals(item1.Value, item2Value);
                }
            }

            result = result && dict1.Count == dict2.Count;

            return result;
        }

        public DXColumnAttribute DeepClone()
        {
            return new DXColumnAttribute(Name, DXExpression, TypeOfEntityLoading);
        }

        public bool DeepEquals(DXColumnAttribute columnDefinition)
        {
            if (columnDefinition == null)
                return false;

            if (Name != columnDefinition.Name)
                return false;

            if (DXExpression != columnDefinition.DXExpression)
                return false;

            if (TypeOfEntityLoading != columnDefinition.TypeOfEntityLoading)
                return false;

            return true;
        }
    }
}