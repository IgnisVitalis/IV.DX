using IV.DX.Application.Actions;
using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Actions
{
    public class DXActionRegistryTests
    {
        [Fact]
        public void Register_And_Resolve_ReturnsActionType()
        {
            var registry = new DXActionRegistry();
            registry.Register(typeof(TestAction));

            var result = registry.Resolve("TestModule", "TestKey");

            Assert.Equal(typeof(TestAction), result);
        }

        [Fact]
        public void Resolve_UnknownAction_ReturnsNull()
        {
            var registry = new DXActionRegistry();

            var result = registry.Resolve("Unknown", "Unknown");

            Assert.Null(result);
        }

        [Fact]
        public void Register_SameModuleKey_OverridesPrevious()
        {
            var registry = new DXActionRegistry();
            registry.Register(typeof(TestAction));
            registry.Register(typeof(TestActionOverride));

            var result = registry.Resolve("TestModule", "TestKey");

            Assert.Equal(typeof(TestActionOverride), result);
        }

        [Fact]
        public void Register_WithoutAttribute_Throws()
        {
            var registry = new DXActionRegistry();

            Assert.Throws<InvalidOperationException>(() =>
                registry.Register(typeof(ActionWithoutAttribute)));
        }

        [Fact]
        public void Register_NonDXActionBase_Throws()
        {
            var registry = new DXActionRegistry();

            Assert.Throws<InvalidOperationException>(() =>
                registry.Register(typeof(NotAnAction)));
        }

        #region Test action classes

        [DXAction("TestModule", "TestKey")]
        public class TestAction : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
                => Task.FromResult(DXActionResult.Ok());
        }

        [DXAction("TestModule", "TestKey")]
        public class TestActionOverride : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
                => Task.FromResult(DXActionResult.Ok("Override"));
        }

        public class ActionWithoutAttribute : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
                => Task.FromResult(DXActionResult.Ok());
        }

        [DXAction("Test", "NotAnAction")]
        public class NotAnAction
        {
        }

        #endregion
    }
}
