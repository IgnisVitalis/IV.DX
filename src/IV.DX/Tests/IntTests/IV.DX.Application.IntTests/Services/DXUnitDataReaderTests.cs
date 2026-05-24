using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
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
    public class DXUnitDataReaderTests : IntTestController
    {
        // TDeviceUnit definition Id (from 01_01_0010_Test_TDeviceUnit.dx)
        private static readonly Guid TDeviceUnitDefinitionId = new Guid("018fa549-e1be-70ce-81f7-a6f7554ffdde");

        // Seeded TDeviceUnit instance IDs (from 01_01_0023_Test_TDeviceUnit.dx)
        private static readonly Guid TDeviceUnit1Id = new Guid("018fa54a-5ad6-7327-a7ea-2fd57bcab0ef");
        private static readonly Guid TDeviceUnit2Id = new Guid("018fa54a-62a6-77ca-8a5a-0412f03aa136");

        private readonly IDXUnitDataReader _reader;

        public DXUnitDataReaderTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            _reader = base.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
        }

        // -------------------------------------------------------------------
        // GetItemAsync(string typeName, Guid id)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemAsync_UsingTypeNameAndExistingId_ReturnsItem()
        {
            var item = await _reader.GetItemAsync("TDeviceUnit", TDeviceUnit1Id);

            var block  = item.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = block?.Data?.Items?.SingleOrDefault();

            Assert.NotNull(record);
            Assert.Equal(TDeviceUnit1Id, record!.Id);
        }

        [Fact]
        public async Task GetItemAsync_UsingTypeNameAndNonExistingId_ReturnsNull()
        {
            var item = await _reader.GetItemAsync("TDeviceUnit", Guid.NewGuid());

            Assert.Null(item);
        }

        // -------------------------------------------------------------------
        // GetItemsAsync(string typeName)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemsAsync_UsingTypeName_ReturnsAllItems()
        {
            var items = await _reader.GetItemsAsync("TDeviceUnit");

            Assert.NotNull(items);
            var block = items.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.NotNull(block?.Data?.Items);
            Assert.NotEmpty(block!.Data!.Items!);
        }

        // -------------------------------------------------------------------
        // GetItemsAsync(string typeName, IEnumerable<Guid> ids)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemsAsync_UsingTypeNameAndIds_ReturnsMatchingItems()
        {
            var ids   = new[] { TDeviceUnit1Id, TDeviceUnit2Id };
            var items = await _reader.GetItemsAsync("TDeviceUnit", (IEnumerable<Guid>)ids);

            var records = items?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items ?? [];

            Assert.Equal(2, records.Count);
            Assert.Contains(records, r => r.Id == TDeviceUnit1Id);
            Assert.Contains(records, r => r.Id == TDeviceUnit2Id);
        }

        // -------------------------------------------------------------------
        // GetItemsAsync(string typeName, string dxFilter)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemsAsync_UsingTypeNameAndFilter_ReturnsMatchingItems()
        {
            var items = await _reader.GetItemsAsync("TDeviceUnit", $"Id = '{TDeviceUnit1Id}'");

            var record = items?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items?.SingleOrDefault();
            Assert.NotNull(record);
            Assert.Equal(TDeviceUnit1Id, record!.Id);
        }

        [Fact]
        public async Task GetItemsAsync_UsingTypeNameAndNonMatchingFilter_ReturnsEmpty()
        {
            var items = await _reader.GetItemsAsync("TDeviceUnit", $"Id = '{Guid.NewGuid()}'");

            Assert.NotNull(items);
            var block = items.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.Empty(block?.Data?.Items ?? []);
        }

        // -------------------------------------------------------------------
        // GetItemAsync(Guid unitDefinitionId, Guid id)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemAsync_UsingDefinitionIdAndExistingInstanceId_ReturnsItem()
        {
            var item = await _reader.GetItemAsync(TDeviceUnitDefinitionId, TDeviceUnit1Id);

            var block  = item.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = block?.Data?.Items?.SingleOrDefault();

            Assert.NotNull(record);
            Assert.Equal(TDeviceUnit1Id, record!.Id);
        }

        [Fact]
        public async Task GetItemAsync_UsingDefinitionIdAndNonExistingInstanceId_ReturnsNull()
        {
            var item = await _reader.GetItemAsync(TDeviceUnitDefinitionId, Guid.NewGuid());

            Assert.Null(item);
        }

        [Fact]
        public async Task GetItemAsync_UsingUnknownDefinitionId_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _reader.GetItemAsync(Guid.NewGuid(), TDeviceUnit1Id));
        }

        // -------------------------------------------------------------------
        // GetItemsAsync(Guid unitDefinitionId)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemsAsync_UsingDefinitionId_ReturnsAllItems()
        {
            var items = await _reader.GetItemsAsync(TDeviceUnitDefinitionId);

            Assert.NotNull(items);
            var block = items.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.NotNull(block?.Data?.Items);
            Assert.NotEmpty(block!.Data!.Items!);
        }

        [Fact]
        public async Task GetItemsAsync_UsingUnknownDefinitionId_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _reader.GetItemsAsync(Guid.NewGuid()));
        }

        // -------------------------------------------------------------------
        // GetItemsAsync(Guid unitDefinitionId, IEnumerable<Guid> ids)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemsAsync_UsingDefinitionIdAndInstanceIds_ReturnsMatchingItems()
        {
            var ids   = new[] { TDeviceUnit1Id, TDeviceUnit2Id };
            var items = await _reader.GetItemsAsync(TDeviceUnitDefinitionId, (IEnumerable<Guid>)ids);

            var records = items?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items ?? [];

            Assert.Equal(2, records.Count);
            Assert.Contains(records, r => r.Id == TDeviceUnit1Id);
            Assert.Contains(records, r => r.Id == TDeviceUnit2Id);
        }

        // -------------------------------------------------------------------
        // GetItemsAsync(Guid unitDefinitionId, string dxFilter)
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemsAsync_UsingDefinitionIdAndFilter_ReturnsMatchingItems()
        {
            var items = await _reader.GetItemsAsync(TDeviceUnitDefinitionId, $"Id = '{TDeviceUnit1Id}'");

            var record = items?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items?.SingleOrDefault();
            Assert.NotNull(record);
            Assert.Equal(TDeviceUnit1Id, record!.Id);
        }

        [Fact]
        public async Task GetItemsAsync_UsingDefinitionIdAndNonMatchingFilter_ReturnsEmpty()
        {
            var items = await _reader.GetItemsAsync(TDeviceUnitDefinitionId, $"Id = '{Guid.NewGuid()}'");

            Assert.NotNull(items);
            var block = items.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.Empty(block?.Data?.Items ?? []);
        }

        // -------------------------------------------------------------------
        // Consistency: Guid unitDefinitionId and string typeName yield the same result
        // -------------------------------------------------------------------

        [Fact]
        public async Task GetItemAsync_ByDefinitionIdAndByTypeName_ReturnSameResult()
        {
            var byName = await _reader.GetItemAsync("TDeviceUnit", TDeviceUnit1Id);
            var byId   = await _reader.GetItemAsync(TDeviceUnitDefinitionId, TDeviceUnit1Id);

            Assert.NotNull(byName);
            Assert.NotNull(byId);
            Assert.Equal(byName.ToString(), byId.ToString());
        }

        [Fact]
        public async Task GetItemsAsync_ByDefinitionIdAndByTypeName_ReturnSameCount()
        {
            var byName = await _reader.GetItemsAsync("TDeviceUnit");
            var byId   = await _reader.GetItemsAsync(TDeviceUnitDefinitionId);

            var countName = byName?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items?.Count ?? 0;
            var countId   = byId?.ToObject<DXDataBlock<DXUnitRecord>>()?.Data?.Items?.Count ?? 0;

            Assert.Equal(countName, countId);
        }
    }
}
