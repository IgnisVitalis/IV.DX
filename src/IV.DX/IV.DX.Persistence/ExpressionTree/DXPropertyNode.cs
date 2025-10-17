using System.Text.RegularExpressions;

namespace IV.DX.Contracts.Persistence.ExpressionTree
{
    internal class DXPropertyNode : DXBaseNode
    {
        public DXLogicOperation LogicOperation { get; private set; }
        public int ExpressionOrder { get; private set; }

        public string LeftValue { get; private set; }
        public string Operator { get; private set; }
        public string RightValue { get; private set; }

        private DXPropertyNode(int x, int y, string value, int expressionOrder, DXLogicOperation logicOperation)
            : base(x, y, value)
        {
            this.LogicOperation = logicOperation;
            this.ExpressionOrder = expressionOrder;


            bool isBaseOperator = false;

            foreach (var baseOperator in DXSQLOperators.BaseOperators)
            {
                if (base.Value.Contains(baseOperator))
                {
                    var values = base.Value.Split(baseOperator);

                    this.Operator = baseOperator;
                    this.LeftValue = values[0].Trim();
                    this.RightValue = values[1].Trim();

                    isBaseOperator = true;
                    break;
                }
            }

            if (!isBaseOperator)
            {
                if (base.Value.Contains("IN", StringComparison.InvariantCultureIgnoreCase))
                {
                    var values = Regex.Split(base.Value, " IN ", RegexOptions.IgnoreCase);

                    this.Operator = "IN";
                    this.LeftValue = values[0].Trim();
                    this.RightValue = values[1].Trim();
                }
            }
        }

        public static DXPropertyNode CreateInstance(int level, int order, string value, int expressionOrder, DXLogicOperation logicOperation)
        {
            var instance = new DXPropertyNode(level, order, value, expressionOrder, logicOperation);

            return instance;
        }
    }
}