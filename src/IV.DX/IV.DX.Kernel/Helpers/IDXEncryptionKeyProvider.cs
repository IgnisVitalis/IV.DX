namespace IV.DX.Kernel.Helpers
{
    public interface IDXEncryptionKeyProvider
    {
        DXEncryptionKey GetCurrent();
        bool TryGet(string keyId, out DXEncryptionKey key);
    }
}

