using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Xunit;

namespace IV.DX.Kernel.UnitTests.Models
{
    public class DXUnitTests
    {
        [Fact]
        public void ToDXModel_UsingDXUnit_Ok()
        {
            // Init
            var id = Guid.NewGuid();
            var timeStamp = DateTime.UtcNow;

            var dxUnit = new DXElementDefinitionUnit()
            {
                ID = id,
                TimeStamp = timeStamp,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id, 
                    TimeStamp = timeStamp,
                    Kind = DXObjectKindEnum.Core,
                    DisplayValue = "DisplayValueStr",
                    Name = "NameStr"
                }
            };

            // Action
            var dxModel = dxUnit.ToDXModel();

            // Assert
        }
    }
}
