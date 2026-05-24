using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.MigrationServiceTests
{
    [Collection("DX:one-time")]
    public sealed class MigrationServiceSyncTests : IntTestController
    {
        private const string ExpectedUnitId = "018fa549-7c2e-7b59-b61e-5454e89fdd51";
        private const string DeletedUnitId1 = "018fa549-6c8e-7e4f-b6d1-9c9e349c1e2d";
        private const string DeletedUnitId2 = "018fa549-745e-7029-b07d-ee08c316b8f4";

        private const string ScopeFilter =
            "TUserMainElement.Name = 'SyncSeed-A' OR " +
            "TUserMainElement.Name = 'SyncSeed-B' OR " +
            "TUserMainElement.Name = 'SyncSeed-C'";

        private readonly IDXMigrationService _migrationService;
        private readonly IDXUnitDataReader _unitDataReader;

        public MigrationServiceSyncTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            _migrationService = this.ServiceProvider.GetRequiredService<IDXMigrationService>();
            _unitDataReader = this.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
        }

        [Fact]
        public async Task MigrateCustomAsync_SyncWithDXFilter_DeletesMissingUnitsInScope()
        {
            await _migrationService.MigrateCustomAsync("MigrationScripts/Sync.json");

            var remaining = await _unitDataReader.GetItemAsync("TUserUnit", new Guid(ExpectedUnitId));
            var deleted1 = await _unitDataReader.GetItemAsync("TUserUnit", new Guid(DeletedUnitId1));
            var deleted2 = await _unitDataReader.GetItemAsync("TUserUnit", new Guid(DeletedUnitId2));

            Assert.NotNull(remaining);
            Assert.Null(deleted1);
            Assert.Null(deleted2);

            var stillInScope = await _unitDataReader.GetItemsAsync("TUserUnit", ScopeFilter);
            var stillInScopeItems = stillInScope?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items;
            Assert.NotNull(stillInScopeItems);
            Assert.Single(stillInScopeItems!);
        }
    }
}
