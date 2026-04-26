using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IV.DX.Application.Mappers;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Mappers
{
    public sealed class DXConventionMapperTests
    {
        // ── Fixtures ───────────────────────────────────────────────────────────────

        [DXElement("CmTag")]
        private sealed class CmTag : DXElement
        {
            public string Label { get; set; } = string.Empty;
        }

        [DXUnit("CmUnit")]
        private sealed class CmUnit : DXUnit
        {
            public string Name { get; set; } = string.Empty;
            public int Score { get; set; }
            public DXMultiElementsContainer<CmTag> Tags { get; set; } = new();
        }

        private sealed class CmDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Score { get; set; }
            public List<CmTag> Tags { get; set; } = [];
        }

        private sealed class CmDtoMissingProp
        {
            public Guid Id { get; set; }
            public string Ghost { get; set; } = string.Empty;  // no match in CmUnit
        }

        private sealed class CmDtoTypeMismatch
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Score { get; set; } = string.Empty;  // int in unit → mismatch
        }

        // ── ToDtoAsync ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task ToDtoAsync_ScalarProperties_MappedFromUnit()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();
            var unit = new CmUnit { Id = Guid.NewGuid(), Name = "Widget", Score = 42 };

            var dto = await mapper.ToDtoAsync(unit);

            Assert.Equal(unit.Id, dto.Id);
            Assert.Equal(unit.Name, dto.Name);
            Assert.Equal(unit.Score, dto.Score);
        }

        [Fact]
        public async Task ToDtoAsync_ElementContainer_MappedToList()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();
            var tag1 = new CmTag { Id = Guid.NewGuid(), Label = "a" };
            var tag2 = new CmTag { Id = Guid.NewGuid(), Label = "b" };
            var unit = new CmUnit();
            unit.Tags.AddToAnnounced(tag1);
            unit.Tags.AddToAnnounced(tag2);

            var dto = await mapper.ToDtoAsync(unit);

            Assert.Equal(2, dto.Tags.Count);
            Assert.Contains(tag1, dto.Tags);
            Assert.Contains(tag2, dto.Tags);
        }

        [Fact]
        public async Task ToDtoAsync_EmptyContainer_MapsToEmptyList()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();

            var dto = await mapper.ToDtoAsync(new CmUnit());

            Assert.Empty(dto.Tags);
        }

        // ── ToUnitAsync ────────────────────────────────────────────────────────────

        [Fact]
        public async Task ToUnitAsync_ScalarProperties_MappedToUnit()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();
            var dto = new CmDto { Id = Guid.NewGuid(), Name = "Widget", Score = 7 };

            var unit = await mapper.ToUnitAsync(dto);

            Assert.Equal(dto.Id, unit.Id);
            Assert.Equal(dto.Name, unit.Name);
            Assert.Equal(dto.Score, unit.Score);
        }

        [Fact]
        public async Task ToUnitAsync_ElementList_AddedToContainerAnnounced()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();
            var tag1 = new CmTag { Id = Guid.NewGuid(), Label = "x" };
            var tag2 = new CmTag { Id = Guid.NewGuid(), Label = "y" };
            var dto = new CmDto { Tags = [tag1, tag2] };

            var unit = await mapper.ToUnitAsync(dto);

            Assert.Equal(2, unit.Tags.Announced.Count);
            Assert.Contains(tag1, unit.Tags.Announced);
            Assert.Contains(tag2, unit.Tags.Announced);
        }

        [Fact]
        public async Task ToUnitAsync_EmptyList_ContainerHasNoElements()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();

            var unit = await mapper.ToUnitAsync(new CmDto { Tags = [] });

            Assert.Empty(unit.Tags.Announced);
        }

        // ── Round-trip ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task ToUnitAsync_ThenToDtoAsync_PreservesAllValues()
        {
            var mapper = new DXConventionMapper<CmDto, CmUnit>();
            var tag = new CmTag { Id = Guid.NewGuid(), Label = "round" };
            var original = new CmDto
            {
                Id = Guid.NewGuid(),
                Name = "Widget",
                Score = 99,
                Tags = [tag]
            };

            var unit = await mapper.ToUnitAsync(original);
            var result = await mapper.ToDtoAsync(unit);

            Assert.Equal(original.Id, result.Id);
            Assert.Equal(original.Name, result.Name);
            Assert.Equal(original.Score, result.Score);
            Assert.Single(result.Tags);
            Assert.Same(tag, result.Tags[0]);
        }

        // ── Startup validation ─────────────────────────────────────────────────────

        [Fact]
        public void Validate_ValidMapping_DoesNotThrow()
        {
            var ex = Record.Exception(() => DXConventionMapper<CmDto, CmUnit>.Validate());
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_DtoPropertyWithNoUnitMatch_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(
                () => DXConventionMapper<CmDtoMissingProp, CmUnit>.Validate());
        }

        [Fact]
        public void Validate_DtoPropertyTypeMismatch_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(
                () => DXConventionMapper<CmDtoTypeMismatch, CmUnit>.Validate());
        }
    }
}
