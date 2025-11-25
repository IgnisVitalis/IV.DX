using IV.DX.Persistence.Contracts.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
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

        protected async Task EstimatePerformanceAsync(Func<Task> action, string message)
        {
            if (action is null) return;

            var sw = Stopwatch.StartNew();
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                var err = $"{message} : ERROR: {ex.GetType().Name} - {ex.Message}";
                Output.WriteLine(err);
                File.AppendAllText("Performance.txt", err + Environment.NewLine);
                throw;
            }
            finally
            {
                sw.Stop();
                var result = $"{message} : {sw.ElapsedMilliseconds} ms; {sw.ElapsedTicks} ticks";
                Output.WriteLine(result);
                File.AppendAllText("Performance.txt", result + Environment.NewLine);
            }
        }

        protected async Task<T> EstimatePerformanceAsync<T>(Func<Task<T>> func, string message)
        {
            if (func is null) return default(T);

            var sw = Stopwatch.StartNew();
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                var err = $"{message} : ERROR: {ex.GetType().Name} - {ex.Message}";
                Output.WriteLine(err);
                File.AppendAllText("Performance.txt", err + Environment.NewLine);
                throw;
            }
            finally
            {
                sw.Stop();
                var result = $"{message} : {sw.ElapsedMilliseconds} ms; {sw.ElapsedTicks} ticks";
                Output.WriteLine(result);
                File.AppendAllText("Performance.txt", result + Environment.NewLine);
            }
        }
    }
}