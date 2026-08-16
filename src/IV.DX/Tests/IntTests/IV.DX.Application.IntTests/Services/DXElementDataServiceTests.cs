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
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXElementDataServiceTests : IntTestController
    {
        private readonly IDXElementDataService _service;
        private readonly IDXUnitDataService _unitService;
        private readonly IDXUnitDataReader _unitReader;
        private readonly IDXUnitGenericRepository _unitGenericRepo;

        public DXElementDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXElementDataService>();
            this._unitService = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._unitReader = base.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
            this._unitGenericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingNewElement_ElementIsStoredUnderItsUnit()
        {
            var book = await CreateBookAsync("chapter-insert");

            var chapter = new TBookChapterElement
            {
                DXUnitId = book.Id,
                Number = 1,
                Text = "Chapter one"
            };

            var id = await _service.InsertOrUpdateAsync("TBookUnit", chapter);

            Assert.NotEqual(Guid.Empty, id);

            var stored = await _service.GetItemAsync<TBookChapterElement>("TBookUnit", id);

            Assert.NotNull(stored);
            Assert.Equal(book.Id, stored.DXUnitId);
            Assert.Equal(1, stored.Number);
            Assert.Equal("Chapter one", stored.Text);
        }

        [Fact]
        public async Task GetItemsByUnitAsync_UsingTwoBooks_ReturnsOnlyTheChaptersOfTheOneAsked()
        {
            var mine = await CreateBookAsync("chapters-mine", "a", "b");
            var other = await CreateBookAsync("chapters-other", "c");

            var chapters = await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", mine.Id);

            Assert.Equal(2, chapters.Count());
            Assert.All(chapters, x => Assert.Equal(mine.Id, x.DXUnitId));
            Assert.DoesNotContain(chapters, x => x.DXUnitId == other.Id);
        }

        [Fact]
        public async Task GetItemAsync_UsingUnknownId_ReturnsNull()
        {
            var missing = await _service.GetItemAsync<TBookChapterElement>("TBookUnit", Guid.CreateVersion7());

            Assert.Null(missing);
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingExistingElement_LeavesTheUnitTimeStampAlone()
        {
            var book = await CreateBookAsync("chapter-update", "original");

            var before = await _unitReader.GetItemAsync<TBookUnit>(book.Id);
            var chapter = (await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", book.Id)).Single();

            chapter.Text = "rewritten";
            await _service.InsertOrUpdateAsync("TBookUnit", chapter);

            var stored = await _service.GetItemAsync<TBookChapterElement>("TBookUnit", chapter.Id);
            var after = await _unitReader.GetItemAsync<TBookUnit>(book.Id);

            Assert.Equal("rewritten", stored!.Text);

            // The element's own row moved; the unit's did not, because nothing in it changed.
            Assert.True(stored.TimeStamp > chapter.TimeStamp);
            Assert.Equal(before!.TimeStamp, after!.TimeStamp);
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingElementOfAnotherUnit_Throws()
        {
            var mine = await CreateBookAsync("reparent-mine");
            var other = await CreateBookAsync("reparent-other", "foreign chapter");

            var foreignChapter = (await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", other.Id)).Single();

            // The element exists and belongs to another book; naming my own book alongside it must
            // not move it, however the access check on my book turns out.
            foreignChapter.DXUnitId = mine.Id;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.InsertOrUpdateAsync("TBookUnit", foreignChapter));

            var untouched = await _service.GetItemAsync<TBookChapterElement>("TBookUnit", foreignChapter.Id);

            Assert.Equal(other.Id, untouched!.DXUnitId);
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingUnknownUnit_Throws()
        {
            var chapter = new TBookChapterElement
            {
                DXUnitId = Guid.CreateVersion7(),
                Number = 1,
                Text = "orphan"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.InsertOrUpdateAsync("TBookUnit", chapter));
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingElementWithoutUnit_Throws()
        {
            var chapter = new TBookChapterElement
            {
                Number = 1,
                Text = "no owner"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.InsertOrUpdateAsync("TBookUnit", chapter));
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingBlockOfSeveralElements_WritesAllOfThem()
        {
            var book = await CreateBookAsync("chapter-block");

            var block = ChapterBlock(
                NewChapterRecord(book.Id, 1, "one"),
                NewChapterRecord(book.Id, 2, "two"),
                NewChapterRecord(book.Id, 3, "three"));

            var ids = (await _service.InsertOrUpdateAsync(block)).ToList();

            Assert.Equal(3, ids.Count);
            Assert.All(ids, x => Assert.NotEqual(Guid.Empty, x));

            var stored = await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", book.Id);

            Assert.Equal(3, stored.Count());
            Assert.Equal(new[] { "one", "two", "three" }, stored.OrderBy(x => x.Number).Select(x => x.Text));
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingBlockWithOneUnwritableElement_WritesNothing()
        {
            var book = await CreateBookAsync("chapter-block-rejected");

            var block = ChapterBlock(
                NewChapterRecord(book.Id, 1, "good"),
                NewChapterRecord(Guid.CreateVersion7(), 2, "points at no book"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InsertOrUpdateAsync(block));

            // The block is one write: the acceptable record must not have landed on its own.
            var stored = await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", book.Id);

            Assert.Empty(stored);
        }

        [Fact]
        public async Task DeleteAsync_UsingBlock_RemovesOnlyTheListedElements()
        {
            var book = await CreateBookAsync("chapter-delete", "keep", "drop");

            var chapters = (await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", book.Id)).ToList();
            var doomed = chapters.Single(x => x.Text == "drop");

            var block = new DXDataBlock<DXElementRecord>
            {
                Meta = ChapterMeta(),
                Data = new DXData<DXElementRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { Id = doomed.Id } }
                }
            };

            var deleted = await _service.DeleteAsync(block);

            Assert.True(deleted);

            var remaining = await _service.GetItemsByUnitAsync<TBookChapterElement>("TBookUnit", book.Id);

            Assert.Equal("keep", Assert.Single(remaining).Text);
        }

        [Fact]
        public async Task DeleteAsync_UsingUnknownElement_ReportsNothingDeleted()
        {
            var block = new DXDataBlock<DXElementRecord>
            {
                Meta = ChapterMeta(),
                Data = new DXData<DXElementRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { Id = Guid.CreateVersion7() } }
                }
            };

            Assert.False(await _service.DeleteAsync(block));
        }

        [Fact]
        public async Task InsertOrUpdateAsync_UsingBlockWithoutUnitContext_Throws()
        {
            var book = await CreateBookAsync("chapter-no-context");

            var block = ChapterBlock(NewChapterRecord(book.Id, 1, "one"));
            block.Meta.DXUnitContext = null;

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.InsertOrUpdateAsync(block));
        }

        private async Task<TBookUnit> CreateBookAsync(string name, params string[] chapters)
        {
            var id = Guid.CreateVersion7();

            var book = chapters.Length == 0
                ? TBookUnitFactory.GetItem(id, name)
                : TBookUnitFactory.GetItemWithText(id, name, chapters);

            await _unitService.InsertAsync(book);

            // InsertAsync assigns fresh ids, so the book has to be tracked by the one it came back with.
            var created = book;
            base._finalizationAction += () => RunActionSafety(() => _unitGenericRepo.Delete(created));

            return book;
        }

        private static DXMeta ChapterMeta() => new DXMeta
        {
            Kind = "DXElement",
            Type = "TBookChapterElement",
            DXUnitContext = "TBookUnit",
            IsMulti = true,
            IsRequired = false
        };

        private static DXDataBlock<DXElementRecord> ChapterBlock(params DXElementRecord[] records) =>
            new DXDataBlock<DXElementRecord>
            {
                Meta = ChapterMeta(),
                Data = new DXData<DXElementRecord> { Items = records.ToList() }
            };

        private static DXElementRecord NewChapterRecord(Guid bookId, int number, string text) =>
            new DXElementRecord
            {
                DXUnitId = bookId,
                Fields = new Dictionary<string, JToken>
                {
                    { "Number", JToken.FromObject(number) },
                    { "Text", JToken.FromObject(text) }
                }
            };
    }
}
