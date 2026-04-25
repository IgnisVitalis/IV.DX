using System;

namespace IV.DX.Kernel.Models
{
    public sealed class DXEnumItem
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }

        public string Type { get; set; } = null!;
        public object? Key { get; set; }
        public object? Value { get; set; }
    }
}
