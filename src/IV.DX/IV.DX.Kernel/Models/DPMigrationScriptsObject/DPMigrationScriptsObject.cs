using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
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