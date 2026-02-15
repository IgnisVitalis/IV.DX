using IV.DX.Persistence.Contracts.Abstractions;
using System.Threading;

namespace IV.DX.Hosting
{
    internal sealed class DXExecutionContextAccessor : IDXExecutionContextAccessor
    {
        private readonly AsyncLocal<DXExecutionContext?> _current = new();

        public DXExecutionContext? Current => _current.Value;

        public IDisposable BeginScope(DXExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var previous = _current.Value;
            _current.Value = context;

            return new Scope(() => _current.Value = previous);
        }

        private sealed class Scope(Action onDispose) : IDisposable
        {
            private readonly Action _onDispose = onDispose;
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _onDispose();
            }
        }
    }
}

