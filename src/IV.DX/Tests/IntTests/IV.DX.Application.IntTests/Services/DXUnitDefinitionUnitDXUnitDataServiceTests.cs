using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{

    [Collection("DX:one-time")]
    public class DXUnitDefinitionUnitDXUnitDataServiceTests : IntTestController
    {
        IDXUnitDataService _service;
        IDXUnitGenericRepository _genericRepo;
        IDXUnitCoreRepository _coreRepo;
        IDXStructureCache _dxStructureCache;

        public DXUnitDefinitionUnitDXUnitDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._genericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
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
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    TimeStamp = timeStamp,
                    Kind = DXObjectKindEnum.Custom,
                    Name = dxUnitName
                }
            };

            // Action
            var dxUnitCreated = await this._service.InsertAsync(dxUnit);

            // Assert
            var items = this._coreRepo.GetItems(dxUnitName);

            Assert.NotNull(items);
            Assert.Empty(items);

            var existigDXUnitStructure = this._dxStructureCache.GetDXUnit(dxUnitName);

            Assert.NotNull(existigDXUnitStructure);

            // Action
            var result = await this._service.DeleteAsync(dxUnit);

            Assert.True(result);

            existigDXUnitStructure = this._dxStructureCache.GetDXUnit(dxUnitName);

            Assert.Null(existigDXUnitStructure);
        }
    }
}