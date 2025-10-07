using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLObjectDefinition("DXMigrationScriptsUnit")]
    public class DXMigrationScriptsUnit : ESQLObject
    {
        public DXMigrationScriptsMainElement DXMigrationScriptsMainElement { get; set; }

        public override string ToString()
        {
            string result = null;

            if (this.DXMigrationScriptsMainElement != null)
            {
                result = this.DXMigrationScriptsMainElement.ToString();
            }

            return result;
        }
    }
}