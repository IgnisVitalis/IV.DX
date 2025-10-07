using System.Text;

namespace IV.DX.Contracts.Persistence.Models
{
    public class QueryContainer
    {
        public string SelectExpression { get; set; }
        public string LeftJoinsExpression { get; set; }
        public string WhereExpression { get; set; }

        public string Query
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(SelectExpression);

                if (!string.IsNullOrEmpty(this.LeftJoinsExpression))
                {
                    sb.Append($" {this.LeftJoinsExpression}");
                }

                if (!string.IsNullOrEmpty(this.WhereExpression))
                {
                    sb.Append($" WHERE {this.WhereExpression}");
                }

                sb.Append(";");

                return sb.ToString();
            }
        }
    }
}
