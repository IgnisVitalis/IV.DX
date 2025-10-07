using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;

namespace IV.DX.Contracts.Common.Models
{
    [ESQLObjectDefinition("DPMigrationScriptsObject")]
    public class DPMigrationScriptsObject : ESQLObject
    {
        public DPMigrationScriptsGenBlock DPMigrationScriptsGenBlock { get; set; }

        public override string ToString()
        {
            string result = null;

            if (this.DPMigrationScriptsGenBlock != null)
            {
                result = this.DPMigrationScriptsGenBlock.ToString();
            }

            return result;
        }
    }
}