using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLObjectDefinition("TPassportObject")]
    public class TPassportObject : ESQLObject
    {
        [ESQLColumnDefinition("User", "User", TypeOfEntityLoading.Base)]
        public Guid User { get; set; }
        public TPassportGenBlock TPassportGenBlock { get; set; }
    }
}