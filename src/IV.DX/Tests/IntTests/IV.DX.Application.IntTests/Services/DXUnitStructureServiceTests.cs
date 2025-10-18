using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXUnitStructureServiceTests : IntTestController
    {
        IDXUnitStructureService _service;

        public DXUnitStructureServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitStructureService>();
        }

        [Fact]
        public async Task GetAsync_Using_Ok()
        {
            // Init
            var dxUnitName = "DXUnitDefinitionUnit";

            // Action
            var dxUnitStructureDefinition = await this._service.GetAsync(dxUnitName);

            // Assert
        }
    }
}
