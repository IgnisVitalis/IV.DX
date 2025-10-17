using IV.DX.Application;
using IV.DX.Application.Handlers;
using IV.DX.Hosting;
using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace IV.DX.Shared.IntTests
{
    public abstract class IntTestController : IDisposable
    {
        protected Action _finalizationAction;

        private readonly IServiceScope _scope;
        protected IServiceProvider ServiceProvider => _scope.ServiceProvider;

        protected ITestOutputHelper Output;  

        public IntTestController(DXTestFixtureBase fx, ITestOutputHelper output)
        {
            this.Output = output;
            _scope = fx.Root.CreateScope();

            _scope.ServiceProvider.GetRequiredService<IDXStructureCache>().WarmUpAsync().Wait();
        }

        public void Dispose()
        {
            if (this._finalizationAction != null)
            {
                this._finalizationAction.Invoke();
            }

            
            //_scope.Dispose();
        }

        protected void RunActionSafety(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {

            }
        }

        protected async Task EstimatePerformanceAsync(Action action, string message)
        {
            if (action == null)
                return;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            action.Invoke();

            sw.Stop();

            string result = $"{message} : {sw.ElapsedMilliseconds} ms; {sw.ElapsedTicks} ticks;\n";

            Output.WriteLine(result);

            File.AppendAllText("Performance.txt", result);
        }
    }
}