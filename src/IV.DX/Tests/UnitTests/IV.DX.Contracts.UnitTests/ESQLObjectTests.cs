using IV.DX.Kernel.Converters;
using IV.DX.Shared.UnitTests.DXObjects.DXUnits;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class ESQLObjectTests
    {
        [Fact]
        public void ConvertToESQLModel_UsingDerivedEntity_CorrectESQLModel()
        {
            // Init
            DerivedUnit1 entity = new DerivedUnit1();

            // Action
            var esqlModel = entity.ConvertToESQLModel();

            // Checking result
            Assert.Equal(1, 1);
        }
    }
}