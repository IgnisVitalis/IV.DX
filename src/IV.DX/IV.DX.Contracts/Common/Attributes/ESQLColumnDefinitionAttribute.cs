using IV.DX.Contracts.Common.Enums;

namespace IV.DX.Contracts.Common.Attributes
{
    public class ESQLColumnDefinitionAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public string ESQLExpression { get; private set; }
        public TypeOfEntityLoading TypeOfEntityLoading { get; private set; }

        public ESQLColumnDefinitionAttribute(
            string columnName,
            string esqlExpression = null,
            TypeOfEntityLoading typeOfEntityLoading = TypeOfEntityLoading.Full)
        {
            ColumnName = columnName;
            ESQLExpression = string.IsNullOrEmpty(esqlExpression) ?
                                    ColumnName : esqlExpression;
            TypeOfEntityLoading = typeOfEntityLoading;
        }

        public static bool DeepEquals(ESQLColumnDefinitionAttribute item1, ESQLColumnDefinitionAttribute item2)
        {
            if (item1 == null || item2 == null)
                return false;

            var result = item1.ColumnName == item2.ColumnName && item1.ESQLExpression == item2.ESQLExpression && item1.TypeOfEntityLoading == item2.TypeOfEntityLoading;

            return result;
        }

        public static bool DeepEquals(IDictionary<string, ESQLColumnDefinitionAttribute> dict1, IDictionary<string, ESQLColumnDefinitionAttribute> dict2)
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

        public ESQLColumnDefinitionAttribute DeepClone()
        {
            return new ESQLColumnDefinitionAttribute(ColumnName, ESQLExpression, TypeOfEntityLoading);
        }

        public bool DeepEquals(ESQLColumnDefinitionAttribute columnDefinition)
        {
            if (columnDefinition == null)
                return false;

            if (ColumnName != columnDefinition.ColumnName)
                return false;

            if (ESQLExpression != columnDefinition.ESQLExpression)
                return false;

            if (TypeOfEntityLoading != columnDefinition.TypeOfEntityLoading)
                return false;

            return true;
        }
    }
}