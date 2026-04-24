using IV.DX.Kernel.Attributes;

namespace IV.DX.Kernel.Models
{
    [DXUnit("DXMigrationScriptsUnit")]
    internal class DXMigrationScriptsUnit : DXUnit
    {
        [DXColumn("FilePath")]
        public string FilePath { get; set; } = null!;
        [DXColumn("Version")]
        public string Version { get; set; } = null!;
        [DXColumn("Build")]
        public string Build { get; set; } = null!;
        [DXColumn("Number")]
        public string Number { get; set; } = null!;
        [DXColumn("Module")]
        public string Module { get; set; } = null!;
        [DXColumn("Name")]
        public string Name { get; set; } = null!;
        [DXColumn("Extension")]
        public string Extension { get; set; } = null!;
        [DXColumn("Content")]
        public string Content { get; set; } = null!;


        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            var obj2 = obj as DXMigrationScriptsUnit;

            if (obj2 == null)
                return false;

            return this.GetHashCode() == obj2.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Version}_{Build}_{Number}_{Module}_{Name}.{Extension}";
        }
    }
}
