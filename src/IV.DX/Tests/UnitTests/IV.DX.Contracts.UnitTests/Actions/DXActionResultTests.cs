using IV.DX.Application.Contracts.Actions;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Actions
{
    public class DXActionResultTests
    {
        [Fact]
        public void Ok_CreatesSuccessResult()
        {
            var result = DXActionResult.Ok();

            Assert.True(result.IsSuccess);
            Assert.Null(result.Message);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Ok_WithMessage_SetsMessage()
        {
            var result = DXActionResult.Ok("Done");

            Assert.True(result.IsSuccess);
            Assert.Equal("Done", result.Message);
        }

        [Fact]
        public void Fail_CreatesFailureResult()
        {
            var result = DXActionResult.Fail("Something went wrong");

            Assert.False(result.IsSuccess);
            Assert.Equal("Something went wrong", result.Error);
            Assert.Null(result.Message);
        }

        [Fact]
        public void Output_IsAlwaysAvailable()
        {
            var result = DXActionResult.Ok();

            Assert.NotNull(result.Output);
        }

        [Fact]
        public void Output_CanSetAndGetValues()
        {
            var result = DXActionResult.Ok();
            result.Output.Set("Key", "Value");

            Assert.Equal("Value", result.Output.Get<string>("Key"));
        }
    }
}
