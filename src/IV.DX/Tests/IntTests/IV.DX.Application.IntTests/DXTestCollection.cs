using IV.DX.Hosting;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
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
}