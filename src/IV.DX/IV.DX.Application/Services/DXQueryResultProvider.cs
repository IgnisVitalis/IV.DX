using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Application.Helpers;
using IV.DX.Application.PrivateModels.DXQueryUnit;
using IV.DX.Kernel;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace IV.DX.Application.Services
{
    internal class DXQueryResultProvider(IDXUnitDataService dataService, IDXRawReader dxRawReader, IDXStructureCache dxStructureCache) : IDXQueryResultProvider
    {
        public async Task<JObject> GetAsync(Guid dxQueryID, Guid? dxFilterID, CancellationToken ct = default)
        {
            var dxQuery = await dataService.GetItemAsync<DXQueryUnit>(dxQueryID);

            if (dxQuery == null)
                return null;

            JObject jObject = new JObject();

            jObject.Add(new JProperty(Constants.SystemPropertyTypeName, dxQuery.DXUnitName));
            jObject.Add(this.GetDataDefintion(dxQuery));
            jObject.Add(this.GetContent(dxQuery));

            return jObject;
        }

        private JProperty GetDataDefintion(DXQueryUnit dxQuery)
        {
            List<DXQueryColumnElement> list = new List<DXQueryColumnElement>()
            {
                new DXQueryColumnElement()
                {
                    Name = Constants.ID,
                    Expression = Constants.ID,
                    Order = -1
                }
            };

            var orderedColumns = list.Concat(dxQuery.DXQueryColumnElement.Announced.OrderBy(x => x.Order));

            var propsToIgnore = new[] { Constants.ID, Constants.DXUnitID, Constants.TimeStamp };

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new IgnorePropertiesResolver(propsToIgnore)
            };

            var serializer = JsonSerializer.Create(settings);

            return new JProperty("QueryDefinition", JArray.FromObject(orderedColumns, serializer));
        }

        private JProperty GetContent(DXQueryUnit dxQuery)
        {
            var orderedColumns = dxQuery.DXQueryColumnElement.Announced.OrderBy(x => x.Order);

            var columns = orderedColumns.ToDictionary(x => x.Name, x => x.Expression);

            if (!columns.ContainsKey(Constants.ID))
            {
                columns.Add(Constants.ID, Constants.ID);
            }

            if (!columns.ContainsKey(Constants.TimeStamp))
            {
                columns.Add(Constants.TimeStamp, Constants.TimeStamp);
            }


            var block = dxRawReader.Get(dxQuery.DXUnitName, columns);

            return new JProperty("Content", JObject.FromObject(block));
        }

        public async Task<IEnumerable<DXDisplayValue>> GetDisplayValuesAsync(string typeName, CancellationToken ct = default)
        {
            DXObjectDefinitionUnit dxObjectInfo;

            dxObjectInfo = dxStructureCache.GetDXUnit(typeName);

            if (dxObjectInfo == null)
            {
                dxObjectInfo = dxStructureCache.GetDXElement(typeName);

                if (dxObjectInfo == null)
                {
                    dxObjectInfo = dxStructureCache.GetDXEnum(typeName);

                    throw new Exception($"There are no type '{typeName}' to provide display values");
                }
            }

            var displayValueExpression =
                string.IsNullOrEmpty(dxObjectInfo.DisplayValue) ?
                "ID" :
                dxObjectInfo.DisplayValue;

            var columns = new Dictionary<string, string>()
            {
                {Constants.ID, Constants.ID },
                {Constants.TimeStamp, Constants.TimeStamp },
                {"DisplayValue",  displayValueExpression }
            };

            var result = dxRawReader.Get(typeName, columns);
            var records = result.Data?.Items ?? new List<DXUnitRecord>();

            var displayValues = records.Select(x => new DXDisplayValue()
            {
                ID = x.ID,
                Type = typeName,
                DisplayValue = x.Fields != null && x.Fields.TryGetValue("DisplayValue", out var v)
                    ? v?.ToString()
                    : string.Empty
            }).ToList();

            return displayValues;
        }
    }
}

