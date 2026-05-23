using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXUnitWithRelationTests : IntTestController
    {
        IDXUnitDataService _service;

        public DXUnitWithRelationTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
        }              

        // [Fact]
        // public async Task T()
        // {
        //     // Init
        //     var id = new Guid("018fa545-8876-7a5a-a72c-3fdaf537245d");

        //     // Action

        //     var dxObject = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id);

        //     // Assert
        // }


        [DXUnit("DXUnitDefinitionUnit")]
        private class DXUnitDefinitionUnit : DXUnit
        {

            [DXColumn("DXUnitDefinitionUnitId", "U2U(DXUnitDefinitionUnit).Id")]
            public Guid DXUnitDefinitionUnitId { get; set; }
        }
    }
}
