using IV.DX.Application.Contracts.Handlers;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Hosting;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Pipeline
{
    public class DXHandlerProviderScopeTests
    {
        [Fact]
        public void GetBeforeGetHandlers_UsingScopedDependency_ResolvesCurrentScopeHandler()
        {
            var services = new ServiceCollection();
            services.AddDXPipeline();
            services.AddScoped<ScopedMarker>();
            services.AddTransient<ScopedBeforeGetHandler>();

            using var root = services.BuildServiceProvider();

            Guid firstScopeMarkerId;

            using (var firstScope = root.CreateScope())
            {
                var provider = firstScope.ServiceProvider.GetRequiredService<IDXUnitGetHandlerProvider>();
                var handler = firstScope.ServiceProvider.GetRequiredService<ScopedBeforeGetHandler>();

                provider.Register<TestUnit>(handler);

                firstScopeMarkerId = provider.GetBeforeGetHandlers<TestUnit>()
                    .OfType<ScopedBeforeGetHandler>()
                    .Single()
                    .MarkerId;
            }

            using (var secondScope = root.CreateScope())
            {
                var provider = secondScope.ServiceProvider.GetRequiredService<IDXUnitGetHandlerProvider>();

                var secondScopeMarkerId = provider.GetBeforeGetHandlers<TestUnit>()
                    .OfType<ScopedBeforeGetHandler>()
                    .Single()
                    .MarkerId;

                Assert.NotEqual(firstScopeMarkerId, secondScopeMarkerId);
            }
        }

        [Fact]
        public void Register_UsingSameHandlerTypeMultipleTimes_DoesNotDuplicateHandlers()
        {
            var services = new ServiceCollection();
            services.AddDXPipeline();
            services.AddScoped<ScopedMarker>();
            services.AddTransient<ScopedBeforeGetHandler>();

            using var root = services.BuildServiceProvider();
            using var scope = root.CreateScope();

            var provider = scope.ServiceProvider.GetRequiredService<IDXUnitGetHandlerProvider>();

            provider.Register<TestUnit>(scope.ServiceProvider.GetRequiredService<ScopedBeforeGetHandler>());
            provider.Register<TestUnit>(scope.ServiceProvider.GetRequiredService<ScopedBeforeGetHandler>());

            var handlers = provider.GetBeforeGetHandlers<TestUnit>().ToArray();

            Assert.Single(handlers);
        }

        [Fact]
        public void GetBeforeInsertHandlers_UsingScopedDependency_ResolvesCurrentScopeHandler()
        {
            var services = new ServiceCollection();
            services.AddDXPipeline();
            services.AddScoped<ScopedMarker>();
            services.AddTransient<ScopedBeforeInsertHandler>();

            using var root = services.BuildServiceProvider();

            Guid firstScopeMarkerId;

            using (var firstScope = root.CreateScope())
            {
                var provider = firstScope.ServiceProvider.GetRequiredService<IDXUnitInsertHandlerProvider>();
                var handler = firstScope.ServiceProvider.GetRequiredService<ScopedBeforeInsertHandler>();

                provider.Register<TestUnit>(handler);

                firstScopeMarkerId = provider.GetBeforeInsertHandlers<TestUnit>()
                    .OfType<ScopedBeforeInsertHandler>()
                    .Single()
                    .MarkerId;
            }

            using (var secondScope = root.CreateScope())
            {
                var provider = secondScope.ServiceProvider.GetRequiredService<IDXUnitInsertHandlerProvider>();

                var secondScopeMarkerId = provider.GetBeforeInsertHandlers<TestUnit>()
                    .OfType<ScopedBeforeInsertHandler>()
                    .Single()
                    .MarkerId;

                Assert.NotEqual(firstScopeMarkerId, secondScopeMarkerId);
            }
        }

        private sealed class ScopedMarker
        {
            public Guid Id { get; } = Guid.NewGuid();
        }

        private sealed class TestUnit : DXUnit
        {
        }

        private sealed class ScopedBeforeGetHandler : IDXBeforeGetHandler<TestUnit>
        {
            private readonly ScopedMarker _marker;

            public ScopedBeforeGetHandler(ScopedMarker marker)
            {
                _marker = marker;
            }

            public int BeforeOrder => 0;

            public Guid MarkerId => _marker.Id;

            public Task<DXResult<Guid>> BeforeGetAsync(Guid id, DXHandlerBaseContext ctx, CancellationToken ct)
            {
                return Task.FromResult(DXResult<Guid>.Ok(id));
            }
        }

        private sealed class ScopedBeforeInsertHandler : IDXBeforeInsertHandler<TestUnit>
        {
            private readonly ScopedMarker _marker;

            public ScopedBeforeInsertHandler(ScopedMarker marker)
            {
                _marker = marker;
            }

            public int BeforeOrder => 0;

            public Guid MarkerId => _marker.Id;

            public Task<DXResult<TestUnit>> BeforeInsertAsync(TestUnit dxUnit, DXHandlerBaseContext ctx, CancellationToken ct)
            {
                return Task.FromResult(DXResult<TestUnit>.Ok(dxUnit));
            }
        }
    }
}
