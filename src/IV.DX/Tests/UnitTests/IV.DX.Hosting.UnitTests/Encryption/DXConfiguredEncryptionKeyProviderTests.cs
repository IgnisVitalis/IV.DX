using IV.DX.Hosting;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Xunit;

namespace IV.DX.Hosting.UnitTests.Encryption
{
    /// <summary>
    /// Tests for DXConfiguredEncryptionKeyProvider.
    /// Uses [Collection] so tests sharing the static StateFilePath never run in parallel.
    /// </summary>
    [Collection("EncryptionState")]
    public sealed class DXConfiguredEncryptionKeyProviderTests : IDisposable
    {
        private readonly string _stateFile;

        public DXConfiguredEncryptionKeyProviderTests()
        {
            _stateFile = Path.Combine(Path.GetTempPath(), $"dx-prov-{Guid.NewGuid()}.json");
            DXConfiguredEncryptionKeyProvider.StateFilePath = _stateFile;
        }

        public void Dispose()
        {
            try { if (File.Exists(_stateFile)) File.Delete(_stateFile); }
            catch (IOException) { /* best-effort cleanup of temp file */ }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);

        private static string ToBase64(byte[] bytes) => Convert.ToBase64String(bytes);

        private static DXConfiguredEncryptionKeyProvider Provider(byte[] keyBytes)
        {
            var opts = Options.Create(new DXEncryptionOptions { Key = ToBase64(keyBytes) });
            return new DXConfiguredEncryptionKeyProvider(opts);
        }

        private static void WriteStateFile(string filePath, string keyBase64, string? keyId = null)
        {
            var state = new { Key = keyBase64, KeyId = keyId };
            File.WriteAllText(filePath, JsonSerializer.Serialize(state));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Key ID derivation
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void DeriveKeyId_SameBytes_ReturnsSameId()
        {
            var key = NewKey();
            Assert.Equal(
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(key),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(key));
        }

        [Fact]
        public void DeriveKeyId_DifferentBytes_ReturnsDifferentIds()
        {
            Assert.NotEqual(
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(NewKey()),
                DXConfiguredEncryptionKeyProvider.DeriveKeyId(NewKey()));
        }

        [Fact]
        public void DeriveKeyId_ReturnsEightCharacters()
        {
            var id = DXConfiguredEncryptionKeyProvider.DeriveKeyId(NewKey());
            Assert.Equal(8, id.Length);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Provider construction
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void GetCurrent_ReturnsKeyWithDerivedId()
        {
            var key = NewKey();
            var provider = Provider(key);
            var current = provider.GetCurrent();

            Assert.Equal(DXConfiguredEncryptionKeyProvider.DeriveKeyId(key), current.KeyId);
            Assert.Equal(key, current.KeyBytes);
        }

        [Fact]
        public void Constructor_ThrowsIfKeyNotConfigured()
        {
            var opts = Options.Create(new DXEncryptionOptions { Key = null });
            Assert.Throws<InvalidOperationException>(
                () => new DXConfiguredEncryptionKeyProvider(opts));
        }

        [Fact]
        public void TryGet_CurrentKeyId_Succeeds()
        {
            var key = NewKey();
            var provider = Provider(key);
            var keyId = provider.GetCurrent().KeyId;

            Assert.True(provider.TryGet(keyId, out var found));
            Assert.Equal(keyId, found.KeyId);
        }

        [Fact]
        public void TryGet_UnknownKeyId_ReturnsFalse()
        {
            var provider = Provider(NewKey());
            Assert.False(provider.TryGet("does-not-exist", out _));
        }

        // ──────────────────────────────────────────────────────────────────────
        // State file — no rotation (key matches)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void StateFileMatchesCurrentKey_OnlyOneKeyLoaded()
        {
            var key = NewKey();
            // State file already has the same key
            WriteStateFile(_stateFile, ToBase64(key));

            var provider = Provider(key);

            Assert.True(provider.TryGet(provider.GetCurrent().KeyId, out _));
            // No "previous" key loaded because keys are identical
            Assert.False(provider.TryGet("some-other-id", out _));
        }

        // ──────────────────────────────────────────────────────────────────────
        // State file — rotation (key differs)
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void StateFileHasDifferentKey_PreviousKeyLoadedUnderItsId()
        {
            var prevKey = NewKey();
            var prevKeyId = DXConfiguredEncryptionKeyProvider.DeriveKeyId(prevKey);
            WriteStateFile(_stateFile, ToBase64(prevKey), prevKeyId);

            var currentKey = NewKey();
            var provider = Provider(currentKey);

            // Previous key must be reachable under its original id
            Assert.True(provider.TryGet(prevKeyId, out var found));
            Assert.Equal(prevKey, found.KeyBytes);
        }

        [Fact]
        public void StateFileHasDifferentKey_CurrentKeyStillReachable()
        {
            WriteStateFile(_stateFile, ToBase64(NewKey()), "old-id");

            var currentKey = NewKey();
            var provider = Provider(currentKey);

            Assert.True(provider.TryGet(provider.GetCurrent().KeyId, out _));
        }

        [Fact]
        public void StateFileHasNoKeyId_FallsBackToDerivedId()
        {
            // Old state files written before KeyId was added to the schema
            var prevKey = NewKey();
            WriteStateFile(_stateFile, ToBase64(prevKey), keyId: null);

            var provider = Provider(NewKey());

            var derivedId = DXConfiguredEncryptionKeyProvider.DeriveKeyId(prevKey);
            Assert.True(provider.TryGet(derivedId, out _));
        }

        [Fact]
        public void MissingStateFile_ProviderConstructsSuccessfully()
        {
            // No state file — fresh install
            Assert.False(File.Exists(_stateFile));
            var provider = Provider(NewKey());
            Assert.NotNull(provider.GetCurrent());
        }

        // ──────────────────────────────────────────────────────────────────────
        // ReadState / WriteState helpers
        // ──────────────────────────────────────────────────────────────────────

        [Fact]
        public void WriteState_ThenReadState_RoundTrips()
        {
            DXConfiguredEncryptionKeyProvider.WriteState("base64key==", "my-key-id");
            var state = DXConfiguredEncryptionKeyProvider.ReadState();

            Assert.NotNull(state);
            Assert.Equal("base64key==", state!.Key);
            Assert.Equal("my-key-id", state.KeyId);
        }

        [Fact]
        public void ReadState_MissingFile_ReturnsNull()
        {
            Assert.Null(DXConfiguredEncryptionKeyProvider.ReadState());
        }

        [Fact]
        public void ReadState_CorruptFile_ReturnsNull()
        {
            File.WriteAllText(_stateFile, "not valid json {{{");
            Assert.Null(DXConfiguredEncryptionKeyProvider.ReadState());
        }

        [Fact]
        public void ReadState_EmptyKey_ReturnsNull()
        {
            File.WriteAllText(_stateFile, JsonSerializer.Serialize(new { Key = "", KeyId = "x" }));
            Assert.Null(DXConfiguredEncryptionKeyProvider.ReadState());
        }
    }
}
