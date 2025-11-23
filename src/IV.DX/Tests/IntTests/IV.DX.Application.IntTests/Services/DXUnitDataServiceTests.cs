using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXUnitDataServiceTests : IntTestController
    {
        IDXUnitDataService _service;
        IDXUnitGenericRepository _genericRepo;
        IDXStructureRepository _dataStructureRepo;

        public DXUnitDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._genericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._dataStructureRepo = base.ServiceProvider.GetRequiredService<IDXStructureRepository>();
        }

        [Fact]
        public async Task Insert_UsingDXUnitAsJObject_Ok()
        {
            // Init
            var content = File.ReadAllText("Services/JSON/DXElementDefinitionUnit.json");
            var jObject = JObject.Parse(content);

            base._finalizationAction = new Action(() =>
            {
                this._service.DeleteAsync(jObject).Wait();
            });

            // Action
            var createdItem = await this._service.InsertAsync(jObject);

            // Assert
            Assert.NotNull(createdItem);
        }

        [Fact]
        public async Task GetItemsAsync_UsingFilterForNonExistingItems_EmptyEnumerable()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            string filter = "DXObjectDefinitionMainElement.Kind = 999888777";

            // Action
            var items = await this._service.GetItemsAsync(typeName, filter);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Fact]
        public async Task GetItemsAsync_UsingFilterForExistingItems_Enumerable()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            string filter = "DXObjectDefinitionMainElement.Kind = 1";

            // Action
            var items = await this._service.GetItemsAsync(typeName, filter);

            // Assert
            Assert.NotNull(items);
            Assert.NotEmpty(items);
        }

        [Fact]
        public async Task GetItemAsync_UsingIDForExistingItems_Ok()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            var id = new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2");

            // Action
            var item = await this._service.GetItemAsync(typeName, id);

            // Assert
            Assert.NotNull(item);
        }


        [Fact]
        public async Task GetItemAsync_UsingIDForExistingItems_Ok1()
        {

            IDXStructureCache cache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();

            await cache.RefreshAsync();

            // Init
            string typeName = "DXElementDefinitionUnit";
            var id = new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2");

            // Action
            var item = await this._service.GetItemAsync(typeName, id);

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public async Task InsertDXUnit_UsingLargeAmountOfMultiItems_Ok()
        {
            // Init
            var id = new Guid("b7ef84bd-da1a-4855-8de8-8c148d2871a0");
            var itemAmount = 10000;
            var textLength = 10000;

            var text = Enumerable.Range(0, itemAmount).Select(x => GetRandomString(textLength)).ToHashSet();
            var item = TBookUnitFactory.GetItemWithText(id, $"Name{id}", text);

            base._finalizationAction = () =>
            {
                this._genericRepo.Delete(item);
            };

            // Action
            var result = await EstimatePerformanceAsync(async () =>
            {
                return await this._service.InsertAsync(item);
            }, $"InsertAsync unit with {itemAmount} multi items. Each multi item has text with {textLength} length");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(text.Count(), result.TBookChapterElement.Announced.Count(x => text.Contains(x.Text)));

            var existingItem = await EstimatePerformanceAsync(async () =>
            {
                return await this._service.GetItemAsync<TBookUnit>(id);
            }, $"GetItemAsync unit with {text.Count()} multi items");

            Assert.NotNull(existingItem);
            Assert.Equal(text.Count(), existingItem.TBookChapterElement.Announced.Count(x => text.Contains(x.Text)));
        }

        [Fact]
        public async Task InsertDXUnit_UsingEnumColumnsWithFullMode_Ok()
        {
            // Init
            var id = new Guid("b028075c-f460-42c0-a456-36e50ba645a8");

            var objectKindEnum = new DXColumnDefinitionElement()
            {
                ID = Guid.NewGuid(),
                DXUnitID = id,
                ColumnType = DXColumnTypeEnum.Int,
                AllowNull = true,
                Name = "ObjectKind",
                EnumType = new Guid("3c9d2fa6-99e3-472b-b493-3e4790597f98"),
                EnumKey = new Guid("15d97f21-fd2d-4019-8e0b-bd480fdc8798")
            };

            var relaionTypeEnum = new DXColumnDefinitionElement()
            {
                ID = Guid.NewGuid(),
                DXUnitID = id,
                ColumnType = DXColumnTypeEnum.Int,
                AllowNull = true,
                Name = "RelationType",
                EnumType = new Guid("3fdb5f35-33f6-4356-8f65-f92da429191c"),
                EnumKey = new Guid("0ce6d41d-1906-4d24-adc3-31f0922fd7cd")
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                ID = id,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id,
                    Name = "DXUnitWithEnum",
                    Kind = DXObjectKindEnum.Test
                },
                DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
                {
                    Mode = MultiElementsMode.Full,
                    Announced = new HashSet<DXColumnDefinitionElement>()
                    {
                      objectKindEnum
                    }
                }
            };

            base._finalizationAction = () =>
            {
                this._service.DeleteAsync(dxUnit).Wait();
            };

            // Action
            await this._service.InsertAsync(dxUnit);

            // Assert
            var createdDXUnit = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id);

            Assert.NotNull(createdDXUnit);

            Assert.NotEmpty(createdDXUnit.DXColumnDefinitionElement.Announced);

            var createdEnums = createdDXUnit.DXColumnDefinitionElement.Announced.SingleOrDefault(x => objectKindEnum.ID == x.ID);
            Assert.NotNull(createdEnums);

            var createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXObjectKindEnum", "ObjectKind", "DXUnitWithEnum", "DXUnitWithEnum");

            Assert.NotNull(createdRelation);
            Assert.Equal("Key", createdRelation.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Null(createdRelation.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXRelationTypeEnum", "RelationType", "DXUnitWithEnum", "DXUnitWithEnum");

            Assert.Null(createdRelation);

            var createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnum", "DXObjectKindEnum", "ObjectKind");

            Assert.NotNull(createdRelationInverted);
            Assert.Null(createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Equal("Key", createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnum", "DXRelationTypeEnum", "RelationType");

            Assert.Null(createdRelationInverted);

            var instancesWithObjectKind = await this._service.GetItemsAsync<DXUnitWithKindEnum>();

            Assert.Empty(instancesWithObjectKind);

            // Action
            dxUnit.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>()
            {
                Mode = MultiElementsMode.Full,
                Announced = new HashSet<DXColumnDefinitionElement>()
                {
                    relaionTypeEnum
                }
            };

            await this._service.InsertAsync(dxUnit);

            // Assert
            createdDXUnit = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id);

            Assert.NotNull(createdDXUnit);

            Assert.NotEmpty(createdDXUnit.DXColumnDefinitionElement.Announced);

            createdEnums = createdDXUnit.DXColumnDefinitionElement.Announced.SingleOrDefault(x => relaionTypeEnum.ID == x.ID);
            Assert.NotNull(createdEnums);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXRelationTypeEnum", "RelationType", "DXUnitWithEnum", "DXUnitWithEnum");

            Assert.NotNull(createdRelation);
            Assert.Equal("Key", createdRelation.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Null(createdRelation.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXObjectKindEnum", "ObjectKind", "DXUnitWithEnum", "DXUnitWithEnum");

            Assert.Null(createdRelation);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnum", "DXRelationTypeEnum", "RelationType");

            Assert.NotNull(createdRelationInverted);
            Assert.Null(createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Equal("Key", createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnum", "DXObjectKindEnum", "ObjectKind");

            Assert.Null(createdRelationInverted);

            
            var instanceWithRelationType = await this._service.GetItemsAsync<DXUnitWithRelationTypeEnum>();

            Assert.Empty(instanceWithRelationType);
        }

        [DXUnit("DXUnitWithEnum")]
        public class DXUnitWithKindEnum : DXUnit
        {
            [DXColumn("ObjectKind")]
            DXObjectKindEnum ObjectKind { get; set; }
        }

        [DXUnit("DXUnitWithEnum")]
        public class DXUnitWithRelationTypeEnum : DXUnit
        {
            [DXColumn("RelationType")]
            DXRelationTypeEnum RelationType { get; set; }
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