using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Xunit;

namespace IV.DX.Kernel.UnitTests.Models
{
    public class DXUnitTests
    {
        [Fact]
        public void ToDXRecord_UsingDXUnit_Ok()
        {
            // Init
            var id = Guid.NewGuid();
            var timeStamp = DateTime.UtcNow;

            var dxUnit = new DXElementDefinitionUnit()
            {
                ID = id,
                TimeStamp = timeStamp,
                Kind = DXObjectKindEnum.Core,
                DisplayValue = "Name",
                Name = "NameStr"
            };

            // Action
            var block = IV.DX.Kernel.Converters.DXObjectConverters.DXRecordWriter.ToBlock(dxUnit);

            // Assert
            Assert.NotNull(block);
            Assert.NotNull(block.Data);
            Assert.NotNull(block.Data.Items);
        }
    }
}

