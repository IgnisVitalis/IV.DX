using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:dto-service")]
    public class DXUnitDtoServiceTests : IntTestController
    {
        private readonly IDXUnitDtoService<TBookDto, TBookDto> _service;

        public DXUnitDtoServiceTests(DXDtoServiceTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            _service = base.ServiceProvider.GetRequiredService<IDXUnitDtoService<TBookDto, TBookDto>>();
        }

        [Fact]
        public async Task SaveAsync_NewDto_ReturnsNonEmptyGuid()
        {
            // Init
            var dto = new TBookDto { TimeStamp = DateTime.UtcNow };

            // Action
            var id = await _service.SaveAsync(dto);

            // Assert
            Assert.NotEqual(Guid.Empty, id);

            base._finalizationAction = () => _service.DeleteAsync(id).Wait();
        }

        [Fact]
        public async Task SaveAsync_NewDto_AssignsGuidV7()
        {
            // Init
            var dto = new TBookDto { TimeStamp = DateTime.UtcNow };

            // Action
            var id = await _service.SaveAsync(dto);

            // Assert — Guid v7 stores timestamp in the top 48 bits; version nibble is 0x7
            var bytes = id.ToByteArray();
            var version = (bytes[7] >> 4) & 0xF;
            Assert.Equal(7, version);

            base._finalizationAction = () => _service.DeleteAsync(id).Wait();
        }

        [Fact]
        public async Task SaveAsync_NewDto_CanBeRetrievedById()
        {
            // Init
            var ts = DateTime.UtcNow;
            var dto = new TBookDto { TimeStamp = ts };

            // Action
            var id = await _service.SaveAsync(dto);

            base._finalizationAction = () => _service.DeleteAsync(id).Wait();

            // Assert
            var retrieved = await _service.GetAsync(id);

            Assert.NotNull(retrieved);
            Assert.Equal(id, retrieved.Id);
        }

        [Fact]
        public async Task SaveAsync_ExistingDto_UpdatesRecord()
        {
            // Init
            var dto = new TBookDto { TimeStamp = DateTime.UtcNow };
            var id = await _service.SaveAsync(dto);

            base._finalizationAction = () => _service.DeleteAsync(id).Wait();

            // Action — save again with the same Id (upsert path)
            var updatedDto = new TBookDto { Id = id, TimeStamp = DateTime.UtcNow.AddMinutes(1) };
            var updatedId = await _service.SaveAsync(updatedDto);

            // Assert — id is stable across update
            Assert.Equal(id, updatedId);
        }

        [Fact]
        public async Task GetAsync_NonExistingId_ReturnsNull()
        {
            var result = await _service.GetAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_AfterInsert_ContainsSavedItem()
        {
            // Init
            var dto = new TBookDto { TimeStamp = DateTime.UtcNow };
            var id = await _service.SaveAsync(dto);

            base._finalizationAction = () => _service.DeleteAsync(id).Wait();

            // Action
            var all = await _service.GetAllAsync();

            // Assert
            Assert.Contains(all, d => d.Id == id);
        }

        [Fact]
        public async Task GetAsync_FilterById_ReturnsMatchingItem()
        {
            // Init
            var dto = new TBookDto { TimeStamp = DateTime.UtcNow };
            var id = await _service.SaveAsync(dto);

            base._finalizationAction = () => _service.DeleteAsync(id).Wait();

            // Action
            var results = await _service.GetAsync($"Id = '{id}'");

            // Assert
            Assert.Single(results);
            Assert.Equal(id, results.Single().Id);
        }

        [Fact]
        public async Task GetAsync_FilterWithNoMatch_ReturnsEmpty()
        {
            var results = await _service.GetAsync($"Id = '{Guid.NewGuid()}'");

            Assert.Empty(results);
        }

        [Fact]
        public async Task DeleteAsync_ExistingItem_RemovesIt()
        {
            // Init
            var dto = new TBookDto { TimeStamp = DateTime.UtcNow };
            var id = await _service.SaveAsync(dto);

            // Action
            await _service.DeleteAsync(id);

            // Assert
            var retrieved = await _service.GetAsync(id);
            Assert.Null(retrieved);
        }
    }
}
