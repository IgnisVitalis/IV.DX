using IV.DX.Shared.IntTests;
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
}