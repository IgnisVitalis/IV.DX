using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{

    [Collection("DX:one-time")]
    public class DXStructureCacheTests : IntTestController
    {
        IDXStructureCache _dxStructureCache;

        public DXStructureCacheTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dxStructureCache = this.ServiceProvider.GetRequiredService<IDXStructureCache>();
        }

        [Theory]
        [InlineData("DXObjectDefinitionUnit", 3)]
        public void GetDXRelations_UsingName_Ok(string enumTypeName, int expectedAmount)
        {
            // Action
            var enums = this._dxStructureCache.GetDXRelations(enumTypeName);

            // Assert
            Assert.Equal(enums.Count(), expectedAmount);
        }
    }
}
