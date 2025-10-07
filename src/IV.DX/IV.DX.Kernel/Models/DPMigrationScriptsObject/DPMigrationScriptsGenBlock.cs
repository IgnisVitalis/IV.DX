using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPMigrationScriptsGenBlock")]
    public class DPMigrationScriptsGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("FilePath")]
        public string FilePath { get; set; }
        [ESQLColumnDefinition("Version")]
        public string Version { get; set; }
        [ESQLColumnDefinition("Build")]
        public string Build { get; set; }
        [ESQLColumnDefinition("Number")]
        public string Number { get; set; }
        [ESQLColumnDefinition("AppName")]
        public string AppName { get; set; }
        [ESQLColumnDefinition("Name")]
        public string Name { get; set; }
        [ESQLColumnDefinition("Extention")]
        public string Extention { get; set; }


        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var obj2 = obj as DPMigrationScriptsGenBlock;

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