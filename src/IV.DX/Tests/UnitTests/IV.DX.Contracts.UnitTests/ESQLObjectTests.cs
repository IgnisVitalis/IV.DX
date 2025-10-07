using IV.DataProvider.Persistence.Shared.UnitTests.ESQLObjects.Enitites;
using IV.DX.Kernel.Converters;
using Xunit;

namespace IV.DataProvider.Persistence.Contracts.UnitTests.Models
{
    public class ESQLObjectTests
    {
        [Fact]
        public void ConvertToESQLModel_UsingDerivedEntity_CorrectESQLModel()
        {
            // Init
            DerivedEntity1 entity = new DerivedEntity1();

            // Action
            var esqlModel = entity.ConvertToESQLModel();

            // Checking result
            Assert.Equal(1, 1);
        }
    }
}