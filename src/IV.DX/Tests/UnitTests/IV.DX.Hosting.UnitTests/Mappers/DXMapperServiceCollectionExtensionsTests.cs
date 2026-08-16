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

        [DXElement("SmElement")]
        private sealed class SmElement : DXElement { }

        private sealed class SmElementDto
        {
            public Guid Id { get; set; }
        }

        private sealed class SmElementSummaryDto
        {
            public Guid Id { get; set; }
        }

        private sealed class SmElementMapper : DXElementMapper<SmElementDto, SmElementDto, SmElement, SmUnit>
        {
            public override Task<SmElementDto> ToDtoAsync(SmElement element, CancellationToken ct = default)
                => Task.FromResult(new SmElementDto { Id = element.Id });

            public override Task<SmElement> ToElementAsync(SmElementDto dto, CancellationToken ct = default)
                => Task.FromResult(new SmElement { Id = dto.Id });
        }

        private sealed class SmElementReadMapper : DXElementReadMapper<SmElementDto, SmElement, SmUnit>
        {
            public override Task<SmElementDto> ToDtoAsync(SmElement element, CancellationToken ct = default)
                => Task.FromResult(new SmElementDto { Id = element.Id });
        }

        private sealed class SmElementSummaryReadMapper : DXElementReadMapper<SmElementSummaryDto, SmElement, SmUnit>
        {
            public override Task<SmElementSummaryDto> ToDtoAsync(SmElement element, CancellationToken ct = default)
                => Task.FromResult(new SmElementSummaryDto { Id = element.Id });
        }

        private sealed class SmElementWriteMapper : DXElementWriteMapper<SmElementDto, SmElement, SmUnit>
        {
            public override Task<SmElement> ToElementAsync(SmElementDto dto, CancellationToken ct = default)
                => Task.FromResult(new SmElement { Id = dto.Id });
        }

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

        // ── Element mappers ────────────────────────────────────────────────────────

        [Fact]
        public void AddDXElementMapper_CustomMapper_RegistersIDXElementDtoService()
        {
            var services = new ServiceCollection();
            services.AddDXElementMapper<SmElementMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXElementDtoService<SmElementDto, SmElementDto>));
        }

        [Fact]
        public void AddDXElementMapper_CustomMapper_RegistersMapper()
        {
            var services = new ServiceCollection();
            services.AddDXElementMapper<SmElementMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(SmElementMapper));
        }

        [Fact]
        public void AddDXElementMapper_TypeNotDerivedFromDXElementMapper_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXElementMapper<NotAMapper>());
        }

        [Fact]
        public void AddDXElementReadMapper_CustomMapper_RegistersIDXElementQueryService()
        {
            var services = new ServiceCollection();
            services.AddDXElementReadMapper<SmElementReadMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXElementQueryService<SmElementDto>));
        }

        [Fact]
        public void AddDXElementReadMapper_TypeNotDerivedFromDXElementReadMapper_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXElementReadMapper<NotAMapper>());
        }

        [Fact]
        public void AddDXElementWriteMapper_CustomMapper_RegistersIDXElementCommandService()
        {
            var services = new ServiceCollection();
            services.AddDXElementWriteMapper<SmElementWriteMapper>();

            Assert.Contains(services,
                d => d.ServiceType == typeof(IDXElementCommandService<SmElementDto>));
        }

        [Fact]
        public void AddDXElementWriteMapper_TypeNotDerivedFromDXElementWriteMapper_ThrowsInvalidOperation()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(
                () => services.AddDXElementWriteMapper<NotAMapper>());
        }

        [Fact]
        public void AddDXElementReadMapper_TwoMappersOverOneElement_BothServicesRegistered()
        {
            var services = new ServiceCollection();

            services.AddDXElementReadMapper<SmElementReadMapper>();
            services.AddDXElementReadMapper<SmElementSummaryReadMapper>();

            // Services are keyed by response DTO, not by element, so several contracts over one
            // element type coexist - the same property the unit mappers have.
            Assert.Contains(services, d => d.ServiceType == typeof(IDXElementQueryService<SmElementDto>));
            Assert.Contains(services, d => d.ServiceType == typeof(IDXElementQueryService<SmElementSummaryDto>));
        }
    }
}
