using IV.DX.Application.Actions;
using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Actions
{
    public class DXActionExecutorTests
    {
        private readonly IDXActionExecutor _executor;

        public DXActionExecutorTests()
        {
            var registry = new DXActionRegistry();
            registry.Register(typeof(SimpleAction));
            registry.Register(typeof(ActionWithDependency));
            registry.Register(typeof(ActionWithInOutParameters));
            registry.Register(typeof(FailingAction));

            var services = new ServiceCollection();
            services.AddSingleton<IDXActionRegistry>(registry);
            services.AddScoped<IDXActionExecutor, DXActionExecutor>();
            services.AddSingleton<IGreetingService, GreetingService>();
            services.AddScoped<SimpleAction>();
            services.AddScoped<ActionWithDependency>();
            services.AddScoped<ActionWithInOutParameters>();
            services.AddScoped<FailingAction>();

            var provider = services.BuildServiceProvider();
            _executor = provider.GetRequiredService<IDXActionExecutor>();
        }

        [Fact]
        public async Task ExecuteAsync_SimpleAction_ReturnsSuccess()
        {
            var result = await _executor.ExecuteAsync("Test", "Simple");

            Assert.True(result.IsSuccess);
            Assert.Equal("Simple executed.", result.Message);
        }

        [Fact]
        public async Task ExecuteAsync_UnknownAction_ReturnsFail()
        {
            var result = await _executor.ExecuteAsync("Unknown", "Unknown");

            Assert.False(result.IsSuccess);
            Assert.Contains("not registered", result.Error);
        }

        [Fact]
        public async Task ExecuteAsync_WithInputParameter_ReturnsOutput()
        {
            var parameters = new DXActionParameters()
                .Set("Name", "World");

            var result = await _executor.ExecuteAsync("Test", "Simple", parameters);

            Assert.True(result.IsSuccess);
            Assert.Equal("Hello, World!", result.Output.Get<string>("Greeting"));
        }

        [Fact]
        public async Task ExecuteAsync_WithoutInputParameter_UsesDefault()
        {
            var result = await _executor.ExecuteAsync("Test", "Simple");

            Assert.True(result.Output.ContainsKey("Greeting"));
            Assert.Equal("Hello, DX!", result.Output.Get<string>("Greeting"));
        }

        [Fact]
        public async Task ExecuteAsync_Counter_IncrementedInOutput()
        {
            var parameters = new DXActionParameters()
                .Set("Counter", 10);

            var result = await _executor.ExecuteAsync("Test", "InOut", parameters);

            Assert.True(result.IsSuccess);
            Assert.Equal(11, result.Output.Get<int>("Counter"));
        }

        [Fact]
        public async Task ExecuteAsync_OutputOnly_NotAffectedByInput()
        {
            var result = await _executor.ExecuteAsync("Test", "InOut");

            Assert.Equal("generated", result.Output.Get<string>("OutputOnly"));
        }

        [Fact]
        public async Task ExecuteAsync_GuidParameter_ParsedFromString()
        {
            var id = Guid.NewGuid();
            var parameters = new DXActionParameters()
                .Set("Id", id.ToString());

            var result = await _executor.ExecuteAsync("Test", "InOut", parameters);

            Assert.Equal(id, result.Output.Get<Guid>("Id"));
        }

        [Fact]
        public async Task ExecuteAsync_WithDIDependency_InjectsService()
        {
            var parameters = new DXActionParameters()
                .Set("Name", "DX");

            var result = await _executor.ExecuteAsync("Test", "WithDependency", parameters);

            Assert.True(result.IsSuccess);
            Assert.Equal("Greetings, DX!", result.Output.Get<string>("Result"));
        }

        [Fact]
        public async Task ExecuteAsync_FailingAction_ReturnsFailAndNoOutput()
        {
            var result = await _executor.ExecuteAsync("Test", "Failing");

            Assert.False(result.IsSuccess);
            Assert.Equal("Something broke", result.Error);
            Assert.False(result.Output.ContainsKey("Value"));
        }

        [Fact]
        public async Task ExecuteAsync_NullParameters_ExecutesWithDefaults()
        {
            var result = await _executor.ExecuteAsync("Test", "Simple", null);

            Assert.True(result.IsSuccess);
            Assert.Equal("Hello, DX!", result.Output.Get<string>("Greeting"));
        }

        #region Test action classes and services

        public interface IGreetingService
        {
            string Greet(string name);
        }

        public class GreetingService : IGreetingService
        {
            public string Greet(string name) => $"Greetings, {name}!";
        }

        [DXAction("Test", "Simple")]
        [DXInParameter("Name", DXActionParameterTypeEnum.String)]
        [DXOutParameter("Greeting", DXActionParameterTypeEnum.String, Required = true)]
        public class SimpleAction : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
            {
                var name = input.Get<string>("Name") ?? "DX";
                var result = DXActionResult.Ok("Simple executed.");
                result.Output.Set("Greeting", $"Hello, {name}!");
                return Task.FromResult(result);
            }
        }

        [DXAction("Test", "InOut")]
        [DXInParameter("Counter", DXActionParameterTypeEnum.Int)]
        [DXInParameter("Id", DXActionParameterTypeEnum.GUID)]
        [DXOutParameter("Counter", DXActionParameterTypeEnum.Int, Required = true)]
        [DXOutParameter("OutputOnly", DXActionParameterTypeEnum.String, Required = true)]
        [DXOutParameter("Id", DXActionParameterTypeEnum.GUID, Required = true)]
        public class ActionWithInOutParameters : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
            {
                var counter = input.Get<int>("Counter");
                var id = input.Get<Guid>("Id");

                var result = DXActionResult.Ok();
                result.Output.Set("Counter", counter + 1);
                result.Output.Set("OutputOnly", "generated");
                result.Output.Set("Id", id);
                return Task.FromResult(result);
            }
        }

        [DXAction("Test", "WithDependency")]
        [DXInParameter("Name", DXActionParameterTypeEnum.String)]
        [DXOutParameter("Result", DXActionParameterTypeEnum.String, Required = true)]
        public class ActionWithDependency : DXActionBase
        {
            private readonly IGreetingService _greetingService;

            public ActionWithDependency(IGreetingService greetingService)
            {
                _greetingService = greetingService;
            }

            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
            {
                var name = input.Get<string>("Name") ?? string.Empty;
                var result = DXActionResult.Ok();
                result.Output.Set("Result", _greetingService.Greet(name));
                return Task.FromResult(result);
            }
        }

        [DXAction("Test", "Failing")]
        public class FailingAction : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
            {
                return Task.FromResult(DXActionResult.Fail("Something broke"));
            }
        }

        #endregion
    }
}
