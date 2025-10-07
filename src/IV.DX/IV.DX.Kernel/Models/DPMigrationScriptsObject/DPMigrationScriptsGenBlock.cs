using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXMigrationScriptsMainElement")]
    public class DXMigrationScriptsMainElement : ESQLBlock
    {
        [DXColumn("FilePath")]
        public string FilePath { get; set; }
        [DXColumn("Version")]
        public string Version { get; set; }
        [DXColumn("Build")]
        public string Build { get; set; }
        [DXColumn("Number")]
        public string Number { get; set; }
        [DXColumn("AppName")]
        public string AppName { get; set; }
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Extention")]
        public string Extention { get; set; }


        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var obj2 = obj as DXMigrationScriptsMainElement;

            if (obj2 == null)
                return false;

            return this.GetHashCode() == obj2.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Version}_{Build}_{Number}_{AppName}_{Name}.{Extention}";
        }
    }
}