using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DXEnumCoreRepository : IntTestController
    {
        IDXEnumCoreRepository _dxEnumGenericRepo;

        public DXEnumCoreRepository(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dxEnumGenericRepo = this.ServiceProvider.GetRequiredService<IDXEnumCoreRepository>();
        }

        [Theory]
        [InlineData("DXObjectKindEnum", 3)]
        [InlineData("DXColumnTypeEnum", 15)]
        [InlineData("DXElementInUnitTypeEnum", 4)]
        [InlineData("DXRelationTypeEnum", 8)]
        public void GetItems_UsingDifferentTypes_Ok(string enumTypeName, int expectedAmount)
        {
            // Action
            var block = this._dxEnumGenericRepo.GetItemsRecord(enumTypeName);

            // Assert
            var count = block.Data?.Items?.Count ?? 0;
            Assert.Equal(expectedAmount, count);
        }
    }
}

