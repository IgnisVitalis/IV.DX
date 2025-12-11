using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Models;
using IV.DX.Persistence;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXUnitWithRelationTests : IntTestController
    {
        IDXUnitDataService _service;
        IDXUnitGenericRepository _genericRepo;
        IDXStructureRepository _dataStructureRepo;
        ISQLQueryBuilder _sqlQueryBuilder;

        public DXUnitWithRelationTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            this._service = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            this._genericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._dataStructureRepo = base.ServiceProvider.GetRequiredService<IDXStructureRepository>();
            this._sqlQueryBuilder = base.ServiceProvider.GetRequiredService<ISQLQueryBuilder>();
        }

        [Fact]
        public async Task F()
        {
            // Init
            var dxUnitDefinitionToInsert = "{\n  \"S_Type\": \"DXUnitDefinitionUnit\",\n  \"ID\": \"cc2a1275-5a0f-468a-be92-b4715b94ab19\",\n  \"TimeStamp\": \"2025-12-11T10:20:09.399068Z\",\n  \"DXObjectDefinitionMainElement\": {\n    \"S_Type\": \"DXObjectDefinitionMainElement\",\n    \"ID\": \"28cfc2b0-fc34-4847-ad9b-86b1be155474\",\n    \"TimeStamp\": \"2025-12-11T10:20:09.388676Z\",\n    \"Name\": \"DXNavigationItemUnit\",\n    \"DisplayValue\": null,\n    \"DXUnitID\": \"cc2a1275-5a0f-468a-be92-b4715b94ab19\",\n    \"Kind\": 1\n  }\n}";
            var dxUnitDefinitionToUpdate = "{\n  \"S_Type\": \"DXUnitDefinitionUnit\",\n  \"ID\": \"cc2a1275-5a0f-468a-be92-b4715b94ab19\",\n  \"TimeStamp\": \"2025-12-11T10:20:09.399068Z\",\n  \"DXObjectDefinitionMainElement\": {\n    \"S_Type\": \"DXObjectDefinitionMainElement\",\n    \"ID\": \"28cfc2b0-fc34-4847-ad9b-86b1be155474\",\n    \"TimeStamp\": \"2025-12-11T10:20:09.388676Z\",\n    \"Name\": \"DXNavigationItemUnit\",\n    \"DisplayValue\": null,\n    \"DXUnitID\": \"cc2a1275-5a0f-468a-be92-b4715b94ab19\",\n    \"Kind\": 1\n  },  \n  \"DXUnitRelationElement\": {\n    \"S_Type\": \"DXUnitRelationElement\",\n    \"Mode\": 2,\n    \"Announced\": [\n      {\n        \"OwnRelationName\": \"Parent\",\n        \"TargetRelationName\": \"Children\",\n        \"RelationType\": 5,\n        \"TargetDXUnit\": \"cc2a1275-5a0f-468a-be92-b4715b94ab19\",\n        \"S_Type\": \"DXUnitRelationElement\",\n        \"ID\": \"1676cad5-c5d6-4584-8d13-e0155fbd8b1b\",\n        \"DXUnitID\": \"cc2a1275-5a0f-468a-be92-b4715b94ab19\",\n        \"TimeStamp\": \"2025-12-11T10:20:16.0861678Z\"\n      }\n    ],\n    \"Deleted\": []\n  }\n}";

            // Action
            var createdItem = await this._service.InsertAsync(JObject.Parse(dxUnitDefinitionToInsert));
            var updateItem = await this._service.UpdateAsync(JObject.Parse(dxUnitDefinitionToUpdate));

            // Assert
            var id = Guid.Parse(createdItem["ID"].ToString());

            var existingItem = await this._service.GetItemAsync("DXUnitDefinitionUnit", id);          
        }

        [Fact]
        public async Task T()
        {
            // Init
            var id = new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c");

            // Action

            var dxObject = await this._service.GetItemAsync<DXUnitDefinitionUnit>(id);

            // Assert
        }


        [DXUnit("DXUnitDefinitionUnit")]
        private class DXUnitDefinitionUnit : DXUnit
        {

            [DXColumn("DXUnitDefinitionUnitID", "R(DXUnitDefinitionUnit).ID")]
            public Guid DXUnitDefinitionUnitID { get; set; }
        }
    }
}