using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Services;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXElementDtoServiceTests : IntTestController
    {
        private readonly IDXElementDtoService<ChapterDto, ChapterDto> _service;
        private readonly IDXUnitDataService _unitService;
        private readonly IDXUnitGenericRepository _unitGenericRepo;

        public DXElementDtoServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._unitService = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._unitGenericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();

            // Built directly rather than resolved: the DI wiring has its own tests in
            // IV.DX.Hosting.UnitTests, and registering a mapper would mean reaching into the shared
            // fixture's container.
            this._service = new DXElementDtoService<ChapterDto, ChapterDto, TBookChapterElement, TBookUnit, ChapterMapper>(
                base.ServiceProvider.GetRequiredService<IDXElementDataService>(),
                new ChapterMapper());
        }

        [Fact]
        public async Task CreateAsync_ThenGetAsync_RoundTripsThroughTheDto()
        {
            var book = await CreateBookAsync("dto-create");

            var id = await _service.CreateAsync(book.Id, new ChapterDto { Number = 1, Text = "one" });

            Assert.NotEqual(Guid.Empty, id);

            var dto = await _service.GetAsync(id);

            Assert.NotNull(dto);
            Assert.Equal(id, dto.Id);
            Assert.Equal(book.Id, dto.BookId);
            Assert.Equal(1, dto.Number);
            Assert.Equal("one", dto.Text);
        }

        [Fact]
        public async Task CreateAsync_UsingDtoCarryingAnId_AssignsItsOwn()
        {
            var book = await CreateBookAsync("dto-create-id");
            var supplied = Guid.CreateVersion7();

            var id = await _service.CreateAsync(book.Id, new ChapterDto { Id = supplied, Number = 1, Text = "one" });

            Assert.NotEqual(supplied, id);
            Assert.Null(await _service.GetAsync(supplied));
        }

        [Fact]
        public async Task GetByUnitAsync_ReturnsEveryChapterOfThatBookOnly()
        {
            var mine = await CreateBookAsync("dto-list-mine", "a", "b");
            var other = await CreateBookAsync("dto-list-other", "c");

            var dtos = (await _service.GetByUnitAsync(mine.Id)).ToList();

            Assert.Equal(2, dtos.Count);
            Assert.All(dtos, x => Assert.Equal(mine.Id, x.BookId));
            Assert.Equal(new[] { "a", "b" }, dtos.OrderBy(x => x.Number).Select(x => x.Text));
            Assert.DoesNotContain(dtos, x => x.BookId == other.Id);
        }

        [Fact]
        public async Task GetAsync_UsingUnknownId_ReturnsNull()
        {
            Assert.Null(await _service.GetAsync(Guid.CreateVersion7()));
        }

        [Fact]
        public async Task UpdateAsync_UsingExistingChapter_AppliesTheChange()
        {
            var book = await CreateBookAsync("dto-update", "before");

            var dto = (await _service.GetByUnitAsync(book.Id)).Single();
            dto.Text = "after";

            Assert.True(await _service.UpdateAsync(dto));

            var reread = await _service.GetAsync(dto.Id);

            Assert.Equal("after", reread!.Text);
            // The owner comes from storage, so an update cannot move the chapter elsewhere.
            Assert.Equal(book.Id, reread.BookId);
        }

        [Fact]
        public async Task UpdateAsync_UsingUnknownId_ReportsFalse()
        {
            var dto = new ChapterDto { Id = Guid.CreateVersion7(), Number = 1, Text = "ghost" };

            Assert.False(await _service.UpdateAsync(dto));
        }

        [Fact]
        public async Task DeleteAsync_UsingExistingChapter_RemovesIt()
        {
            var book = await CreateBookAsync("dto-delete", "keep", "drop");

            var doomed = (await _service.GetByUnitAsync(book.Id)).Single(x => x.Text == "drop");

            Assert.True(await _service.DeleteAsync(doomed.Id));

            var remaining = await _service.GetByUnitAsync(book.Id);

            Assert.Equal("keep", Assert.Single(remaining).Text);
        }

        [Fact]
        public async Task DeleteAsync_UsingUnknownId_ReportsFalse()
        {
            Assert.False(await _service.DeleteAsync(Guid.CreateVersion7()));
        }

        private async Task<TBookUnit> CreateBookAsync(string name, params string[] chapters)
        {
            var id = Guid.CreateVersion7();

            var book = chapters.Length == 0
                ? TBookUnitFactory.GetItem(id, name)
                : TBookUnitFactory.GetItemWithText(id, name, chapters);

            await _unitService.InsertAsync(book);

            var created = book;
            base._finalizationAction += () => RunActionSafety(() => _unitGenericRepo.Delete(created));

            return book;
        }

        public sealed class ChapterDto
        {
            public Guid Id { get; set; }
            public Guid BookId { get; set; }
            public int Number { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        private sealed class ChapterMapper : DXElementMapper<ChapterDto, ChapterDto, TBookChapterElement, TBookUnit>
        {
            public override Task<ChapterDto> ToDtoAsync(TBookChapterElement element, CancellationToken ct = default)
                => Task.FromResult(new ChapterDto
                {
                    Id = element.Id,
                    BookId = element.DXUnitId,
                    Number = element.Number,
                    Text = element.Text
                });

            public override Task<TBookChapterElement> ToElementAsync(ChapterDto dto, CancellationToken ct = default)
                => Task.FromResult(new TBookChapterElement
                {
                    Id = dto.Id,
                    Number = dto.Number,
                    Text = dto.Text
                });
        }
    }
}
