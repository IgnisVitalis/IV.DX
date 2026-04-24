using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System;

namespace IV.DX.Shared.IntTests.Models.Test
{
    [DXUnit("TUserUnit")]
    public class TUserUnit : DXUnit
    {
        [DXColumn("Manager")]
        public Guid? Manager { get; set; }

        public TUserMainElement TUserMainElement { get; set; }
    }
}