using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXMigrationScriptsUnit")]
    public class DXMigrationScriptsUnit : DXUnit
    {
        [DXRequired]
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