using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.DataServiceTests
{
    [Collection("DX:one-time")]
    public class DXDerivedUnitTypeTests : IntTestController
    {
        private static readonly Guid TDeviceUnitDefinitionId  = new Guid("7f8501fd-7f16-42f8-8a7d-297619126e13");
        private static readonly Guid TComputerUnitDefinitionId = new Guid("020357c3-bfb2-4583-b285-3ed31e0e24f7");
        private static readonly Guid ExistingUserId            = new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3");

        private static readonly IReadOnlyList<Guid> SeededTDeviceUnitIds = new[]
        {
            new Guid("a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7"),
            new Guid("53ced1ab-2582-4aee-b2bc-50e676eebde3"),
            new Guid("58a98dbf-ce5d-43d1-adb2-670dea20c7bf"),
            new Guid("36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b"),
            new Guid("24d8f6ff-b411-4acc-8a35-5e958ce7f070"),
            new Guid("1c16f974-8e52-408b-9cac-acbb548864fa")
        };

        private readonly IDXUnitDataService _service;
        private readonly IDXStructureCache _structureCache;
        private readonly IDXRawReader _rawReader;

        public DXDerivedUnitTypeTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            _service       = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _structureCache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();
            _rawReader     = base.ServiceProvider.GetRequiredService<IDXRawReader>();
        }

        // -----------------------------------------------------------------------
        // Structure-level: the DerivedDXUnitType relation must exist on TDeviceUnit
        // -----------------------------------------------------------------------

        [Fact]
        public void DerivedDXUnitTypeRelation_WhenTComputerUnitInheritsTDeviceUnit_ExistsWithManyToOneType()
        {
            var relations = _structureCache.GetDXRelations("TDeviceUnit");

            var relation = relations.SingleOrDefault(r =>
                r.RelationNameRight == "DerivedDXUnitType" &&
                r.ObjectNameRight   == "DXUnitDefinitionUnit");

            Assert.NotNull(relation);
            Assert.Equal(DXRelationTypeEnum.ManyToOne, relation.RelationType);
        }

        // -----------------------------------------------------------------------
        // Data-level: seeded TDeviceUnit rows must point to TDeviceUnit definition
        // -----------------------------------------------------------------------

        [Fact]
        public void DerivedDXUnitType_ForSeededTDeviceUnitEntries_EqualsTDeviceUnitDefinitionId()
        {
            var columns = new Dictionary<string, string> { ["DerivedDXUnitType"] = "DerivedDXUnitType" };
            var rows    = _rawReader.Get("TDeviceUnit", columns);
            var byId    = rows.Data.Items.ToDictionary(x => x.Id);

            foreach (var id in SeededTDeviceUnitIds)
            {
                Assert.True(byId.ContainsKey(id), $"Seeded TDeviceUnit {id} not found in table");

                var derivedType = Guid.Parse(byId[id].Fields["DerivedDXUnitType"].ToString());
                Assert.Equal(TDeviceUnitDefinitionId, derivedType);
            }
        }

        // -----------------------------------------------------------------------
        // Insert TDeviceUnit — DerivedDXUnitType must equal TDeviceUnit definition
        // -----------------------------------------------------------------------

        [Fact]
        public async Task DerivedDXUnitType_WhenInsertingNewTDeviceUnit_EqualsTDeviceUnitDefinitionId()
        {
            var id     = new Guid("7987d811-4626-4ea7-b80d-9798be2ae389");
            var device = TDeviceUnitFactory.GetItem(
                id,
                model: "TestModelDevice",
                uuid:  Guid.NewGuid(),
                user:  new TUserUnit { Id = ExistingUserId });

            await _service.InsertAsync(device);

            var columns = new Dictionary<string, string> { ["DerivedDXUnitType"] = "DerivedDXUnitType" };
            var row     = _rawReader.Get("TDeviceUnit", columns)
                                    .Data.Items
                                    .SingleOrDefault(x => x.Id == id);

            Assert.NotNull(row);
            Assert.Equal(TDeviceUnitDefinitionId, Guid.Parse(row.Fields["DerivedDXUnitType"].ToString()));
        }

        // -----------------------------------------------------------------------
        // Insert TComputerUnit — TDeviceUnit row must carry TComputerUnit definition
        // -----------------------------------------------------------------------

        [Fact]
        public async Task DerivedDXUnitType_WhenInsertingTComputerUnit_EqualsTComputerUnitDefinitionIdInTDeviceUnitTable()
        {
            var id = new Guid("e828afee-3986-4a4a-ad31-36fc6224e280");
            var computer = new TComputerUnit
            {
                Id   = id,
                User = ExistingUserId,
                TDeviceMainElement = new TDeviceMainElement
                {
                    Id       = Guid.NewGuid(),
                    DXUnitId = id,
                    Model    = "TestModelComputer",
                    UUID     = Guid.NewGuid()
                }
            };

            await _service.InsertAsync(computer);

            var columns = new Dictionary<string, string> { ["DerivedDXUnitType"] = "DerivedDXUnitType" };
            var row     = _rawReader.Get("TDeviceUnit", columns)
                                    .Data.Items
                                    .SingleOrDefault(x => x.Id == id);

            Assert.NotNull(row);
            Assert.Equal(TComputerUnitDefinitionId, Guid.Parse(row.Fields["DerivedDXUnitType"].ToString()));
        }
    }
}
