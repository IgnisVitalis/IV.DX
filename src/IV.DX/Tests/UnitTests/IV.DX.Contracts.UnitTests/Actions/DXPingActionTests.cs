using IV.DX.Application.Actions;
using IV.DX.Application.Contracts.Actions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Actions
{
    public class DXPingActionTests
    {
        private readonly IDXActionExecutor _executor;

        public DXPingActionTests()
        {
            var registry = new DXActionRegistry();
            registry.Register(typeof(DXPingAction));

            var services = new ServiceCollection();
            services.AddSingleton<IDXActionRegistry>(registry);
            services.AddScoped<IDXActionExecutor, DXActionExecutor>();
            services.AddScoped<DXPingAction>();

            var provider = services.BuildServiceProvider();
            _executor = provider.GetRequiredService<IDXActionExecutor>();
        }

        [Fact]
        public async Task PingAction_ReturnsSuccess()
        {
            var result = await _executor.ExecuteAsync("IV.DX", "Ping",
                new DXActionParameters().Set("Message", "Hello"));

            Assert.True(result.IsSuccess);
            Assert.Equal("Ping executed successfully.", result.Message);
        }

        [Fact]
        public async Task PingAction_ReturnsPongResponse()
        {
            var result = await _executor.ExecuteAsync("IV.DX", "Ping",
                new DXActionParameters().Set("Message", "Hello"));

            Assert.Equal("Pong: Hello", result.Output.Get<string>("Response"));
        }

        [Fact]
        public async Task PingAction_ReturnsTimestamp()
        {
            var before = DateTime.UtcNow;

            var result = await _executor.ExecuteAsync("IV.DX", "Ping",
                new DXActionParameters().Set("Message", "Test"));

            var timestamp = result.Output.Get<DateTime>("Timestamp");
            Assert.True(timestamp >= before);
            Assert.True(timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public async Task PingAction_WithEmptyMessage_ReturnsPongEmpty()
        {
            var result = await _executor.ExecuteAsync("IV.DX", "Ping");

            Assert.Equal("Pong: ", result.Output.Get<string>("Response"));
        }
    }
}
