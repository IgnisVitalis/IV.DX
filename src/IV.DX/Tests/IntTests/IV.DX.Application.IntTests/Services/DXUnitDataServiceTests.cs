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
        public async Task GetItemAsync_UsingDXElementDefinitionUnitAndIDForExistingItem_Ok()
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
        public async Task GetItemAsync_UsingDXUnitDefinitionUnitTypeAndIDForExistingItem_Ok()
        {
            // Init
            string typeName = "DXUnitDefinitionUnit";
            var id = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c");

            // Action
            var item = await this._service.GetItemAsync(typeName, id);

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public async Task InsertDXUnitItemAsync_UsingDXUnitWithRelatedDXUnits_Ok()
        {
            // Init           
            var id1 = new Guid("acc683cf-33e1-473e-b218-565697a0378e");
            var id2 = new Guid("69a8eb15-125e-400e-8f0c-0996fca7d076");

            var dxUnit1 = new DXUnitDefinitionUnit()
            {
                ID = id1,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id1,
                    Name = "dxUnit1",
                    Kind = DXObjectKindEnum.Test
                }
            };

            var dxUnit2 = new DXUnitDefinitionUnit()
            {
                ID = id2,
                DXObjectDefinitionMainElement = new DXObjectDefinitionMainElement()
                {
                    ID = Guid.NewGuid(),
                    DXUnitID = id2,
                    Name = "dxUnit2",
                    Kind = DXObjectKindEnum.Test
                },
                DXUnitRelationElement = new DXMultiElementsContainer<DXUnitRelationElement>()
                {
                    Announced = new HashSet<DXUnitRelationElement>()
                    {
                        new DXUnitRelationElement()
                        {
                            ID = Guid.NewGuid(),
                            DXUnitID = id2,
                            OwnRelationName = "dxUnit2RelationName",
                            RelationType = DXRelationTypeEnum.OneToMany,
                            TargetRelationName = "dxUnit1RelationName",
                            TargetUnit = id1
                        }
                    }
                }
            };

            // Action
            var item1 = await this._service.InsertAsync(dxUnit1);
            var item2 = await this._service.InsertAsync(dxUnit2);

            // Assert
            var existingItem1 = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id1);
            var existingItem2 = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id2);

            Assert.Single(existingItem1.DXUnitRelationElement.Announced);

            Assert.Single(existingItem2.DXUnitRelationElement.Announced);
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

            var objectKindEnum = new DXObjectEnumElement()
            {
                ID = Guid.NewGuid(),
                DXUnitID = id,
                AllowNull = true,
                Name = "ObjectKind",
                EnumType = new Guid("3c9d2fa6-99e3-472b-b493-3e4790597f98"),
                EnumKey = new Guid("15d97f21-fd2d-4019-8e0b-bd480fdc8798")
            };

            var relaionTypeEnum = new DXObjectEnumElement()
            {
                ID = Guid.NewGuid(),
                DXUnitID = id,
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
                DXObjectEnumElement = new DXMultiElementsContainer<DXObjectEnumElement>()
                {
                    Mode = MultiElementsMode.Full,
                    Announced = new HashSet<DXObjectEnumElement>()
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

            Assert.NotEmpty(createdDXUnit.DXObjectEnumElement.Announced);

            var createdEnums = createdDXUnit.DXObjectEnumElement.Announced.SingleOrDefault(x => objectKindEnum.ID == x.ID);
            Assert.NotNull(createdEnums);

            var createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXObjectKindEnum", "ObjectKind", "DXUnitWithEnum", "DXUnitWithEnumObjectKind");

            Assert.NotNull(createdRelation);
            Assert.Equal("Key", createdRelation.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Null(createdRelation.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXRelationTypeEnum", "RelationType", "DXUnitWithEnum", "DXUnitWithEnumRelationType");

            Assert.Null(createdRelation);

            var createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumObjectKind", "DXObjectKindEnum", "ObjectKind");

            Assert.NotNull(createdRelationInverted);
            Assert.Null(createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Equal("Key", createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumRelationType", "DXRelationTypeEnum", "RelationType");

            Assert.Null(createdRelationInverted);

            var instancesWithObjectKind = await this._service.GetItemsAsync<DXUnitWithKindEnum>();

            Assert.Empty(instancesWithObjectKind);

            // Action
            dxUnit.DXObjectEnumElement = new DXMultiElementsContainer<DXObjectEnumElement>()
            {
                Mode = MultiElementsMode.Full,
                Announced = new HashSet<DXObjectEnumElement>()
                {
                    relaionTypeEnum
                }
            };

            await this._service.InsertAsync(dxUnit);

            // Assert
            createdDXUnit = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id);

            Assert.NotNull(createdDXUnit);

            Assert.NotEmpty(createdDXUnit.DXObjectEnumElement.Announced);

            createdEnums = createdDXUnit.DXObjectEnumElement.Announced.SingleOrDefault(x => relaionTypeEnum.ID == x.ID);
            Assert.NotNull(createdEnums);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXRelationTypeEnum", "RelationType", "DXUnitWithEnum", "DXUnitWithEnumRelationType");

            Assert.NotNull(createdRelation);
            Assert.Equal("Key", createdRelation.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Null(createdRelation.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXObjectKindEnum", "ObjectKind", "DXUnitWithEnum", "DXUnitWithEnumObjectKind");

            Assert.Null(createdRelation);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumRelationType", "DXRelationTypeEnum", "RelationType");

            Assert.NotNull(createdRelationInverted);
            Assert.Null(createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameLeft);
            Assert.Equal("Key", createdRelationInverted.DXRelationDefinitionMainElement.RelationColumnNameRight);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumObjectKind", "DXObjectKindEnum", "ObjectKind");

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