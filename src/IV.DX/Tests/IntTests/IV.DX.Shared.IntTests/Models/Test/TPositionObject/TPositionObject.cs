using IV.DataProvider.Persistence.Contracts.Models;
using IV.DX.Contracts.Common.Attributes;
using IV.DX.Contracts.Common.Enums;
using System;

namespace IV.DataProvider.Persistence.Shared.IntTests.Models.Test
{
    [ESQLObjectDefinition("TPositionObject")]
    public class TPositionObject : ESQLObject
    {
        [ESQLColumnDefinition("User", "User", TypeOfEntityLoading.Base)]
        public Guid? User { get; set; }
        public TPositionGenBlock TPositionGenBlock { get; set; }
    }
}