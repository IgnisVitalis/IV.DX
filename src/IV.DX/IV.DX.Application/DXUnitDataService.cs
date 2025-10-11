using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application
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

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string esqlWhereExpression, IDXHandlerContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.GetItemsAsync<T>(esqlWhereExpression, context, ct);

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

            throw new Exception($"There are an error to get dxUnit by query ({esqlWhereExpression}): {result.Error}");
        }

        public async Task<T> InsertAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.InsertAsync<T>(esqlObject, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<T> InsertOrUpdateAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var typeName = AttributeReader.GetESQLObjectTypeName(esqlObject.GetType());

            var itemIsExisting = coreRepo.IsItemExisting(typeName, esqlObject.ID);

            if (itemIsExisting)
            {
                return await this.UpdateAsync(esqlObject, context, ct);
            }
            else
            {
                return await this.InsertAsync(esqlObject, context, ct);
            }
        }

        public async Task<T> UpdateAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.UpdateAsync<T>(esqlObject, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> IsItemExistingAsync(Guid id, string type, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var entityType = type;

            if (EntityHandlerProvider.IsCustomHandlerExisting(entityType))
            {
                var handler = EntityHandlerProvider.GetHandler(entityType);

                return handler.IsItemExisting(id, context);
            }
            else
            {
                return EntityHandlerProvider.CoreModelHandler.IsItemExisting(entityType, id, context);
            }
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
                    return null;
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
                    return null;
                }
            }

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and IDs: {result.Error}");
        }

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string esqlWhereExpression, IDXHandlerContext? context = default, CancellationToken ct = default)
        {
            var result = await dxPipelineExecutor.GetItemsAsync(typeName, esqlWhereExpression, context, ct);

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

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and query ({esqlWhereExpression}): {result.Error}");
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

        public async Task<bool> DeleteAsync<T>(T esqlObject, IDXHandlerContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            var result = await dxPipelineExecutor.DeleteAsync<T>(esqlObject, context, ct);

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
            var esqlModel = DXModel.CreateInstance(jObject);

            var objId = esqlModel.OwnSingleItem.Item.ID;

            if (objId.HasValue
                && await this.IsItemExistingAsync(objId.Value, esqlModel.OwnSingleItem.ObjectInfo.ObjectName, context, ct))
            {
                return await this.UpdateAsync(jObject, context, ct);
            }
            else
            {
                return await this.InsertAsync(jObject, context, ct);
            }
        }
    }
}