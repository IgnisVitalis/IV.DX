using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DX.Contracts.Common.Converters;
using IV.DX.Contracts.Common.Models;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Repositories.IntTests
{
    public class ObjectRepositoryTests : IntTestController
    {
        public ObjectRepositoryTests(ITestOutputHelper output)
            : base(output)
        {

        }

        [Fact]
        public void CreateObject_WithDefaultValues_Success()
        {
            // Init
            string json = File.ReadAllText("Assets/JSON/Objects/DPObjectDescObject0000.json");

            DPObjectDescObject objDesc = ESQLObjectHelper.CreateInstance<DPEntityDescObject>(json);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objDesc);
            });

            // Action
            this._dataService.Insert(objDesc);

            // Checking
            var objDefinition = this._genericRepo.GetItem<DPEntityDescObject>(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"));

            Assert.NotNull(objDefinition);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.ID);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.DPObjectDescGenBlock.ObjectID);
            Assert.Equal(new Guid("19EAEF84-8E84-4B1B-BC5E-90A277BB67E5"), objDefinition.DPObjectDescGenBlock.ID);
            Assert.Equal("NewObject", objDefinition.DPObjectDescGenBlock.Name);
            Assert.True(objDefinition is DPEntityDescObject);
        }

        [Fact]
        public void UpdateObject_UsingNewValues_Success()
        {
            // Ini
            string json0 = File.ReadAllText("Assets/JSON/Objects/DPObjectDescObject0000.json");
            string json1 = File.ReadAllText("Assets/JSON/Objects/DPObjectDescObject0001.json");
            var objDesc0 = ESQLObjectHelper.CreateInstance<DPEntityDescObject>(json0);
            var objDesc1 = ESQLObjectHelper.CreateInstance<DPEntityDescObject>(json1);

            base._finalizationAction = new Action(() =>
            {
                this._dataService.Delete(objDesc1);
            });

            // Action
            this._dataService.Insert(objDesc0);
            this._dataService.Update(objDesc1);

            // Checking
            var objDefinition = this._genericRepo.GetItem<DPEntityDescObject>(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"));

            Assert.NotNull(objDefinition);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.ID);
            Assert.Equal(new Guid("0C632EA2-D6E0-424B-8E4E-CF2B52847D54"), objDefinition.DPObjectDescGenBlock.ObjectID);
            Assert.Equal(new Guid("19EAEF84-8E84-4B1B-BC5E-90A277BB67E5"), objDefinition.DPObjectDescGenBlock.ID);
            Assert.Equal("UpdatedObject", objDefinition.DPObjectDescGenBlock.Name);
            Assert.True(objDefinition is DPEntityDescObject);
        }
    }
}