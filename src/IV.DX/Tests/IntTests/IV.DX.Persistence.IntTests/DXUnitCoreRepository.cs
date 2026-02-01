using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DXUnitCoreRepository : IntTestController
    {
        IDXUnitCoreRepository _dxUnitCoreRepo;

        public DXUnitCoreRepository(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dxUnitCoreRepo = this.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
        }

        [Fact]
        public void GetItem_UsingTypeNameAndID_WholeDXRecord()
        {
            // Init
            var id = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c");

            // Action
            var dxUnitDefinition = this._dxUnitCoreRepo.GetItemRecord("DXUnitDefinitionUnit", id);

            // Assert
            Assert.NotNull(dxUnitDefinition);
        }
    }
}
