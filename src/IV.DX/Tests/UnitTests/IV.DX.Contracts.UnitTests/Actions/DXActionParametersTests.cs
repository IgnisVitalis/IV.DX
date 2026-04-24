using IV.DX.Application.Contracts.Actions;
using System;
using Xunit;

namespace IV.DX.Contracts.UnitTests.Actions
{
    public class DXActionParametersTests
    {
        [Fact]
        public void Set_And_Get_ReturnsValue()
        {
            var parameters = new DXActionParameters();
            parameters.Set("Name", "Test");

            var result = parameters.Get<string>("Name");

            Assert.Equal("Test", result);
        }

        [Fact]
        public void Get_NonExistentKey_ReturnsDefault()
        {
            var parameters = new DXActionParameters();

            var result = parameters.Get<string>("Missing");

            Assert.Null(result);
        }

        [Fact]
        public void Get_IntValue_ReturnsCorrectType()
        {
            var parameters = new DXActionParameters();
            parameters.Set("Count", 42);

            var result = parameters.Get<int>("Count");

            Assert.Equal(42, result);
        }

        [Fact]
        public void Get_GuidFromString_ParsesCorrectly()
        {
            var expected = Guid.NewGuid();
            var parameters = new DXActionParameters();
            parameters.Set("Id", expected.ToString());

            var result = parameters.Get<Guid>("Id");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Get_NullValue_ReturnsDefault()
        {
            var parameters = new DXActionParameters();
            parameters.Set("Key", null);

            var result = parameters.Get<string>("Key");

            Assert.Null(result);
        }

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            var parameters = new DXActionParameters();
            parameters.Set("Key", "Value");

            Assert.True(parameters.ContainsKey("Key"));
        }

        [Fact]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            var parameters = new DXActionParameters();

            Assert.False(parameters.ContainsKey("Missing"));
        }

        [Fact]
        public void ContainsKey_IsCaseInsensitive()
        {
            var parameters = new DXActionParameters();
            parameters.Set("MyKey", "Value");

            Assert.True(parameters.ContainsKey("mykey"));
            Assert.True(parameters.ContainsKey("MYKEY"));
        }

        [Fact]
        public void Set_FluentChaining_Works()
        {
            var parameters = new DXActionParameters()
                .Set("A", 1)
                .Set("B", 2)
                .Set("C", 3);

            Assert.Equal(1, parameters.Get<int>("A"));
            Assert.Equal(2, parameters.Get<int>("B"));
            Assert.Equal(3, parameters.Get<int>("C"));
        }

        [Fact]
        public void ToDictionary_ReturnsAllEntries()
        {
            var parameters = new DXActionParameters()
                .Set("A", 1)
                .Set("B", "two");

            var dict = parameters.ToDictionary();

            Assert.Equal(2, dict.Count);
        }
    }
}
