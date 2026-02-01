using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXElementDataServiceTests : IntTestController
    {
        IDXElementDataService _service;

        public DXElementDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXElementDataService>();
        }

        // [Fact]
        // public async Task GetItemsAsync_UsingDXFilterWithRelation_Ok()
        // {
        //     // Init
        //     var dxFilter = "R(EnumType).Name = 'DXColumnTypeEnum'";

        //     // Action
        //     var dxElements = await _service.GetItemsAsync<DXObjectEnumElement>("DXColumnTypeEnum", dxFilter);

        //     // Assert
        //     Assert.NotNull(dxElements);

        //     //Assert.Equal(3, dxElements.Count());
        // }
    }
}