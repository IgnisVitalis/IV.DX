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
            var id = new Guid("018fa545-8876-7a5a-a72c-3fdaf537245d");

            // Action
            var dxUnitDefinition = this._dxUnitCoreRepo.GetItemRecord("DXUnitDefinitionUnit", id);

            // Assert
            Assert.NotNull(dxUnitDefinition);

            var item = Assert.Single(dxUnitDefinition.Data.Items);
            Assert.Equal(id, item.Id);
            Assert.NotNull(item.Fields);
            Assert.Equal("DXObjectDefinitionUnit", item.Fields["Name"].ToString());
            Assert.True(item.Fields.ContainsKey("DerivedDXUnitType"), "DerivedDXUnitType field must be present");
            Assert.True(Guid.TryParse(item.Fields["DerivedDXUnitType"].ToString(), out _), "DerivedDXUnitType must be a valid Guid");
        }

        [Fact]
        public void GetItem_UsingBaseTypeNameAndID_BaseDXRecord()
        {
            // Init
            var id = new Guid("018fa545-8876-7a5a-a72c-3fdaf537245d");

            // Action
            var dxUnitDefinition = this._dxUnitCoreRepo.GetItemRecord("DXObjectDefinitionUnit", id);

            // Assert
            Assert.NotNull(dxUnitDefinition);

            var item = Assert.Single(dxUnitDefinition.Data.Items);
            Assert.Equal(id, item.Id);
            Assert.NotNull(item.Fields);
            Assert.Equal("DXObjectDefinitionUnit", item.Fields["Name"].ToString());
            Assert.True(item.Fields.ContainsKey("DerivedDXUnitType"), "DerivedDXUnitType field must be present");
            Assert.True(Guid.TryParse(item.Fields["DerivedDXUnitType"].ToString(), out _), "DerivedDXUnitType must be a valid Guid");
        }

        [Fact]
        public void GetComputer_UsingID_WholeDXRecord()
        {
            // Init
            var id = new Guid("018fa54a-5306-7a28-99f7-599b35d6b299");

            // Action
            var dxUnitDefinition = this._dxUnitCoreRepo.GetItemRecord("TComputerUnit", id);

            // Assert
            Assert.NotNull(dxUnitDefinition);
        }
    }
}
