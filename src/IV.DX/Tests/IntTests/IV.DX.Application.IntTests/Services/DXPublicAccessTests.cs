using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXPublicAccessTests : IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitDataReader _reader;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXStructureCache _structureCache;
        private readonly IDXExecutionContextAccessor _executionContextAccessor;

        public DXPublicAccessTests(DXTestFixture fx)
        {
            _scope = fx.Root.CreateScope();
            _dataService = _scope.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _reader = _scope.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
            _genericRepo = _scope.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _structureCache = _scope.ServiceProvider.GetRequiredService<IDXStructureCache>();
            _executionContextAccessor = _scope.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
        }

        [Fact]
        public async Task AnonymousRead_WhenUnitTypeIsPublicRead_ReturnsItems()
        {
            var unitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>("Name = 'TUserUnit'").First();
            var originalIsPublicRead = unitDef.IsPublicRead;
            var insertedId = Guid.NewGuid();

            await RunAsSystemAsync(async () =>
            {
                var mutable = _genericRepo.GetDXUnit<DXUnitDefinitionUnit>(unitDef.Id);
                mutable.IsPublicRead = true;
                _genericRepo.Update(mutable);

                await _structureCache.RefreshAsync();

                await _dataService.InsertAsync(TUserUnitFactory.GetItem(
                    insertedId,
                    "PublicUser",
                    "ByType",
                    DateTime.UtcNow.Date));
            });

            try
            {
                var items = (await _reader.GetItemsAsync<TUserUnit>()).Select(x => x.Id).ToHashSet();
                Assert.Contains(insertedId, items);
            }
            finally
            {
                await RunAsSystemAsync(async () =>
                {
                    await DeleteByIdsAsync("TUserUnit", new[] { insertedId });

                    var mutable = _genericRepo.GetDXUnit<DXUnitDefinitionUnit>(unitDef.Id);
                    mutable.IsPublicRead = originalIsPublicRead;
                    _genericRepo.Update(mutable);

                    await _structureCache.RefreshAsync();
                });
            }
        }

        [Fact]
        public async Task AnonymousRead_WhenPrivateTypeHasPublicEntry_ReturnsOnlyPublicEntry()
        {
            var unitDef = _genericRepo.GetDXUnits<DXUnitDefinitionUnit>("Name = 'TUserUnit'").First();
            var originalIsPublicRead = unitDef.IsPublicRead;

            var publicItemId = Guid.NewGuid();
            var privateItemId = Guid.NewGuid();
            var publicAccessId = Guid.NewGuid();

            await RunAsSystemAsync(async () =>
            {
                var mutable = _genericRepo.GetDXUnit<DXUnitDefinitionUnit>(unitDef.Id);
                mutable.IsPublicRead = false;
                _genericRepo.Update(mutable);

                await _structureCache.RefreshAsync();

                await _dataService.InsertAsync(TUserUnitFactory.GetItem(
                    publicItemId,
                    "PublicEntry",
                    "Allowed",
                    DateTime.UtcNow.Date));

                await _dataService.InsertAsync(TUserUnitFactory.GetItem(
                    privateItemId,
                    "PrivateEntry",
                    "Blocked",
                    DateTime.UtcNow.Date));

                await _dataService.InsertAsync(new DXPublicAccessUnit
                {
                    Id = publicAccessId,
                    TimeStamp = DateTime.UtcNow,
                    DXUnitDefinition = unitDef.Id,
                    PublicDXUnitId = publicItemId
                });
            });

            try
            {
                var publicItem = await _reader.GetItemAsync<TUserUnit>(publicItemId);
                var privateItem = await _reader.GetItemAsync<TUserUnit>(privateItemId);
                var visibleIds = (await _reader.GetItemsAsync<TUserUnit>()).Select(x => x.Id).ToHashSet();

                Assert.NotNull(publicItem);
                Assert.Null(privateItem);
                Assert.Contains(publicItemId, visibleIds);
                Assert.DoesNotContain(privateItemId, visibleIds);
            }
            finally
            {
                await RunAsSystemAsync(async () =>
                {
                    await DeleteByIdsAsync("DXPublicAccessUnit", new[] { publicAccessId });
                    await DeleteByIdsAsync("TUserUnit", new[] { publicItemId, privateItemId });

                    var mutable = _genericRepo.GetDXUnit<DXUnitDefinitionUnit>(unitDef.Id);
                    mutable.IsPublicRead = originalIsPublicRead;
                    _genericRepo.Update(mutable);

                    await _structureCache.RefreshAsync();
                });
            }
        }

        private async Task RunAsSystemAsync(Func<Task> action)
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "system:public-access-tests",
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
