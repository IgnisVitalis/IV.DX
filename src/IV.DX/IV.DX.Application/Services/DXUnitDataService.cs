using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal class DXUnitDataService(IDXCoreRepository coreRepo, IDXPipelineExecutor dxPipelineExecutor) : IDXUnitDataService
    {
        public async Task<T> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.GetAsync<T>(id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return null;
                }
            }

            throw new Exception($"There are an error to get dxUnit by ID ({id}): {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.GetItemsAsync<T>(context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<T>();
                }
            }

            throw new Exception($"There are an error to get all dxUnit: {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.GetItemsAsync<T>(ids, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<T>();
                }
            }

            throw new Exception($"There are an error to get dxUnit by ids: {result.Error}");
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string dxsqlWhereExpression, IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.GetItemsAsync<T>(dxsqlWhereExpression, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<T>();
                }
            }

            throw new Exception($"There are an error to get dxUnit by query ({dxsqlWhereExpression}): {result.Error}");
        }

        public async Task<T> InsertAsync<T>(T dxUnit, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.InsertAsync(dxUnit, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<T> InsertOrUpdateAsync<T>(T dxUnit, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var typeName = AttributeReader.GetDXUnitTypeName(dxUnit.GetType());

            var itemIsExisting = coreRepo.IsItemExisting(typeName, dxUnit.ID);

            if (itemIsExisting)
            {
                return await UpdateAsync(dxUnit, context, ct);
            }
            else
            {
                return await InsertAsync(dxUnit, context, ct);
            }
        }

        public async Task<T> UpdateAsync<T>(T dxUnit, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.UpdateAsync(dxUnit, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> IsItemExistingAsync(string typeName, Guid id, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.IsUnitExistingAsync(typeName, id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok)
                {
                    return result.Value;
                }
            }

            throw new Exception($"There are an error to check dxModel existing by type ({typeName}) and id ({id}): {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.GetItemsAsync(typeName, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<JObject>();
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}): {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.GetItemsAsync(typeName, ids, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<JObject>();
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and IDs: {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string dxsqlWhereExpression, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.GetItemsAsync(typeName, dxsqlWhereExpression, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return Enumerable.Empty<JObject>();
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and query ({dxsqlWhereExpression}): {result.Error}");
        }

        public async Task<JObject> GetItemAsync(string typeName, Guid id, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.GetAsync(typeName, id, context, ct);

            if (result.IsSuccess)
            {
                if (result.Outcome == DXOutcome.Ok && result.Value != null)
                {
                    return result.Value;
                }
                else if (result.Outcome == DXOutcome.NotFound)
                {
                    return null;
                }
            }

            throw new Exception($"There are an error to get dxModel by ID ({id}): {result.Error}");
        }

        public async Task<bool> DeleteAsync<T>(T dxUnit, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.DeleteAsync(dxUnit, context, ct);

            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<JObject> InsertAsync(JObject jObject, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.InsertAsync(jObject, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<JObject> UpdateAsync(JObject jObject, IDXHandlerContext? context = null, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.UpdateAsync(jObject, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> DeleteAsync(JObject jObject, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.DeleteAsync(jObject, context, ct);

            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<JObject> InsertOrUpdateAsync(JObject jObject, IDXHandlerContext? context = null, CancellationToken ct = default)
        {
            var dxModel = DXModelConverter.Parse(jObject);

            var objId = dxModel.DXMainElement.Item.ID;

            if (objId.HasValue
                && await IsItemExistingAsync(dxModel.DXMainElement.Attribute.Type, objId.Value, context, ct))
            {
                return await UpdateAsync(jObject, context, ct);
            }
            else
            {
                return await InsertAsync(jObject, context, ct);
            }
        }
    }
}