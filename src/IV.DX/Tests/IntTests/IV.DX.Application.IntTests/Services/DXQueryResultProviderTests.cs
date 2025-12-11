using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{

    [Collection("DX:one-time")]
    public class DXQueryResultProviderTests : IntTestController
    {
        IDXQueryResultProvider _dxQueryResultProvider;

        public DXQueryResultProviderTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._dxQueryResultProvider = base.ServiceProvider.GetRequiredService<IDXQueryResultProvider>();
        }

        [Fact]
        public async Task GetItemsAsync_UsingEnumName_Ok()
        {
            // Init
            var dxQueryID = new Guid("664a996a-bf83-46f9-aebb-c55f89deb6eb");

            // Action
            var result = await _dxQueryResultProvider.GetAsync(dxQueryID);

            // Assert
        }
    }
}