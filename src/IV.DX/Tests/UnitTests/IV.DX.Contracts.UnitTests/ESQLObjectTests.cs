using IV.DX.Shared.UnitTests.DXObjects.DXUnits;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class dxUnitectTests
    {
        [Fact]
        public void ConvertToDXRecord_UsingDerivedDXUnit_CorrectDXRecord()
        {
            // Init
            DerivedUnit1 dxUnit = new DerivedUnit1();

            // Action
            var block = IV.DX.Kernel.Converters.DXObjectConverters.DXRecordWriter.ToBlock(dxUnit);

            // Checking result
            Assert.NotNull(block);
        }
    }
}
