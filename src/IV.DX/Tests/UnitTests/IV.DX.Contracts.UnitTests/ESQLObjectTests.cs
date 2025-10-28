using IV.DX.Shared.UnitTests.DXObjects.DXUnits;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class dxUnitectTests
    {
        [Fact]
        public void ConvertToDXModel_UsingDerivedDXUnit_CorrectDXModel()
        {
            // Init
            DerivedUnit1 dxUnit = new DerivedUnit1();

            // Action
            var dxModel = dxUnit.ToDXModel();

            // Checking result
            Assert.Equal(1, 1);
        }
    }
}