using IV.DX.Application.Actions;
using IV.DX.Application.Contracts.Actions;
using IV.DX.Kernel.Attributes;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Actions
{
    public class DXActionScannerTests
    {
        [Fact]
        public void FindActionTypes_FindsPingAction_InApplicationAssembly()
        {
            var types = DXActionScanner.FindActionTypes(new[] { typeof(DXPingAction).Assembly });

            Assert.Contains(types, t => t == typeof(DXPingAction));
        }

        [Fact]
        public void FindActionTypes_IgnoresAbstractClasses()
        {
            var types = DXActionScanner.FindActionTypes(new[] { typeof(DXActionScannerTests).Assembly });

            Assert.DoesNotContain(types, t => t == typeof(AbstractTestAction));
        }

        [Fact]
        public void FindActionTypes_IgnoresClassesWithoutAttribute()
        {
            var types = DXActionScanner.FindActionTypes(new[] { typeof(DXActionScannerTests).Assembly });

            Assert.DoesNotContain(types, t => t == typeof(ActionWithoutAttr));
        }

        [Fact]
        public void FindActionTypes_FindsConcreteAnnotatedActions()
        {
            var types = DXActionScanner.FindActionTypes(new[] { typeof(DXActionScannerTests).Assembly }).ToList();

            Assert.Contains(types, t => t == typeof(DiscoverableTestAction));
        }

        #region Test action classes

        [DXAction("Scanner", "Abstract")]
        public abstract class AbstractTestAction : DXActionBase { }

        public class ActionWithoutAttr : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
                => Task.FromResult(DXActionResult.Ok());
        }

        [DXAction("Scanner", "Discoverable")]
        public class DiscoverableTestAction : DXActionBase
        {
            public override Task<DXActionResult> ExecuteAsync(DXActionParameters input, CancellationToken ct)
                => Task.FromResult(DXActionResult.Ok());
        }

        #endregion
    }
}
