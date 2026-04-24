using Xunit;

namespace IV.DX.Hosting.UnitTests
{
    /// <summary>
    /// Disables parallelism between test classes that share DXConfiguredEncryptionKeyProvider.StateFilePath.
    /// </summary>
    [CollectionDefinition("EncryptionState", DisableParallelization = true)]
    public sealed class EncryptionStateCollection { }
}
