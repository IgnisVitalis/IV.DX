using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXEnumDataServiceTests : IntTestController
    {
        IDXEnumDataService _dxEnumDataService;

        public DXEnumDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._dxEnumDataService = base.ServiceProvider.GetRequiredService<IDXEnumDataService>();
        }

        [Fact]
        public async Task GetItemsAsync_UsingEnumName_Ok()
        {
            // Init
            var enumName = "DXObjectKindEnum";

            // Action
            var enumValues = await _dxEnumDataService.GetItemsAsync(enumName);

            // Assert
            Assert.NotNull(enumValues);

            Assert.Equal(3, enumValues.Count());
        }
    }
}
