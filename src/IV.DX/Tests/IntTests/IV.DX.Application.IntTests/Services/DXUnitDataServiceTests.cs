using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXUnitDataServiceTests : IntTestController
    {
        IDXUnitDataService _service;

        public DXUnitDataServiceTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
        }

        [Fact]
        public async Task GetItemsAsync_UsingFilterForNonExistingItems_EmptyEnumerable()
        {
            // Init
            string typeName = "DXElementDefinitionUnit";
            string filter = "DXObjectDefinitionMainElement.Kind = 999888777";

            // Action
            var items = await this._service.GetItemsAsync(typeName, filter);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
        }
    }
}