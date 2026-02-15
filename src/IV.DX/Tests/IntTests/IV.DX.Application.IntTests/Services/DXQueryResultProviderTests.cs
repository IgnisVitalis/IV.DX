using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXQueryResultProviderTests : IntTestController
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXQueryResultProvider _queryProvider;
        private readonly IDXExecutionContextAccessor _executionContextAccessor;

        public DXQueryResultProviderTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            _dataService = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _queryProvider = base.ServiceProvider.GetRequiredService<IDXQueryResultProvider>();
            _executionContextAccessor = base.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
        }

        [Fact]
        public async Task GetAsync_WhenQuerySelectsEncryptedString_MasksValue_Ok()
        {
            var cache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();
            await cache.RefreshAsync();

            var now = DateTime.UtcNow;

            var unitDefinitionId = Guid.NewGuid();
            var columnDefinitionId = Guid.NewGuid();
            var unitName = $"DXQueryMaskedSecretUnit_{Guid.NewGuid():N}";

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
                            ID = unitDefinitionId,
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject(unitName) },
                                { "DisplayValue", JToken.FromObject("Secret") },
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
                                                ID = columnDefinitionId,
                                                DXUnitID = unitDefinitionId,
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

            await _dataService.InsertOrUpdateAsync(unitDefinitionBlock);
            await cache.RefreshAsync();

            var instanceId = Guid.NewGuid();
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
                            ID = instanceId,
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(plaintext) }
                            }
                        }
                    }
                }
            };

            await _dataService.InsertAsync(insertBlock);

            var queryId = Guid.NewGuid();
            var queryColumnId = Guid.NewGuid();

            var queryBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = "DXQueryUnit"
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            ID = queryId,
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject($"Q_{Guid.NewGuid():N}") },
                                { "Description", JValue.CreateNull() },
                                { "DXUnitDefinition", JToken.FromObject(unitDefinitionId) }
                            },
                            DXElements = new Dictionary<string, DXDataBlock<DXElementRecord>>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["DXQueryColumnElement"] = new DXDataBlock<DXElementRecord>
                                {
                                    Meta = new DXMeta
                                    {
                                        Kind = "DXElement",
                                        Type = "DXQueryColumnElement",
                                        Op = "Patch",
                                        IsMulti = true
                                    },
                                    Data = new DXData<DXElementRecord>
                                    {
                                        Items = new List<DXElementRecord>
                                        {
                                            new DXElementRecord
                                            {
                                                ID = queryColumnId,
                                                DXUnitID = queryId,
                                                TimeStamp = now,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "Name", JToken.FromObject("Secret") },
                                                    { "Expression", JToken.FromObject("Secret") },
                                                    { "Order", JToken.FromObject(0) }
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

            await _dataService.InsertOrUpdateAsync(queryBlock);

            var result = await _queryProvider.GetAsync(queryId, dxFilterID: null);

            Assert.NotNull(result);

            var content = result["Content"] as JObject;
            Assert.NotNull(content);

            var contentBlock = content!.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.NotNull(contentBlock);

            var item = contentBlock!.Data?.Items?.SingleOrDefault(x => x.ID == instanceId);
            Assert.NotNull(item);

            var secret = item!.Fields != null && item.Fields.TryGetValue("Secret", out var v) ? v?.ToString() : null;
            Assert.Equal(string.Empty, secret);

            await _dataService.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = unitName },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { ID = instanceId } }
                }
            });

            await _dataService.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = "DXQueryUnit" },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { ID = queryId } }
                }
            });
        }

        [Fact]
        public async Task GetDisplayValuesAsync_WhenReadAccessDenied_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "query-provider-test-user",
                AllowedReadUnitTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "DXRoleUnit"
                }
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _queryProvider.GetDisplayValuesAsync("DXUnitDefinitionUnit"));
        }

        [Fact]
        public async Task GetDisplayValuesAsync_WhenTenantDeniedAndMembershipOrGroupAllowed_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "hierarchy-query-user",
                TenantReadUnitTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "DXRoleUnit"
                },
                MembershipReadUnitTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "DXUnitDefinitionUnit"
                },
                GroupReadUnitTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "DXUnitDefinitionUnit"
                },
                ApplyGroupRestrictions = true
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _queryProvider.GetDisplayValuesAsync("DXUnitDefinitionUnit"));
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
    }
}
