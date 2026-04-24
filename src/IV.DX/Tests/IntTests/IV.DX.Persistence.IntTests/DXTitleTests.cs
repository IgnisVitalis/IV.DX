using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DXTitleTests : IntTestController
    {
        private readonly IDXUnitCoreRepository _coreRepo;
        private readonly IDXUnitGenericRepository _genericRepo;

        private static readonly Guid VictorId = new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3");
        private const string VictorName = "Victor";

        public DXTitleTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            _coreRepo = ServiceProvider.GetRequiredService<IDXUnitCoreRepository>();
            _genericRepo = ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        public void GetItemRecord_DXTitle_ReturnsExpectedTitle()
        {
            var block = _coreRepo.GetItemRecord("TUserUnit", VictorId);

            Assert.NotNull(block);
            var record = Assert.Single(block.Data.Items);
            Assert.Equal(VictorName, record.DXTitle);
        }

        [Fact]
        public void GetItemRecord_DXTitle_PresentInJObject()
        {
            var block = _coreRepo.GetItemRecord("TUserUnit", VictorId);

            Assert.NotNull(block);
            var record = Assert.Single(block.Data.Items);

            var jObject = JObject.FromObject(record);

            Assert.True(jObject.ContainsKey("DXTitle"), "DXTitle key must be present in JSON");
            Assert.Equal(VictorName, jObject["DXTitle"]!.ToString());
        }

        [Fact]
        public void GetDXUnit_DXTitle_ReturnsExpectedTitle()
        {
            var user = _genericRepo.GetDXUnit<TUserUnit>(VictorId);

            Assert.NotNull(user);
            Assert.Equal(VictorName, user.DXTitle);
        }
    }
}
