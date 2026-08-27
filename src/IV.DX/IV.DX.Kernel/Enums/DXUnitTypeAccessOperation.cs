namespace IV.DX.Kernel.Enums
{
    public enum DXUnitTypeAccessOperation
    {
        Read = 1,

        /// <summary>Bringing new instances of a type into existence.</summary>
        Create = 2,

        /// <summary>Modifying instances that already exist.</summary>
        Update = 3,

        Delete = 4
    }
}
