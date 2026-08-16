using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Npgsql;
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
        IDXUnitDataReader _reader;
        IDXUnitGenericRepository _genericRepo;
        IDXRawReader _dxRawReader;
        IDXStructureRepository _dataStructureRepo;
        ISQLQueryBuilder _sqlBuilder;
        IDXExecutionContextAccessor _executionContextAccessor;

        public DXUnitDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._reader = base.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
            this._genericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._dataStructureRepo = base.ServiceProvider.GetRequiredService<IDXStructureRepository>();
            this._dxRawReader = base.ServiceProvider.GetRequiredService<IDXRawReader>();
            this._sqlBuilder = base.ServiceProvider.GetRequiredService<ISQLQueryBuilder>();
            this._executionContextAccessor = base.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
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
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            Id = definitionId,
                            TimeStamp = definitionTime,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject("DXPNavigationItemUnit") },
                                { "DXTitleExpression", JValue.CreateNull() },
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
                                        Items = new List<DXElementRecord>
                                        {
                                            new DXElementRecord
                                            {
                                                Id = relationId,
                                                DXUnitId = definitionId,
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
            var existingItem = await this._reader.GetItemAsync("DXPNavigationItemUnit", id);

            Assert.Null(existingItem);

            var columns = new Dictionary<string, string>()
            {
                {"Id","Id" },
                {"TimeStamp", "TimeStamp"},
                {"ChildrenId", "U2U(Children).Id"},
                {"ParentId", "U2U(Parent).Id"}
            };

            var dxFilter = "U2U(Children).Id = '075980bc-9728-47cf-aab9-077f391ded48' AND U2U(Parent).Id = '88bbeb1b-627f-4eaf-be6a-4e52f13cab5d'";

            var result = this._dxRawReader.Get("DXPNavigationItemUnit", columns, dxFilter);

            Assert.NotNull(result);
            Assert.Empty(result.Data?.Items ?? new List<DXUnitRecord>());

            var sql = this._sqlBuilder.BuildSQLExpression("DXPNavigationItemUnit", columns, dxFilter);

            var match = System.Text.RegularExpressions.Regex.Match(sql, "\"T_(\\d+)_0\"");
            Assert.True(match.Success);
            var index = int.Parse(match.Groups[1].Value);

            Assert.Contains($"\"T_{index}_0\".\"Id\" AS \"Id\"", sql);
            Assert.Contains($"\"T_{index}_0\".\"TimeStamp\" AS \"TimeStamp\"", sql);
            Assert.Contains($"FROM\n\"DXPNavigationItemUnit\" AS \"T_{index}_0\"", sql);

            var childAliasMatch = System.Text.RegularExpressions.Regex.Match(
                sql,
                $"\"T_{index}_(\\d+)\"\\.\"Id\" AS \"ChildrenId\"");
            Assert.True(childAliasMatch.Success);
            var childrenAliasIndex = childAliasMatch.Groups[1].Value;

            var parentAliasMatch = System.Text.RegularExpressions.Regex.Match(
                sql,
                $"\"T_{index}_(\\d+)\"\\.\"Id\" AS \"ParentId\"");
            Assert.True(parentAliasMatch.Success);
            var parentAliasIndex = parentAliasMatch.Groups[1].Value;

            Assert.NotEqual(childrenAliasIndex, parentAliasIndex);

            Assert.Contains(
                $"LEFT JOIN \"DXPNavigationItemUnit\" AS \"T_{index}_{childrenAliasIndex}\" ON \"T_{index}_{childrenAliasIndex}\".\"Parent\" = \"T_{index}_0\".\"Id\"",
                sql);
            Assert.Contains(
                $"LEFT JOIN \"DXPNavigationItemUnit\" AS \"T_{index}_{parentAliasIndex}\" ON \"T_{index}_{parentAliasIndex}\".\"Id\" = \"T_{index}_0\".\"Parent\"",
                sql);

            Assert.Contains($"\"T_{index}_{childrenAliasIndex}\".\"Id\" = '075980bc-9728-47cf-aab9-077f391ded48'", sql);
            Assert.Contains($"\"T_{index}_{parentAliasIndex}\".\"Id\" = '88bbeb1b-627f-4eaf-be6a-4e52f13cab5d'", sql);
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
            var createdId = await this._service.InsertAsync(jObject);

            // Assert
            Assert.NotEqual(Guid.Empty, createdId);
        }

        [Fact]
        public async Task InsertAsync_WhenWriteAccessDenied_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "unit-service-test-user",
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Create, "DXRoleUnit")
            });

            var id = Guid.NewGuid();
            var dxUnit = new DXUnitDefinitionUnit
            {
                Id = id,
                Name = $"Denied_{id:N}",
                DXTitleExpression = "Name",
                Kind = DXObjectKindEnum.Test
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => this._service.InsertAsync(dxUnit));
        }

        [Fact]
        public async Task GetItemsAsync_WhenNonSystemHasExplicitReadGrantToCore_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "core-read-denied-user",
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "DXUnitDefinitionUnit")
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => this._reader.GetItemsAsync("DXUnitDefinitionUnit"));
        }

        [Fact]
        public async Task InsertAsync_WhenNonSystemHasExplicitWriteGrantToCore_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "core-write-denied-user",
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Create, "DXUnitDefinitionUnit")
            });

            var id = Guid.NewGuid();
            var dxUnit = new DXUnitDefinitionUnit
            {
                Id = id,
                Name = $"DeniedCore_{id:N}",
                DXTitleExpression = "Name",
                Kind = DXObjectKindEnum.Test
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => this._service.InsertAsync(dxUnit));
        }

        [Fact]
        public async Task GetItemsAsync_UsingFilterForNonExistingItems_ReturnsEmptyBlock()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            string filter = "Kind = 999888777";

            // Action
            var items = await this._reader.GetItemsAsync(typeName, filter);

            // Assert
            Assert.NotNull(items);
            var block = items.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.NotNull(block);
            Assert.Empty(block.Data.Items ?? []);
        }

        [Fact]
        public async Task GetItemsAsync_UsingFilterForExistingItems_ReturnsBlock()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            string filter = "Kind = 1";

            // Action
            var items = await this._reader.GetItemsAsync(typeName, filter);

            // Assert
            Assert.NotNull(items);
            var block = items.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.NotNull(block?.Data?.Items);
            Assert.NotEmpty(block.Data.Items);
        }

        [Fact]
        public async Task GetItemsAsync_UsingHierarchicalAccessWithoutGroupForNonCoreUnit_Ok()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "hierarchical-read-user",
                // Tenant ∩ membership, no group level.
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "TUserUnit")
                    .Intersect(DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "TUserUnit"))
            });

            var items = await this._reader.GetItemsAsync("TUserUnit");

            Assert.NotNull(items);
            Assert.NotEmpty(items);
        }

        [Fact]
        public async Task GetItemsAsync_UsingHierarchicalAccessForNonCoreUnitWhenMembershipDenied_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "hierarchical-read-user",
                // Membership grants a different type, so tenant ∩ membership ∩ group is empty.
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "TUserUnit")
                    .Intersect(DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "DXRoleUnit"))
                    .Intersect(DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "TUserUnit"))
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => this._reader.GetItemsAsync("TUserUnit"));
        }

        [Fact]
        public async Task GetItemAsync_UsingDXElementDefinitionUnitAndIDForExistingItem_Ok()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            var id = new Guid("018fa545-1efe-7b84-8698-30a019eac3f2");

            // Action
            var item = await this._reader.GetItemAsync(typeName, id);

            // Assert
            var block = item.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = block?.Data?.Items?.SingleOrDefault();

            Assert.NotNull(record);
            Assert.Equal(id, record!.Id);
            Assert.Equal("DXColumnDefinitionElement", record.Fields?["Name"]?.ToObject<string>());
            Assert.Equal("Name", record.Fields?["DXTitleExpression"]?.ToObject<string>());
        }

        [Fact]
        public async Task GetItemAsync_UsingDXUnitDefinitionUnitTypeAndIDForExistingItem_Ok()
        {
            // Init
            string typeName = "DXUnitDefinitionUnit";
            var id = new Guid("018fa545-8876-7a5a-a72c-3fdaf537245d");

            // Action
            var item = await this._reader.GetItemAsync(typeName, id);

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public async Task InsertDXUnitItemAsync_UsingDXUnitWithRelatedDXUnits_Ok()
        {
            // Init
            var suffix = Guid.NewGuid().ToString("N")[..8];

            var dxUnit1 = new DXUnitDefinitionUnit()
            {
                Name = $"dxUnit1_{suffix}",
                DXTitleExpression = "Name",
                Kind = DXObjectKindEnum.Test
            };

            var dxUnit2 = new DXUnitDefinitionUnit()
            {
                Name = $"dxUnit2_{suffix}",
                DXTitleExpression = "Name",
                Kind = DXObjectKindEnum.Test
            };

            var dxUnit3 = new DXUnitDefinitionUnit()
            {
                Name = $"dxUnit3_{suffix}",
                DXTitleExpression = "Name",
                Kind = DXObjectKindEnum.Test
            };

            this._finalizationAction = () =>
            {
                this._service.DeleteAsync(dxUnit1).Wait();
                this._service.DeleteAsync(dxUnit2).Wait();
                this._service.DeleteAsync(dxUnit3).Wait();
            };

            // Action — insert dxUnit1 first to obtain its generated ID for the relation
            var item1 = await this._service.InsertAsync(dxUnit1);

            var dxUnitRelation1 = new DXUnitToUnitRelationElement()
            {
                OwnRelationName = "dxUnit2RelationName",
                RelationType = DXRelationTypeEnum.OneToMany,
                TargetRelationName = "dxUnit1RelationName",
                TargetDXUnit = item1
            };

            var dxUnitRelation2 = new DXUnitToUnitRelationElement()
            {
                Id = Guid.NewGuid(),
                OwnRelationName = "dxUnit2RelationName",
                RelationType = DXRelationTypeEnum.ManyToMany,
                TargetRelationName = "dxUnit3RelationName"
            };

            dxUnit2.DXUnitToUnitRelationElement = new DXMultiElementsContainer<DXUnitToUnitRelationElement>()
            {
                Announced = new HashSet<DXUnitToUnitRelationElement>()
                {
                    dxUnitRelation1
                }
            };

            var item2 = await this._service.InsertAsync(dxUnit2);
            var item3 = await this._service.InsertAsync(dxUnit3);

            dxUnitRelation2.TargetDXUnit = item3;

            // Assert
            var existingItem1 = await this._reader.GetItemAsync<DXUnitDefinitionUnit>(item1);
            var existingItem2 = await this._reader.GetItemAsync<DXUnitDefinitionUnit>(item2);

            Assert.Single(existingItem1.DXUnitToUnitRelationElement.Announced);

            Assert.Single(existingItem2.DXUnitToUnitRelationElement.Announced);

            var dxUnitRelation1Existing = existingItem1.DXUnitToUnitRelationElement.Announced.Single();
            var dxUnitRelation2Existing = existingItem2.DXUnitToUnitRelationElement.Announced.Single();

            Assert.Equal(dxUnitRelation1Existing.TargetDXUnit, dxUnitRelation2Existing.DXUnitId);
            Assert.Equal(dxUnitRelation1Existing.DXUnitId, dxUnitRelation2Existing.TargetDXUnit);
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
            dxUnitRelation2.DXUnitId = item2;

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
            existingItem2 = await this._reader.GetItemAsync<DXUnitDefinitionUnit>(item2);
            var existingItem3 = await this._reader.GetItemAsync<DXUnitDefinitionUnit>(item3);

            Assert.Single(existingItem2.DXUnitToUnitRelationElement.Announced);

            Assert.Single(existingItem3.DXUnitToUnitRelationElement.Announced);

            dxUnitRelation2Existing = existingItem2.DXUnitToUnitRelationElement.Announced.Single();
            var dxUnitRelation3Existing = existingItem3.DXUnitToUnitRelationElement.Announced.Single();

            Assert.Equal(dxUnitRelation2Existing.TargetDXUnit, dxUnitRelation3Existing.DXUnitId);
            Assert.Equal(dxUnitRelation2Existing.DXUnitId, dxUnitRelation3Existing.TargetDXUnit);
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
            var id = new Guid("018fa545-1efe-7b84-8698-30a019eac3f2");

            // Action
            var item = await this._reader.GetItemAsync(typeName, id);

            // Assert
            Assert.NotNull(item);
        }

        [Fact]
        public async Task InsertAndUpdate_UsingHashedStringColumn_HashesAndAvoidsDoubleHash_Ok()
        {
            var now = DateTime.UtcNow;

            var unitName = $"DXHashedSecretUnit_{Guid.NewGuid():N}";

            var unitDefinitionBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = "DXUnitDefinitionUnit"
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject(unitName) },
                                { "DXTitleExpression", JToken.FromObject("Secret") },
                                { "Kind", JToken.FromObject(1) }
                            },
                            DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["DXColumnDefinitionElement"] = new DXDataBlock<DXElementRecord>
                                {
                                    Meta = new DXMeta
                                    {
                                        Kind = "DXElement",
                                        Type = "DXColumnDefinitionElement",
                                        Op = "Patch",
                                        IsMulti = true
                                    },
                                    Data = new DXData<DXElementRecord>
                                    {
                                        Items = new List<DXElementRecord>
                                        {
                                            new DXElementRecord
                                            {
                                                TimeStamp = now,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "Name", JToken.FromObject("Secret") },
                                                    { "Length", JToken.FromObject(255) },
                                                    { "Precision", JValue.CreateNull() },
                                                    { "Scale", JValue.CreateNull() },
                                                    { "AllowNull", JToken.FromObject(false) },
                                                    { "DefaultValue", JValue.CreateNull() },
                                                    { "ColumnType", JToken.FromObject((int)DXColumnTypeEnum.HashedString) }
                                                }
                                            }
                                        }
                                    }
                                },
                                ["DXUniqueColumnsElement"] = BuildEmptyMultiElementBlock("DXUniqueColumnsElement"),
                                ["DXObjectEnumElement"] = BuildEmptyMultiElementBlock("DXObjectEnumElement")
                            }
                        }
                    }
                }
            };

            await this._service.InsertOrUpdateAsync(unitDefinitionBlock);

            var plaintext = "P@ssw0rd";

            var insertBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = unitName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(plaintext) }
                            }
                        }
                    }
                }
            };

            var instanceId = await this._service.InsertAsync(insertBlock);

            var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Secret"] = "Secret"
            };

            var firstRead = this._dxRawReader.Get(unitName, columns, $"Id = '{instanceId}'");
            var firstSecret = firstRead.Data.Items.Single().Fields["Secret"]?.ToString();

            Assert.NotNull(firstSecret);
            Assert.NotEqual(plaintext, firstSecret);
            Assert.StartsWith("$pbkdf2-sha512$", firstSecret);

            var updateBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = unitName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            Id = instanceId,
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(firstSecret) }
                            }
                        }
                    }
                }
            };

            await this._service.UpdateAsync(updateBlock);

            var secondRead = this._dxRawReader.Get(unitName, columns, $"Id = '{instanceId}'");
            var secondSecret = secondRead.Data.Items.Single().Fields["Secret"]?.ToString();

            Assert.Equal(firstSecret, secondSecret);

            await this._service.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = unitName },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { Id = instanceId } }
                }
            });
        }

        [Fact]
        public async Task InsertAndUpdate_UsingEncryptedStringColumn_EncryptsAndAvoidsDoubleEncrypt_Ok()
        {
            var now = DateTime.UtcNow;

            var unitName = $"DXEncryptedSecretUnit_{Guid.NewGuid():N}";

            var unitDefinitionBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = "DXUnitDefinitionUnit"
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject(unitName) },
                                { "DXTitleExpression", JToken.FromObject("Secret") },
                                { "Kind", JToken.FromObject(1) }
                            },
                            DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["DXColumnDefinitionElement"] = new DXDataBlock<DXElementRecord>
                                {
                                    Meta = new DXMeta
                                    {
                                        Kind = "DXElement",
                                        Type = "DXColumnDefinitionElement",
                                        Op = "Patch",
                                        IsMulti = true
                                    },
                                    Data = new DXData<DXElementRecord>
                                    {
                                        Items = new List<DXElementRecord>
                                        {
                                            new DXElementRecord
                                            {
                                                TimeStamp = now,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "Name", JToken.FromObject("Secret") },
                                                    { "Length", JValue.CreateNull() },
                                                    { "Precision", JValue.CreateNull() },
                                                    { "Scale", JValue.CreateNull() },
                                                    { "AllowNull", JToken.FromObject(false) },
                                                    { "DefaultValue", JValue.CreateNull() },
                                                    { "ColumnType", JToken.FromObject((int)DXColumnTypeEnum.EncryptedString) }
                                                }
                                            }
                                        }
                                    }
                                },
                                ["DXUniqueColumnsElement"] = BuildEmptyMultiElementBlock("DXUniqueColumnsElement"),
                                ["DXObjectEnumElement"] = BuildEmptyMultiElementBlock("DXObjectEnumElement")
                            }
                        }
                    }
                }
            };

            await this._service.InsertOrUpdateAsync(unitDefinitionBlock);

            var plaintext = "super-secret";

            var insertBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = unitName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(plaintext) }
                            }
                        }
                    }
                }
            };

            var instanceId = await this._service.InsertAsync(insertBlock);

            var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Secret"] = "Secret"
            };

            var firstRead = this._dxRawReader.Get(unitName, columns, $"Id = '{instanceId}'");
            var firstSecret = firstRead.Data.Items.Single().Fields["Secret"]?.ToString();

            Assert.NotNull(firstSecret);
            Assert.Equal(plaintext, firstSecret);

            var dbOptions = base.ServiceProvider.GetRequiredService<IOptions<DXDatabaseOptions>>().Value;

            string storedCiphertext;
            using (var conn = new NpgsqlConnection(dbOptions.ConnectionString))
            {
                conn.Open();
                using var cmd = new NpgsqlCommand($"SELECT \"Secret\" FROM \"{unitName}\" WHERE \"Id\" = @id", conn);
                cmd.Parameters.AddWithValue("id", instanceId);
                storedCiphertext = cmd.ExecuteScalar() as string ?? string.Empty;
            }

            Assert.NotEqual(plaintext, storedCiphertext);
            Assert.StartsWith("$aesgcm$", storedCiphertext);

            var updateBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = unitName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            Id = instanceId,
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(firstSecret) }
                            }
                        }
                    }
                }
            };

            await this._service.UpdateAsync(updateBlock);

            string storedCiphertextAfter;
            using (var conn = new NpgsqlConnection(dbOptions.ConnectionString))
            {
                conn.Open();
                using var cmd = new NpgsqlCommand($"SELECT \"Secret\" FROM \"{unitName}\" WHERE \"Id\" = @id", conn);
                cmd.Parameters.AddWithValue("id", instanceId);
                storedCiphertextAfter = cmd.ExecuteScalar() as string ?? string.Empty;
            }

            Assert.Equal(storedCiphertext, storedCiphertextAfter);

            var secondRead = this._dxRawReader.Get(unitName, columns, $"Id = '{instanceId}'");
            var secondSecret = secondRead.Data.Items.Single().Fields["Secret"]?.ToString();

            Assert.Equal(plaintext, secondSecret);

            await this._service.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = unitName },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { Id = instanceId } }
                }
            });
        }

        [Fact]
        public async Task GetItemAsync_UsingDataReader_MasksSensitiveFieldsAndKeepsUnchangedOnUpdate_Ok()
        {
            IDXStructureCache cache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();
            await cache.RefreshAsync();

            var now = DateTime.UtcNow;

            var unitName = $"DXSensitiveUnit_{Guid.NewGuid():N}";

            var unitDefinitionBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = "DXUnitDefinitionUnit"
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject(unitName) },
                                { "DXTitleExpression", JToken.FromObject("Secret") },
                                { "Kind", JToken.FromObject(1) }
                            },
                            DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["DXColumnDefinitionElement"] = new DXDataBlock<DXElementRecord>
                                {
                                    Meta = new DXMeta
                                    {
                                        Kind = "DXElement",
                                        Type = "DXColumnDefinitionElement",
                                        Op = "Patch",
                                        IsMulti = true
                                    },
                                    Data = new DXData<DXElementRecord>
                                    {
                                        Items = new List<DXElementRecord>
                                        {
                                            new DXElementRecord
                                            {
                                                TimeStamp = now,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "Name", JToken.FromObject("Secret") },
                                                    { "Length", JValue.CreateNull() },
                                                    { "Precision", JValue.CreateNull() },
                                                    { "Scale", JValue.CreateNull() },
                                                    { "AllowNull", JToken.FromObject(false) },
                                                    { "DefaultValue", JValue.CreateNull() },
                                                    { "ColumnType", JToken.FromObject((int)DXColumnTypeEnum.EncryptedString) }
                                                }
                                            },
                                            new DXElementRecord
                                            {
                                                TimeStamp = now,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "Name", JToken.FromObject("PasswordHash") },
                                                    { "Length", JToken.FromObject(255) },
                                                    { "Precision", JValue.CreateNull() },
                                                    { "Scale", JValue.CreateNull() },
                                                    { "AllowNull", JToken.FromObject(false) },
                                                    { "DefaultValue", JValue.CreateNull() },
                                                    { "ColumnType", JToken.FromObject((int)DXColumnTypeEnum.HashedString) }
                                                }
                                            }
                                        }
                                    }
                                },
                                ["DXUniqueColumnsElement"] = BuildEmptyMultiElementBlock("DXUniqueColumnsElement"),
                                ["DXObjectEnumElement"] = BuildEmptyMultiElementBlock("DXObjectEnumElement")
                            }
                        }
                    }
                }
            };

            await this._service.InsertOrUpdateAsync(unitDefinitionBlock);
            await cache.RefreshAsync();

            var secretPlain = "super-secret";
            var passwordPlain = "P@ssw0rd";

            var insertBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = unitName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(secretPlain) },
                                { "PasswordHash", JToken.FromObject(passwordPlain) }
                            }
                        }
                    }
                }
            };

            var instanceId = await this._service.InsertAsync(insertBlock);

            var dbOptions = base.ServiceProvider.GetRequiredService<IOptions<DXDatabaseOptions>>().Value;

            (string Secret, string PasswordHash) ReadDb()
            {
                using var conn = new NpgsqlConnection(dbOptions.ConnectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand($"SELECT \"Secret\", \"PasswordHash\" FROM \"{unitName}\" WHERE \"Id\" = @id", conn);
                cmd.Parameters.AddWithValue("id", instanceId);
                using var r = cmd.ExecuteReader();
                Assert.True(r.Read());
                return (r.GetString(0), r.GetString(1));
            }

            var storedBefore = ReadDb();
            Assert.StartsWith("$aesgcm$", storedBefore.Secret);
            Assert.StartsWith("$pbkdf2-sha512$", storedBefore.PasswordHash);

            var masked = await this._reader.GetItemAsync(unitName, instanceId);
            var maskedBlock = masked.ToObject<DXDataBlock<DXUnitRecord>>();

            Assert.NotNull(maskedBlock);
            var maskedRec = maskedBlock.Data.Items.Single();
            Assert.Equal(string.Empty, maskedRec.Fields["Secret"]?.ToString());
            Assert.Equal(string.Empty, maskedRec.Fields["PasswordHash"]?.ToString());

            await this._service.UpdateAsync(masked);

            var storedAfter = ReadDb();
            Assert.Equal(storedBefore.Secret, storedAfter.Secret);
            Assert.Equal(storedBefore.PasswordHash, storedAfter.PasswordHash);

            await this._service.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = unitName },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { Id = instanceId } }
                }
            });
        }

        [Fact]
        public async Task UpdateDXUnit_UsingUnchangedValues_KeepsTimeStamp()
        {
            // Init
            var item = TBookUnitFactory.GetItem(Guid.Empty, "TimeStampBook");

            base._finalizationAction = () =>
            {
                this._genericRepo.Delete(item);
            };

            await this._service.InsertAsync(item);

            var stored = await this._reader.GetItemAsync<TBookUnit>(item.Id);
            Assert.NotNull(stored);

            // Action: write the record back exactly as it was read.
            await this._service.UpdateAsync(stored);

            // Assert: a write with nothing to say leaves the row, and its timestamp, alone. The
            // timestamp is the only version marker a caller has, so moving it on a no-op edit
            // invalidates every cache downstream for no reason.
            var reread = await this._reader.GetItemAsync<TBookUnit>(item.Id);

            Assert.NotNull(reread);
            Assert.Equal(stored.TimeStamp, reread.TimeStamp);
        }

        [Fact]
        public async Task UpdateDXUnit_UsingChangedValues_MovesTimeStamp()
        {
            // Init
            var item = TBookUnitFactory.GetItem(Guid.Empty, "TimeStampChangedBook");

            base._finalizationAction = () =>
            {
                this._genericRepo.Delete(item);
            };

            await this._service.InsertAsync(item);

            var stored = await this._reader.GetItemAsync<TBookUnit>(item.Id);
            Assert.NotNull(stored);

            // Action
            stored.TBookMainElement.Name = "TimeStampChangedBook - edited";
            await this._service.UpdateAsync(stored);

            // Assert
            var reread = await this._reader.GetItemAsync<TBookUnit>(item.Id);

            Assert.NotNull(reread);
            Assert.Equal("TimeStampChangedBook - edited", reread.TBookMainElement.Name);
            Assert.True(reread.TBookMainElement.TimeStamp > stored.TBookMainElement.TimeStamp);
        }

        [Fact]
        public async Task InsertDXUnit_UsingLargeAmountOfMultiItems_Ok()
        {
            // Init
            var itemAmount = 10000;
            var textLength = 10000;

            var text = Enumerable.Range(0, itemAmount).Select(x => GetRandomString(textLength)).ToHashSet();
            var item = TBookUnitFactory.GetItemWithText(Guid.Empty, $"NameBook", text);

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
            Assert.NotEqual(Guid.Empty, result);
            Assert.Equal(text.Count(), item.TBookChapterElement.Announced.Count(x => text.Contains(x.Text)));

            var existingItem = await EstimatePerformanceAsync(async () =>
            {
                return await this._reader.GetItemAsync<TBookUnit>(item.Id);
            }, $"GetItemAsync unit with {text.Count()} multi items");

            Assert.NotNull(existingItem);
            Assert.Equal(text.Count(), existingItem.TBookChapterElement.Announced.Count(x => text.Contains(x.Text)));
        }

        [Fact]
        public async Task InsertDXUnit_UsingEnumColumnsWithFullMode_Ok()
        {
            // Init
            var objectKindEnum = new DXObjectEnumElement()
            {
                AllowNull = true,
                Name = "ObjectKind",
                EnumType = new Guid("018fa544-ffbe-7e77-9a08-d6c3808267f9"),
                EnumKey = new Guid("018fa545-078e-7f2c-bbcd-3d9481ce9dea")
            };

            var relaionTypeEnum = new DXObjectEnumElement()
            {
                Id = Guid.NewGuid(),
                AllowNull = true,
                Name = "RelationType",
                EnumType = new Guid("018fa545-0f5e-7873-aaef-714bedee7c07"),
                EnumKey = new Guid("018fa545-172e-76b9-a4eb-c2bbda8789f0")
            };

            var dxUnit = new DXUnitDefinitionUnit()
            {
                Name = "DXUnitWithEnum",
                DXTitleExpression = "Name",
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
            var createdDXUnit = await this._reader.GetItemAsync<DXUnitDefinitionUnit>(dxUnit.Id);

            Assert.NotNull(createdDXUnit);

            Assert.NotEmpty(createdDXUnit.DXObjectEnumElement.Announced);

            var createdEnums = createdDXUnit.DXObjectEnumElement.Announced.SingleOrDefault(x => objectKindEnum.Id == x.Id);
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

            var instancesWithObjectKind = await this._reader.GetItemsAsync<DXUnitWithKindEnum>();

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
            createdDXUnit = await this._reader.GetItemAsync<DXUnitDefinitionUnit>(dxUnit.Id);

            Assert.NotNull(createdDXUnit);

            Assert.NotEmpty(createdDXUnit.DXObjectEnumElement.Announced);

            createdEnums = createdDXUnit.DXObjectEnumElement.Announced.SingleOrDefault(x => relaionTypeEnum.Id == x.Id);
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

            var instanceWithRelationType = await this._reader.GetItemsAsync<DXUnitWithRelationTypeEnum>();

            Assert.Empty(instanceWithRelationType);
        }

        [Fact]
        public async Task InsertOrUpdate_UsingExistingCoreDXUnitWithTargetModeForMultiElements_Ok()
        {
            // Init         
            var unitId = new Guid("018fa545-5996-7b4b-8a6e-b6af9c1207dc");
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
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            Id = unitId,
                            TimeStamp = unitTime,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject("DXUnitToUnitRelationElement") },
                                { "DXTitleExpression", JToken.FromObject("OwnRelationName") },
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
            var insertedId = await this._service.InsertOrUpdateAsync(JObject.FromObject(dxUnitBlock));

            // Assert
            Assert.NotEqual(Guid.Empty, insertedId);
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
                    Items = new List<DXElementRecord>(),
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

