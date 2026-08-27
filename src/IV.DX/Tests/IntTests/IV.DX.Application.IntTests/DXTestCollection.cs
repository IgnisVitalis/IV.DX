using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Hosting;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Application.IntTests
{
    [CollectionDefinition("DX:one-time", DisableParallelization = true)]
    public class DXTestCollection : ICollectionFixture<DXTestFixture>
    {

    }
    public class DXTestFixture : DXTestFixtureBase
    {
        protected override string Database => "IV.DX.Application.IntTests";

        protected override void ConfigureAdditionalServices(IServiceCollection services)
        {
            // Gives the ownership tests an IDXUnitQueryService to exercise GetOwnedAsync through,
            // over the same unit their ownership rows point at.
            services.AddDXUnitReadMapper<TUserOwnedMapper>();
        }
    }

    [CollectionDefinition("DX:dto-service", DisableParallelization = true)]
    public class DXDtoServiceTestCollection : ICollectionFixture<DXDtoServiceTestFixture> { }

    public class DXDtoServiceTestFixture : DXTestFixtureBase
    {
        protected override string Database => "IV.DX.DtoService.IntTests";

        protected override void ConfigureAdditionalServices(IServiceCollection services)
        {
            services.AddDXUnitMapper<TBookDto, TBookUnit>();
        }
    }

    public sealed class TBookDto
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public sealed class TUserOwnedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class TUserOwnedMapper : DXUnitReadMapper<TUserOwnedDto, TUserUnit>
    {
        public override Task<TUserOwnedDto> ToDtoAsync(TUserUnit unit, CancellationToken ct = default)
            => Task.FromResult(new TUserOwnedDto
            {
                Id = unit.Id,
                Name = unit.TUserMainElement?.Name ?? string.Empty
            });
    }
}
