using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IV.DX.Hosting.UnitTests.Mappers
{
    public sealed class DXMapperServiceCollectionExtensionsTests
    {
        // ── Fixtures ───────────────────────────────────────────────────────────────

        [DXUnit("SmUnit")]
        private sealed class SmUnit : DXUnit { }

        private sealed class SmDto
        {
            public Guid Id { get; set; }
        }

        private sealed class SmDtoBad
        {
            public Guid Id { get; set; }
            public string Ghost { get; set; } = string.Empty;  // no property in SmUnit
        }

        private sealed class SmMapper : DXUnitMapper<SmDto, SmDto, SmUnit>
        {
            public override Task<SmDto> ToDtoAsync(SmUnit unit, CancellationToken ct = default)
                => Task.FromResult(new SmDto { Id = unit.Id });

            public override Task<SmUnit> ToUnitAsync(SmDto dto, CancellationToken ct = default)
                => Task.FromResult(new SmUnit { Id = dto.Id });
        }

        private sealed class SmReadMapper : DXUnitReadMapper<SmDto, SmUnit>
        {
            public override Task<SmDto> ToDtoAsync(SmUnit unit, CancellationToken ct = default)
                => Task.FromResult(new SmDto { Id = unit.Id });
        }

        private sealed class SmWriteMapper : DXUnitWriteMapper<SmDto, SmUnit>
        {
            public override Task<SmUnit> ToUnitAsync(SmDto dto, CancellationToken ct = default)
                => Task.FromResult(new SmUnit { Id = dto.Id });
        }

        private sealed class NotAMapper { }

        // ── AddDXUnitMapper<TMapper> ───────────────────────────────────────────────

        [Fact]
        public void AddDXUnitMapper_CustomMapper_RegistersIDXUnitDtoService()
        {
            var services = new ServiceCollection();
            services.AddDXUnitMapper<SmMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXUnitDtoService<SmDto, SmDto>));
        }

        [Fact]
        public void AddDXUnitMapper_CustomMapper_RegistersMapper()
        {
            var services = new ServiceCollection();
            services.AddDXUnitMapper<SmMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(SmMapper));
        }

        [Fact]
        public void AddDXUnitMapper_TypeNotDerivedFromDXUnitMapper_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXUnitMapper<NotAMapper>());
        }

        // ── AddDXUnitMapper<TDto, TUnit> ───────────────────────────────────────────

        [Fact]
        public void AddDXUnitMapper_ConventionMapper_ValidTypes_RegistersIDXUnitDtoService()
        {
            var services = new ServiceCollection();
            services.AddDXUnitMapper<SmDto, SmUnit>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXUnitDtoService<SmDto, SmDto>));
        }

        // ── AddDXUnitReadMapper<TMapper> ───────────────────────────────────────────

        [Fact]
        public void AddDXUnitReadMapper_RegistersIDXUnitQueryService()
        {
            var services = new ServiceCollection();
            services.AddDXUnitReadMapper<SmReadMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXUnitQueryService<SmDto>));
        }

        [Fact]
        public void AddDXUnitReadMapper_RegistersMapper()
        {
            var services = new ServiceCollection();
            services.AddDXUnitReadMapper<SmReadMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(SmReadMapper));
        }

        [Fact]
        public void AddDXUnitReadMapper_TypeNotDerivedFromDXUnitReadMapper_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXUnitReadMapper<NotAMapper>());
        }

        // ── AddDXUnitWriteMapper<TMapper> ──────────────────────────────────────────

        [Fact]
        public void AddDXUnitWriteMapper_RegistersIDXUnitCommandService()
        {
            var services = new ServiceCollection();
            services.AddDXUnitWriteMapper<SmWriteMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXUnitCommandService<SmDto>));
        }

        [Fact]
        public void AddDXUnitWriteMapper_RegistersMapper()
        {
            var services = new ServiceCollection();
            services.AddDXUnitWriteMapper<SmWriteMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(SmWriteMapper));
        }

        [Fact]
        public void AddDXUnitWriteMapper_TypeNotDerivedFromDXUnitWriteMapper_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXUnitWriteMapper<NotAMapper>());
        }

        [Fact]
        public void AddDXUnitMapper_ConventionMapper_InvalidDto_ThrowsAtRegistration()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXUnitMapper<SmDtoBad, SmUnit>());
        }
    }
}
