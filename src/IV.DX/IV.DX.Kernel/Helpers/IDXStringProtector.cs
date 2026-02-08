namespace IV.DX.Kernel.Helpers
{
    public interface IDXStringProtector
    {
        string Protect(string plaintext);
        string Unprotect(string protectedValue);
        bool TryUnprotect(string protectedValue, out string plaintext);
    }
}

