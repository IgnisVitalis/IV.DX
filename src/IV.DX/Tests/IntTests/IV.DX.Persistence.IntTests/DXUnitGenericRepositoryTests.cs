using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ObjectFactory = IV.DX.Shared.IntTests.Factories.ObjectFactory;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DXUnitGenericRepositoryTests : IntTestController
    {
        IDXUnitGenericRepository _genericRepo;

        public DXUnitGenericRepositoryTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._genericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        private void CRUD_UsingEnity_DXUnitIsProcessedCorrectly_Action()
        {
            // Init
            var objectId = new Guid("8C29571C-3784-4D11-AD3E-F1D055023FD6");

            var item = ObjectFactory.GetItem(objectId, "SomeTestName");

            base._finalizationAction = new Action(() =>
            {
                this._genericRepo.Delete(item);
            });

            // Action Insert
            this._genericRepo.Insert(item);

            // Checking result
            var result = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectId);

            Assert.NotNull(result);
            Assert.Equal(objectId, result.ID);
            Assert.NotNull(result.DXObjectDefinitionMainElement);
            Assert.True(result.DXObjectDefinitionMainElement.ID != default(Guid));
            Assert.Equal(objectId, result.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal("SomeTestName", result.DXObjectDefinitionMainElement.Name);
            Assert.True(result is DXUnitDefinitionUnit);

            // Action Update
            item.DXObjectDefinitionMainElement.Name = "UpdatedSomeTestName";
            this._genericRepo.Update(item);

            // Checking result
            result = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectId);

            Assert.NotNull(result);
            Assert.Equal(objectId, result.ID);
            Assert.NotNull(result.DXObjectDefinitionMainElement);
            Assert.True(result.DXObjectDefinitionMainElement.ID != default(Guid));
            Assert.Equal(objectId, result.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal("UpdatedSomeTestName", result.DXObjectDefinitionMainElement.Name);
            Assert.True(result is DXUnitDefinitionUnit);

            // Action Delete
            this._genericRepo.Delete(item);

            // Checking result
            result = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectId);

            Assert.Null(result);
        }


        // TODO: this test should be update to check multifragments also. Also need to use another dxUnit because DXObjectDefinitionUnit can be used in another tests.
        [Fact]
        public void CRUD_UsingEnities_DXUnitsAreProcessedCorrectly()
        {
            // Init
            var objectId1 = new Guid("3DBA464B-542F-484B-A121-2D2FFEE9FEAC");

            DXObjectDefinitionUnit item1 = ObjectFactory.GetItem(objectId1, "SomeTestName1");

            var objectId2 = new Guid("789E3EFA-EB65-498D-A1FC-C2CCA046DC62");

            DXObjectDefinitionUnit item2 = ObjectFactory.GetItem(objectId2, "SomeTestName2");

            IEnumerable<DXObjectDefinitionUnit> items = new List<DXObjectDefinitionUnit>()
            {
                item1, item2
            };

            base._finalizationAction = new Action(() =>
            {
                foreach (var item in items)
                {
                    this._genericRepo.Delete(item);
                }
            });

            // Action Insert
            this._genericRepo.Insert(item1);
            this._genericRepo.Insert(item2);

            // Checking result
            var result = this._genericRepo.GetDXUnits<DXUnitDefinitionUnit>();

            var resultItem1 = result.SingleOrDefault(x => x.ID == objectId1);

            Assert.NotNull(resultItem1);
            Assert.Equal(objectId1, resultItem1.ID);
            Assert.NotNull(resultItem1.DXObjectDefinitionMainElement);
            Assert.True(resultItem1.DXObjectDefinitionMainElement.ID != default(Guid));
            Assert.Equal(objectId1, resultItem1.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal("SomeTestName1", resultItem1.DXObjectDefinitionMainElement.Name);
            Assert.True(resultItem1 is DXUnitDefinitionUnit);

            var resultItem2 = result.SingleOrDefault(x => x.ID == objectId2);

            Assert.NotNull(resultItem2);
            Assert.Equal(objectId2, resultItem2.ID);
            Assert.NotNull(resultItem2.DXObjectDefinitionMainElement);
            Assert.True(resultItem2.DXObjectDefinitionMainElement.ID != default(Guid));
            Assert.Equal(objectId2, resultItem2.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal("SomeTestName2", resultItem2.DXObjectDefinitionMainElement.Name);
            Assert.True(resultItem2 is DXUnitDefinitionUnit);

            // Action Update
            item1.DXObjectDefinitionMainElement.Name = "UpdatedSomeTestName1";
            this._genericRepo.Update(item1);

            item2.DXObjectDefinitionMainElement.Name = "UpdatedSomeTestName2";
            this._genericRepo.Update(item2);

            // Checking result
            result = this._genericRepo.GetDXUnits<DXUnitDefinitionUnit>();

            resultItem1 = result.SingleOrDefault(x => x.ID == objectId1);

            Assert.NotNull(resultItem1);
            Assert.Equal(objectId1, resultItem1.ID);
            Assert.NotNull(resultItem1.DXObjectDefinitionMainElement);
            Assert.True(resultItem1.DXObjectDefinitionMainElement.ID != default(Guid));
            Assert.Equal(objectId1, resultItem1.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal("UpdatedSomeTestName1", resultItem1.DXObjectDefinitionMainElement.Name);
            Assert.True(resultItem1 is DXUnitDefinitionUnit);

            resultItem2 = result.SingleOrDefault(x => x.ID == objectId2);

            Assert.NotNull(resultItem2);
            Assert.Equal(objectId2, resultItem2.ID);
            Assert.NotNull(resultItem2.DXObjectDefinitionMainElement);
            Assert.True(resultItem2.DXObjectDefinitionMainElement.ID != default(Guid));
            Assert.Equal(objectId2, resultItem2.DXObjectDefinitionMainElement.DXUnitID);
            Assert.Equal("UpdatedSomeTestName2", resultItem2.DXObjectDefinitionMainElement.Name);
            Assert.True(resultItem2 is DXUnitDefinitionUnit);

            // Action Delete
            this._genericRepo.Delete(item1);

            // Checking result
            resultItem1 = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectId1);

            Assert.Null(resultItem1);

            // Action Delete
            this._genericRepo.Delete(item2);

            // Checking result
            resultItem2 = this._genericRepo.GetDXUnit<DXUnitDefinitionUnit>(objectId2);

            Assert.Null(resultItem2);
        }

        [Fact]
        public async Task InsertDXUnit_UsingLargeAmountOfMultiItems_Ok()
        {
            // Init
            var id = new Guid("27bf2430-f8a5-4293-ac66-5d834ce244c9");
            var itemAmount = 10000;
            var textLength = 10000;

            var text = Enumerable.Range(0, itemAmount).Select(x => GetRandomString(textLength)).ToHashSet();
            var item = TBookUnitFactory.GetItemWithText(id, $"Name{id}", text);

            base._finalizationAction = () =>
            {
                this._genericRepo.Delete(item);
            };

            // Action
            await EstimatePerformanceAsync(async () =>
            {
                this._genericRepo.Insert(item);
            }, $"Insert unit with {itemAmount} multi items. Each multi item has text with {textLength} length");

            // Assert         
            var existingItem = await EstimatePerformanceAsync(async () =>
            {
                return this._genericRepo.GetDXUnit<TBookUnit>(id);
            }, $"GetItemAsync unit with {itemAmount} multi items");

            Assert.NotNull(existingItem);
            Assert.Equal(text.Count(), existingItem.TBookChapterElement.Announced.Count(x => text.Contains(x.Text)));
        }

        private const string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GetRandomString(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var result = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[sizeof(uint)];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    uint num = BitConverter.ToUInt32(buffer, 0);
                    result.Append(_chars[(int)(num % (uint)_chars.Length)]);
                }
            }

            return result.ToString();
        }
    }
}