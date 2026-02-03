using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
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
        IDXRawReader _dxRawReader;
        IDXStructureRepository _dataStructureRepo;
        ISQLQueryBuilder _sqlBuilder;

        public DXUnitDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._genericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._dataStructureRepo = base.ServiceProvider.GetRequiredService<IDXStructureRepository>();
            this._dxRawReader = base.ServiceProvider.GetRequiredService<IDXRawReader>();
            this._sqlBuilder = base.ServiceProvider.GetRequiredService<ISQLQueryBuilder>();
        }

        [Fact]
        public async Task Insert_UsingDXUnitWithBlob_Ok()
        {

        }

        [Fact]
        public async Task Insert_UsingDXUnitWithSelfRelation_Ok()
        {
            // Init            
            var definitionId = new Guid("cc2a1275-5a0f-468a-be92-b4715b94ab19");
            var relationId = new Guid("1676cad5-c5d6-4584-8d13-e0155fbd8b1b");
            var definitionTime = DateTime.Parse("2025-12-11T10:20:09.399068Z");
            var relationTime = DateTime.Parse("2025-12-11T10:20:16.0861678Z");

            var dxUnitDefinition = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = "DXUnitDefinitionUnit"
                },
                Data = new DXData<DXUnitRecord>
                {
                    Upsert = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            ID = definitionId,
                            TimeStamp = definitionTime,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject("DXNavigationItemUnit") },
                                { "DisplayValue", JValue.CreateNull() },
                                { "Kind", JToken.FromObject(1) }
                            },
                            DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["DXUnitToUnitRelationElement"] = new DXDataBlock<DXElementRecord>
                                {
                                    Meta = new DXMeta
                                    {
                                        Kind = "DXElement",
                                        Type = "DXUnitToUnitRelationElement",
                                        Op = "Patch",
                                        IsMulti = true
                                    },
                                    Data = new DXData<DXElementRecord>
                                    {
                                        Upsert = new List<DXElementRecord>
                                        {
                                            new DXElementRecord
                                            {
                                                ID = relationId,
                                                DXUnitID = definitionId,
                                                TimeStamp = relationTime,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "OwnRelationName", JToken.FromObject("Parent") },
                                                    { "TargetRelationName", JToken.FromObject("Children") },
                                                    { "RelationType", JToken.FromObject(5) },
                                                    { "TargetDXUnit", JToken.FromObject(definitionId) }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Action
            var createdItem = await this._service.InsertAsync(JObject.FromObject(dxUnitDefinition));

            // Assert
            var id = definitionId;

            var existingItem = await this._service.GetItemAsync("DXNavigationItemUnit", id);

            Assert.Null(existingItem);

            var columns = new Dictionary<string, string>()
            {
                {"ID","ID" },
                {"TimeStamp", "TimeStamp"},
                {"ChildrenID", "U2U(Children).ID"},
                {"ParentID", "U2U(Parent).ID"}
            };

            var dxFilter = "U2U(Children).ID = '075980bc-9728-47cf-aab9-077f391ded48' AND U2U(Parent).ID = '88bbeb1b-627f-4eaf-be6a-4e52f13cab5d'";

            var result = this._dxRawReader.Get("DXNavigationItemUnit", columns, dxFilter);

            Assert.NotNull(result);
            Assert.Empty(result.Data?.Upsert ?? new List<DXUnitRecord>());

            var sql = this._sqlBuilder.BuildSQLExpression("DXNavigationItemUnit", columns, dxFilter);

            var expectedSqlQuery = "SELECT\n\"T_14_0\".\"ID\" AS \"ID\",\n\"T_14_0\".\"TimeStamp\" AS \"TimeStamp\",\n\"T_14_1\".\"ID\" AS \"ChildrenID\",\n\"T_14_2\".\"ID\" AS \"ParentID\"\nFROM\n\"DXNavigationItemUnit\" AS \"T_14_0\"\nLEFT JOIN \"DXNavigationItemUnit\" AS \"T_14_1\" ON \"T_14_1\".\"Parent\" = \"T_14_0\".\"ID\"\nLEFT JOIN \"DXNavigationItemUnit\" AS \"T_14_2\" ON \"T_14_2\".\"ID\" = \"T_14_0\".\"Parent\"\nWHERE\n\"T_14_1\".\"ID\" = '075980bc-9728-47cf-aab9-077f391ded48'  AND  \"T_14_2\".\"ID\" = '88bbeb1b-627f-4eaf-be6a-4e52f13cab5d'";

            Assert.Equal(expectedSqlQuery, sql);
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
            string filter = "Kind = 999888777";

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
            string filter = "Kind = 1";

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
            var id = new Guid("ce754889-4efb-4281-ad1f-14d710b30007");

            // Action
            var item = await this._service.GetItemAsync(typeName, id);

            // Assert
            var block = item.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = block?.Data?.Upsert?.SingleOrDefault();

            Assert.NotNull(record);
            Assert.Equal(id, record!.ID);
            Assert.Equal("DXColumnDefinitionElement", record.Fields?["Name"]?.ToObject<string>());
            Assert.Equal("Name", record.Fields?["DisplayValue"]?.ToObject<string>());
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
            var id3 = new Guid("f6c170e8-c2f4-4b3f-883c-1bdf24bddf29");

            var dxUnit1 = new DXUnitDefinitionUnit()
            {
                ID = id1,
                Name = "dxUnit1",
                DisplayValue = "Name",
                Kind = DXObjectKindEnum.Test
            };

            var dxUnit2 = new DXUnitDefinitionUnit()
            {
                ID = id2,
                Name = "dxUnit2",
                DisplayValue = "Name",
                Kind = DXObjectKindEnum.Test
            };

            var dxUnit3 = new DXUnitDefinitionUnit()
            {
                ID = id3,
                Name = "dxUnit3",
                DisplayValue = "Name",
                Kind = DXObjectKindEnum.Test
            };

            var dxUnitRelation1 = new DXUnitToUnitRelationElement()
            {
                ID = Guid.NewGuid(),
                DXUnitID = id2,
                OwnRelationName = "dxUnit2RelationName",
                RelationType = DXRelationTypeEnum.OneToMany,
                TargetRelationName = "dxUnit1RelationName",
                TargetDXUnit = id1
            };

            var dxUnitRelation2 = new DXUnitToUnitRelationElement()
            {
                ID = Guid.NewGuid(),
                DXUnitID = id2,
                OwnRelationName = "dxUnit2RelationName",
                RelationType = DXRelationTypeEnum.ManyToMany,
                TargetRelationName = "dxUnit3RelationName",
                TargetDXUnit = id3
            };

            dxUnit2.DXUnitToUnitRelationElement = new DXMultiElementsContainer<DXUnitToUnitRelationElement>()
            {
                Announced = new HashSet<DXUnitToUnitRelationElement>()
                {
                    dxUnitRelation1
                }
            };

            this._finalizationAction = () =>
            {
                this._service.DeleteAsync(dxUnit1).Wait();
                this._service.DeleteAsync(dxUnit2).Wait();
                this._service.DeleteAsync(dxUnit3).Wait();
            };

            // Action
            var item1 = await this._service.InsertAsync(dxUnit1);
            var item2 = await this._service.InsertAsync(dxUnit2);
            var item3 = await this._service.InsertAsync(dxUnit3);

            // Assert
            var existingItem1 = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id1);
            var existingItem2 = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id2);

            Assert.Single(existingItem1.DXUnitToUnitRelationElement.Announced);

            Assert.Single(existingItem2.DXUnitToUnitRelationElement.Announced);

            var dxUnitRelation1Existing = existingItem1.DXUnitToUnitRelationElement.Announced.Single();
            var dxUnitRelation2Existing = existingItem2.DXUnitToUnitRelationElement.Announced.Single();

            Assert.Equal(dxUnitRelation1Existing.TargetDXUnit, dxUnitRelation2Existing.DXUnitID);
            Assert.Equal(dxUnitRelation1Existing.DXUnitID, dxUnitRelation2Existing.TargetDXUnit);
            Assert.Equal(dxUnitRelation1Existing.OwnRelationName, dxUnitRelation2Existing.TargetRelationName);
            Assert.Equal(dxUnitRelation1Existing.TargetRelationName, dxUnitRelation2Existing.OwnRelationName);
            Assert.Equal(dxUnitRelation1Existing.RelationType, DXRelationTypeEnumHelper.GetInvertedRelationType(dxUnitRelation2Existing.RelationType));

            var relationDefinition1 = this._dataStructureRepo.GetDXRelationDefinition(
                dxUnit1.Name,
                dxUnitRelation1Existing.OwnRelationName,
                dxUnit2.Name,
                dxUnitRelation1Existing.TargetRelationName);

            Assert.NotNull(relationDefinition1);
            Assert.Equal(dxUnitRelation1Existing.RelationType, relationDefinition1.RelationType);

            var relationDefinition2 = this._dataStructureRepo.GetDXRelationDefinition(
                dxUnit2.Name,
                dxUnitRelation2Existing.OwnRelationName,
                dxUnit1.Name,
                dxUnitRelation2Existing.TargetRelationName);

            Assert.NotNull(relationDefinition2);
            Assert.Equal(dxUnitRelation2Existing.RelationType, relationDefinition2.RelationType);

            // Action
            dxUnit2.DXUnitToUnitRelationElement = new DXMultiElementsContainer<DXUnitToUnitRelationElement>()
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUnitToUnitRelationElement>()
                {
                    dxUnitRelation2
                },
                Deleted = new HashSet<DXUnitToUnitRelationElement>()
                {
                    dxUnitRelation1
                }
            };

            item2 = await this._service.UpdateAsync(dxUnit2);

            // Assert
            existingItem2 = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id2);
            var existingItem3 = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id3);

            Assert.Single(existingItem2.DXUnitToUnitRelationElement.Announced);

            Assert.Single(existingItem3.DXUnitToUnitRelationElement.Announced);

            dxUnitRelation2Existing = existingItem2.DXUnitToUnitRelationElement.Announced.Single();
            var dxUnitRelation3Existing = existingItem3.DXUnitToUnitRelationElement.Announced.Single();

            Assert.Equal(dxUnitRelation2Existing.TargetDXUnit, dxUnitRelation3Existing.DXUnitID);
            Assert.Equal(dxUnitRelation2Existing.DXUnitID, dxUnitRelation3Existing.TargetDXUnit);
            Assert.Equal(dxUnitRelation2Existing.OwnRelationName, dxUnitRelation3Existing.TargetRelationName);
            Assert.Equal(dxUnitRelation2Existing.TargetRelationName, dxUnitRelation3Existing.OwnRelationName);
            Assert.Equal(dxUnitRelation2Existing.RelationType, DXRelationTypeEnumHelper.GetInvertedRelationType(dxUnitRelation3Existing.RelationType));

            relationDefinition2 = this._dataStructureRepo.GetDXRelationDefinition(
                dxUnit2.Name,
                dxUnitRelation2Existing.OwnRelationName,
                dxUnit3.Name,
                dxUnitRelation2Existing.TargetRelationName);

            Assert.NotNull(relationDefinition2);
            Assert.Equal(dxUnitRelation2Existing.RelationType, relationDefinition2.RelationType);

            var relationDefinition3 = this._dataStructureRepo.GetDXRelationDefinition(
                dxUnit3.Name,
                dxUnitRelation3Existing.OwnRelationName,
                dxUnit2.Name,
                dxUnitRelation3Existing.TargetRelationName);

            Assert.NotNull(relationDefinition2);
            Assert.Equal(dxUnitRelation3Existing.RelationType, relationDefinition3.RelationType);

            // Finalization
            await this._service.DeleteAsync(dxUnit1);
            await this._service.DeleteAsync(dxUnit2);
            await this._service.DeleteAsync(dxUnit3);
        }

        [Fact]
        public async Task GetItemAsync_UsingIDForExistingItems_Ok()
        {
            IDXStructureCache cache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();

            await cache.RefreshAsync();

            // Init
            string typeName = "DXElementDefinitionUnit";
            var id = new Guid("ce754889-4efb-4281-ad1f-14d710b30007");

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
                Name = "DXUnitWithEnum",
                DisplayValue = "Name",
                Kind = DXObjectKindEnum.Test,

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
            Assert.Equal("Key", createdRelation.RelationColumnNameLeft);
            Assert.Equal("ObjectKind", createdRelation.RelationColumnNameRight);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXRelationTypeEnum", "RelationType", "DXUnitWithEnum", "DXUnitWithEnumRelationType");

            Assert.Null(createdRelation);

            var createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumObjectKind", "DXObjectKindEnum", "ObjectKind");

            Assert.NotNull(createdRelationInverted);
            Assert.Equal("ObjectKind", createdRelationInverted.RelationColumnNameLeft);
            Assert.Equal("Key", createdRelationInverted.RelationColumnNameRight);

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

            await this._service.UpdateAsync(dxUnit);

            // Assert
            createdDXUnit = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id);

            Assert.NotNull(createdDXUnit);

            Assert.NotEmpty(createdDXUnit.DXObjectEnumElement.Announced);

            createdEnums = createdDXUnit.DXObjectEnumElement.Announced.SingleOrDefault(x => relaionTypeEnum.ID == x.ID);
            Assert.NotNull(createdEnums);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXRelationTypeEnum", "RelationType", "DXUnitWithEnum", "DXUnitWithEnumRelationType");

            Assert.NotNull(createdRelation);
            Assert.Equal("Key", createdRelation.RelationColumnNameLeft);
            Assert.Equal("RelationType", createdRelation.RelationColumnNameRight);

            createdRelation = this._dataStructureRepo.GetDXRelationDefinition("DXObjectKindEnum", "ObjectKind", "DXUnitWithEnum", "DXUnitWithEnumObjectKind");

            Assert.Null(createdRelation);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumRelationType", "DXRelationTypeEnum", "RelationType");

            Assert.NotNull(createdRelationInverted);
            Assert.Equal("RelationType", createdRelationInverted.RelationColumnNameLeft);
            Assert.Equal("Key", createdRelationInverted.RelationColumnNameRight);

            createdRelationInverted = this._dataStructureRepo.GetDXRelationDefinition("DXUnitWithEnum", "DXUnitWithEnumObjectKind", "DXObjectKindEnum", "ObjectKind");

            Assert.Null(createdRelationInverted);

            var instanceWithRelationType = await this._service.GetItemsAsync<DXUnitWithRelationTypeEnum>();

            Assert.Empty(instanceWithRelationType);
        }

        [Fact]
        public async Task InsertOrUpdate_UsingExistingCoreDXUnitWithTargetModeForMultiElements_Ok()
        {
            // Init         
            var unitId = new Guid("00b29615-f32e-457e-81c6-606a0b4fd4f7");
            var unitTime = DateTime.Parse("2026-01-25T16:04:15.604149Z");

            var dxUnitBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = "DXElementDefinitionUnit"
                },
                Data = new DXData<DXUnitRecord>
                {
                    Upsert = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            ID = unitId,
                            TimeStamp = unitTime,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject("DXUnitToUnitRelationElement") },
                                { "DisplayValue", JToken.FromObject("OwnRelationName") },
                                { "Kind", JToken.FromObject(1) }
                            },
                            DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["DXColumnDefinitionElement"] = BuildEmptyMultiElementBlock("DXColumnDefinitionElement"),
                                ["DXUniqueColumnsElement"] = BuildEmptyMultiElementBlock("DXUniqueColumnsElement"),
                                ["DXObjectEnumElement"] = BuildEmptyMultiElementBlock("DXObjectEnumElement")
                            }
                        }
                    }
                }
            };

            // Action
            var existingDXUnit = await this._service.InsertOrUpdateAsync(JObject.FromObject(dxUnitBlock));

            // Assert
            Assert.NotNull(existingDXUnit);
        }

        private static DXDataBlock<DXElementRecord> BuildEmptyMultiElementBlock(string elementType)
        {
            return new DXDataBlock<DXElementRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXElement",
                    Type = elementType,
                    Op = "Patch",
                    IsMulti = true
                },
                Data = new DXData<DXElementRecord>
                {
                    Upsert = new List<DXElementRecord>(),
                    Delete = new List<DXDeleteRef>()
                }
            };
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
