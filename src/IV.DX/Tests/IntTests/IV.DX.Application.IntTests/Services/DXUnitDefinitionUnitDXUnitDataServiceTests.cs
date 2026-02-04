using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{

    [Collection("DX:one-time")]
    public class DXUnitDefinitionUnitDXUnitDataServiceTests : IntTestController
    {
        IDXUnitDataService _service;
        IDXUnitCoreRepository _coreRepo;
        IDXStructureCache _dxStructureCache;

        public DXUnitDefinitionUnitDXUnitDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._coreRepo = base.ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            this._dxStructureCache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();
        }

        [Fact]
        public async Task InsertAsyncAndDeleteAsync_UsingSimpleDXUnit_DXUnitIsRemoved()
        {
            // Init
            var id = new Guid("9e52f604-6719-42a6-b29d-e8d8a73bc173");
            var dxUnitName = $"DXUnit{id}";
            var timeStamp = DateTime.UtcNow;

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = id,
                TimeStamp = timeStamp,
                Kind = DXObjectKindEnum.Custom,
                Name = dxUnitName,
                DisplayValue = "Name"
            };

            // Action
            var dxUnitCreated = await this._service.InsertAsync(dxUnit);

            // Assert
            var block = this._coreRepo.GetItemsRecord(dxUnitName);

            Assert.True(block.Data == null || block.Data.Items == null || block.Data.Items.Count == 0);

            await this._dxStructureCache.RefreshAsync();
            var existigDXUnitStructure = this._dxStructureCache.GetDXUnit(dxUnitName);

            Assert.NotNull(existigDXUnitStructure);

            // Action
            var result = await this._service.DeleteAsync(dxUnit);

            Assert.True(result);

            await this._dxStructureCache.RefreshAsync();
            existigDXUnitStructure = this._dxStructureCache.GetDXUnit(dxUnitName);

            Assert.Null(existigDXUnitStructure);
        }
    }
}

