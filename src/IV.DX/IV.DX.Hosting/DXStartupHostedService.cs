using Microsoft.Extensions.Hosting;

namespace IV.DX.Hosting
{
    internal sealed class DXStartupHostedService : IHostedService
    {
        private readonly IServiceProvider _root;

        public DXStartupHostedService(IServiceProvider root)
        {
            _root = root;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => _root.StartDXAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
