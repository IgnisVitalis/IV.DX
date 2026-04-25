using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXQueryPublicAccessTests : IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly IDXUnitDataService _dataService;
        private readonly IDXQueryResultProvider _queryProvider;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXStructureCache _structureCache;
        private readonly IDXExecutionContextAccessor _executionContextAccessor;

        public DXQueryPublicAccessTests(DXTestFixture fx)
        {
            _scope = fx.Root.CreateScope();
            _dataService = _scope.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _queryProvider = _scope.ServiceProvider.GetRequiredService<IDXQueryResultProvider>();
            _genericRepo = _scope.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _structureCache = _scope.ServiceProvider.GetRequiredService<IDXStructureCache>();
            _executionContextAccessor = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
        }

        [Fact]
        public async Task AnonymousQuery_WhenTypeIsPublicRead_ReturnsContent()
        {
            var userUnitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>("Name = 'TUserUnit'").First();
            var originalUserIsPublicRead = userUnitDef.IsPublicRead;

            var itemId = Guid.NewGuid();
            var queryId = Guid.NewGuid();

            await RunAsSystemAsync(async () =>
            {
                SetIsPublicRead(userUnitDef.Id, true);
                await _structureCache.RefreshAsync();

                await _dataService.InsertAsync(TUserUnitFactory.GetItem(
                    itemId,
                    "QueryPublic",
                    "User",
                    DateTime.UtcNow.Date));

                await InsertQueryForTypeAsync(queryId, userUnitDef.Id);
            });

            try
            {
                var result = await _queryProvider.GetAsync(queryId);
                Assert.NotNull(result);

                var ids = ExtractContentIds(result!);
                Assert.Contains(itemId, ids);
            }
            finally
            {
                await RunAsSystemAsync(async () =>
                {
                    await DeleteByIdsAsync("DXQueryUnit", new[] { queryId });
                    await DeleteByIdsAsync("TUserUnit", new[] { itemId });

                    SetIsPublicRead(userUnitDef.Id, originalUserIsPublicRead);
                    await _structureCache.RefreshAsync();
                });
            }
        }

        [Fact]
        public async Task AnonymousQuery_WhenTypeIsPrivateAndEntryIsPublic_ReturnsOnlyMappedContent()
        {
            var userUnitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>("Name = 'TUserUnit'").First();
            var originalUserIsPublicRead = userUnitDef.IsPublicRead;

            var publicItemId = Guid.NewGuid();
            var privateItemId = Guid.NewGuid();
            var queryId = Guid.NewGuid();
            var publicAccessId = Guid.NewGuid();

            await RunAsSystemAsync(async () =>
            {
                SetIsPublicRead(userUnitDef.Id, false);
                await _structureCache.RefreshAsync();

                await _dataService.InsertAsync(TUserUnitFactory.GetItem(
                    publicItemId,
                    "PublicFromQuery",
                    "Allowed",
                    DateTime.UtcNow.Date));

                await _dataService.InsertAsync(TUserUnitFactory.GetItem(
                    privateItemId,
                    "PrivateFromQuery",
                    "Blocked",
                    DateTime.UtcNow.Date));

                await _dataService.InsertAsync(new DXPublicAccessUnit
                {
                    Id = publicAccessId,
                    TimeStamp = DateTime.UtcNow,
                    DXUnitDefinition = userUnitDef.Id,
                    PublicDXUnitId = publicItemId
                });

                await InsertQueryForTypeAsync(queryId, userUnitDef.Id);
            });

            try
            {
                var result = await _queryProvider.GetAsync(queryId);
                Assert.NotNull(result);

                var ids = ExtractContentIds(result!);
                Assert.Contains(publicItemId, ids);
                Assert.DoesNotContain(privateItemId, ids);

                var DXTitleExpressions = (await _queryProvider.GetDXTitleExpressionsAsync(nameof(TUserUnit))).Select(x => x.Id).ToHashSet();
                Assert.Contains(publicItemId, DXTitleExpressions);
                Assert.DoesNotContain(privateItemId, DXTitleExpressions);
            }
            finally
            {
                await RunAsSystemAsync(async () =>
                {
                    await DeleteByIdsAsync("DXPublicAccessUnit", new[] { publicAccessId });
                    await DeleteByIdsAsync("DXQueryUnit", new[] { queryId });
                    await DeleteByIdsAsync("TUserUnit", new[] { publicItemId, privateItemId });

                    SetIsPublicRead(userUnitDef.Id, originalUserIsPublicRead);
                    await _structureCache.RefreshAsync();
                });
            }
        }

        private HashSet<Guid> ExtractContentIds(JObject queryResult)
        {
            var contentBlock = queryResult["Content"]?.ToObject<DXDataBlock<DXUnitRecord>>();
            var records = contentBlock?.Data?.Items ?? new List<DXUnitRecord>();
            return records.Select(x => x.Id).ToHashSet();
        }

        private void SetIsPublicRead(Guid unitDefinitionId, bool isPublicRead)
        {
            var mutable = _genericRepo.GetDXUnit<DXUnitDefinitionUnit>(unitDefinitionId);
            mutable.IsPublicRead = isPublicRead;
            _genericRepo.Update(mutable);
        }

        private async Task InsertQueryForTypeAsync(Guid queryId, Guid unitDefinitionId)
        {
            var queryColumnId = Guid.NewGuid();

            await _dataService.InsertOrUpdateAsync(new DXDataBlock<DXUnitRecord>
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
                            Id = queryId,
                            TimeStamp = DateTime.UtcNow,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Name", JToken.FromObject($"Q_{Guid.NewGuid():N}") },
                                { "Description", JToken.FromObject("Public access test query") },
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
                                                Id = queryColumnId,
                                                DXUnitId = queryId,
                                                TimeStamp = DateTime.UtcNow,
                                                Fields = new Dictionary<string, JToken>
                                                {
                                                    { "Name", JToken.FromObject("VisibleId") },
                                                    { "Expression", JToken.FromObject("Id") },
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
            });
        }

        private async Task RunAsSystemAsync(Func<Task> action)
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:query-public-access-tests",
                IsSystem = true
            });

            await action();
        }

        private Task<bool> DeleteByIdsAsync(string typeName, IEnumerable<Guid> ids)
        {
            var deleteRefs = ids
                .Where(x => x != Guid.Empty)
                .Select(x => new DXDeleteRef { Id = x })
                .ToList();

            if (deleteRefs.Count == 0)
                return Task.FromResult(true);

            return _dataService.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = typeName,
                    Op = "Patch",
                    IsMulti = true
                },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = deleteRefs
                }
            });
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
