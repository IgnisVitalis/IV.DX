using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Schema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.DataServiceTests
{
    [Collection("DX:one-time")]
    public class DXUniqueColumnsTests : IntTestController
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXTestSchemaHelper _schemaHelper;

        public DXUniqueColumnsTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            _dataService = ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _genericRepo = ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _schemaHelper = ServiceProvider.GetRequiredService<IDXTestSchemaHelper>();
        }

        // UC name matches GetUniqueConstraintName: UC_{table}_{cols sorted alphabetically joined by _}
        private static string UC(string table, params string[] cols) =>
            $"UC_{table}_{string.Join("_", cols.OrderBy(c => c))}";

        private static DXElementDefinitionUnit BuildElement(string name)
        {
            return new DXElementDefinitionUnit
            {
                Name = name,
                DXTitleExpression = name,
                DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>
                {
                    Mode = MultiElementsMode.Target,
                    Announced = new HashSet<DXColumnDefinitionElement>
                    {
                        new DXColumnDefinitionElement
                        {
                            Name = "col1",
                            ColumnType = DXColumnTypeEnum.String,
                            Length = 100,
                            DefaultValue = "''"
                        },
                        new DXColumnDefinitionElement
                        {
                            Name = "col2",
                            ColumnType = DXColumnTypeEnum.String,
                            Length = 100,
                            DefaultValue = "''"
                        }
                    }
                }
            };
        }

        private static DXUniqueColumnsElement UniqueEntry(Guid elementId, string columns) =>
            new DXUniqueColumnsElement { Id = Guid.NewGuid(), DXUnitId = elementId, Columns = columns };

        // ── TargetMode ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task TargetMode_Insert_SingleConstraint_OneElementRecordAndSchemaConstraint()
        {
            var element = BuildElement("UCTest01");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(Guid.Empty, "col1,col2") }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            var saved = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Single(saved.DXUniqueColumnsElement.Announced);

            Assert.True(await _schemaHelper.UniqueConstraintExistsAsync("UCTest01", UC("UCTest01", "col1", "col2")));
        }

        [Fact]
        public async Task TargetMode_Insert_DuplicateColumnSetsInAnnounced_DeduplicatesToOneRecord()
        {
            var element = BuildElement("UCTest02");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement>
                {
                    UniqueEntry(Guid.Empty, "col1,col2"),
                    UniqueEntry(Guid.Empty, "col2,col1")   // same constraint, reversed order
                }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            var saved = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Single(saved.DXUniqueColumnsElement.Announced);

            Assert.True(await _schemaHelper.UniqueConstraintExistsAsync("UCTest02", UC("UCTest02", "col1", "col2")));
        }

        [Fact]
        public async Task TargetMode_Remove_WithReversedColumnOrder_RemovesElementRecordAndSchemaConstraint()
        {
            var element = BuildElement("UCTest03");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(Guid.Empty, "col1,col2") }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            // Remove using reversed column order — the handler must match to the existing DB record
            element.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement> { Mode = MultiElementsMode.Target };
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Deleted = new HashSet<DXUniqueColumnsElement> { UniqueEntry(element.Id, "col2,col1") }
            };

            await _dataService.UpdateAsync(element);

            var saved = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Empty(saved.DXUniqueColumnsElement.Announced);

            Assert.False(await _schemaHelper.UniqueConstraintExistsAsync("UCTest03", UC("UCTest03", "col1", "col2")));
        }

        [Fact]
        public async Task TargetMode_Update_AnnouncedConstraintAlreadyExists_ReversedOrder_NoError()
        {
            var element = BuildElement("UCTest07");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(Guid.Empty, "col1,col2") }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            // TargetMode update announcing the same constraint with reversed column order — must not fail
            element.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement> { Mode = MultiElementsMode.Target };
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(element.Id, "col2,col1") }
            };

            await _dataService.UpdateAsync(element);

            var saved = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Single(saved.DXUniqueColumnsElement.Announced);

            Assert.True(await _schemaHelper.UniqueConstraintExistsAsync("UCTest07", UC("UCTest07", "col1", "col2")));
        }

        // ── FullMode ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task FullMode_Update_ReversedColumnOrder_PreservesExistingElementRecord()
        {
            var element = BuildElement("UCTest04");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(Guid.Empty, "col1,col2") }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            var savedBefore = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            var existingElementId = savedBefore.DXUniqueColumnsElement.Announced.Single().Id;

            // FullMode update: same columns but reversed order — should be treated as identical
            element.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement> { Mode = MultiElementsMode.Target };
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Full,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(element.Id, "col2,col1") }
            };

            await _dataService.UpdateAsync(element);

            var savedAfter = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Single(savedAfter.DXUniqueColumnsElement.Announced);

            // The original element record must be preserved — no delete + insert
            Assert.Equal(existingElementId, savedAfter.DXUniqueColumnsElement.Announced.Single().Id);

            Assert.True(await _schemaHelper.UniqueConstraintExistsAsync("UCTest04", UC("UCTest04", "col1", "col2")));
        }

        [Fact]
        public async Task FullMode_Update_DuplicateColumnSetsInAnnounced_DeduplicatesToOneRecord()
        {
            var element = BuildElement("UCTest05");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(Guid.Empty, "col1,col2") }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            // FullMode update with duplicates in Announced
            element.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement> { Mode = MultiElementsMode.Target };
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Full,
                Announced = new HashSet<DXUniqueColumnsElement>
                {
                    UniqueEntry(element.Id, "col1,col2"),
                    UniqueEntry(element.Id, "col2,col1")   // duplicate of the above
                }
            };

            await _dataService.UpdateAsync(element);

            var saved = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Single(saved.DXUniqueColumnsElement.Announced);

            Assert.True(await _schemaHelper.UniqueConstraintExistsAsync("UCTest05", UC("UCTest05", "col1", "col2")));
        }

        [Fact]
        public async Task FullMode_Update_ReplaceConstraint_AddsNewRemovesOld()
        {
            var element = BuildElement("UCTest06");
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Target,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(Guid.Empty, "col1,col2") }
            };

            _finalizationAction = () => _dataService.DeleteAsync(element).Wait();

            await _dataService.InsertAsync(element);

            // FullMode update: replace col1+col2 constraint with col1 only
            element.DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement> { Mode = MultiElementsMode.Target };
            element.DXUniqueColumnsElement = new DXMultiElementsContainer<DXUniqueColumnsElement>
            {
                Mode = MultiElementsMode.Full,
                Announced = new HashSet<DXUniqueColumnsElement> { UniqueEntry(element.Id, "col1") }
            };

            await _dataService.UpdateAsync(element);

            var saved = _genericRepo.GetDXUnit<DXElementDefinitionUnit>(element.Id);
            Assert.Single(saved.DXUniqueColumnsElement.Announced);
            Assert.Equal("col1", saved.DXUniqueColumnsElement.Announced.Single().Columns.Trim());

            Assert.False(await _schemaHelper.UniqueConstraintExistsAsync("UCTest06", UC("UCTest06", "col1", "col2")));
            Assert.True(await _schemaHelper.UniqueConstraintExistsAsync("UCTest06", UC("UCTest06", "col1")));
        }
    }
}
