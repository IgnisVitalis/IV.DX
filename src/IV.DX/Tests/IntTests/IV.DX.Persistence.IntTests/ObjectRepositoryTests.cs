using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class ObjectRepositoryTests : IntTestController
    {
        IDXUnitGenericRepository _genericRepo;
        IDXUnitDataService _dataService;

        public ObjectRepositoryTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._genericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();
        }

        [Fact]
        public void CreateObject_WithDefaultValues_Success()
        {
            // Init
            string json = File.ReadAllText("Assets/JSON/DXUnitDefinitionUnit/DXObjectDefinitionUnit0000.json");

            var objDesc = DXUnitConverter.ToDXUnit<DXUnitDefinitionUnit>(json);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(objDesc).Wait();
            });

            // Action
            this._dataService.InsertAsync(objDesc).Wait();

            // Checking
            var objDefinition = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"));

            Assert.NotNull(objDefinition);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.ID);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal(new Guid("19EAEF84-8E84-4B1B-BC5E-90A277BB67E5"), objDefinition.DXObjectDefinitionMainElement.ID);
            Assert.Equal("NewObject", objDefinition.DXObjectDefinitionMainElement.Name);
            Assert.True(objDefinition is DXUnitDefinitionUnit);
        }

        [Fact]
        public void UpdateObject_UsingNewValues_Success()
        {
            // Ini
            string json0 = File.ReadAllText("Assets/JSON/DXUnitDefinitionUnit/DXObjectDefinitionUnit0000.json");
            string json1 = File.ReadAllText("Assets/JSON/DXUnitDefinitionUnit/DXObjectDefinitionUnit0001.json");
            var objDesc0 = DXUnitConverter.ToDXUnit<DXUnitDefinitionUnit>(json0);
            var objDesc1 = DXUnitConverter.ToDXUnit<DXUnitDefinitionUnit>(json1);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.DeleteAsync(objDesc1).Wait();
            });

            // Action
            this._dataService.InsertAsync(objDesc0).Wait();
            this._dataService.UpdateAsync(objDesc1).Wait();

            // Checking
            var objDefinition = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"));

            Assert.NotNull(objDefinition);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.ID);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal(new Guid("19EAEF84-8E84-4B1B-BC5E-90A277BB67E5"), objDefinition.DXObjectDefinitionMainElement.ID);
            Assert.Equal("UpdatedObject", objDefinition.DXObjectDefinitionMainElement.Name);
            Assert.True(objDefinition is DXUnitDefinitionUnit);
        }
    }
}