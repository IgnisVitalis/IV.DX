using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Helpers;
using IV.DX.Application.PrivateModels.DXQueryUnit;
using IV.DX.Kernel;
using IV.DX.Kernel.Converters.JObjectConverters;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal class DXQueryResultProvider(IDXUnitDataService dataService, IDXRawReader dxRawReader) : IDXQueryResultProvider
    {
        public async Task<JObject> GetAsync(Guid dxQueryID, CancellationToken ct = default)
        {
            var dxQuery = await dataService.GetItemAsync<DXQueryUnit>(dxQueryID);

            if (dxQuery == null)
                return null;

            var dxUnits = await dataService.GetItemsAsync(dxQuery.DXUnitName);


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


            var result = dxRawReader.Get(dxQuery.DXUnitName, columns);

            var jArray = result.Announced.ToJArray(true);

            return new JProperty("Content", jArray);
        }
    }
}
