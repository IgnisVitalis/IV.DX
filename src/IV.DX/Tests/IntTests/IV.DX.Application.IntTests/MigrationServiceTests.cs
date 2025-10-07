using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Services.IntTests
{
    public class MigrationServiceTests : IntTestController
    {
        public MigrationServiceTests(ITestOutputHelper output)
            : base(output)
        {
            this._migrationService = this.ServiceProvider.GetService<IMigrationService>();
        }

        //[Fact]
        public void Init()
        {
            //return;
            // Init
            //base._finalizationAction = new Action(() =>
            //{
            //    this._dataService.Delete(model);
            //});

            // Action    
            //this._coreRepo.DropDataBase();
            //this._coreRepo.CreateDataBase();

            //this._migrationService.LoadStructure("MigrationScripts/Test.json", base._coreRepo);

            //var json = File.ReadAllText("Assets/DPObjectDescObject.dat");

            //var handlerType = EntityHandlerProvider.GetHandlerType("DPObjectDescObject");

            //var obj = JsonConvert.DeserializeObject(json, handlerType);


            // Checking result
        }

        //[Fact]
        public void Test()
        {
            DPBlockDescObject block = new DPBlockDescObject()
            {
                ID = new Guid("2176ef3f-fa54-466a-af9f-eccebb649628"),
                DPObjectDescGenBlock = new DPObjectDescGenBlock()
                {
                    ID = new Guid("473a8e0f-9970-4ea2-af3b-14eb115824f1"),
                    DisplayValue = "DisplayValue",
                    Kind = DPObjectKindEnum.Custom,
                    Name = "Name"
                }
            };

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(block);
            });


            this._dataService.Insert(block);

            //var items = this._genericRepo.GetItems<DPEntityDescObjectDerived>();

            //var items2 = this._genericRepo.GetItems<DPEntityDescObject>();

            //var items3 = this._genericRepo.GetItems<DPBlockDescObject>();

        }

        //[Fact]
        public void Finilization()
        {
            //var item = this._dataService.GetItem<DPObjectDescObject>(new Guid("05fdf970-e682-436d-8a09-94fc60d6b650"));
            var item2 = this._dataService.GetItem<TDeviceObject>(new Guid("24d8f6ff-b411-4acc-8a35-5e958ce7f070"));

            var all1 = this._genericRepo.GetItems<DPMigrationScriptsObject>();
            var all2 = this._dataService.GetItems<DPMigrationScriptsObject>();
            var all3 = this._genericRepo.GetItems<DPMigrationScriptsObject>();
            var all4 = this._dataService.GetItems<DPMigrationScriptsObject>();
        }
    }
}
