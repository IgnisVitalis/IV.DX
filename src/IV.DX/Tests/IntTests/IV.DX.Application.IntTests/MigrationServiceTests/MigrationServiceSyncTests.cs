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
        private const string ExpectedUnitId = "e6845581-680e-4d6d-9d75-bb9c2d747aa5";
        private const string DeletedUnitId1 = "702689cb-fe31-400a-b84d-9ceaaf548deb";
        private const string DeletedUnitId2 = "869fab30-c037-483f-86ac-e0fad536a5b9";

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
