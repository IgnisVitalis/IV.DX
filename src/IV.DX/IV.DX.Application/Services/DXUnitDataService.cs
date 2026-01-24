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
    internal class DXUnitDataService(IDXUnitCoreRepository coreRepo, IDXPipelineExecutor dxPipelineExecutor) : IDXUnitDataService
    {
        public async Task<T> GetItemAsync<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<IEnumerable<T>> GetItemsAsync<T>(DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<IEnumerable<T>> GetItemsAsync<T>(IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string dxFilter, DXHandlerBaseContext? context = default, DXLoadingType typeOfLoading = DXLoadingType.Full, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync<T>(dxFilter, context, ct);

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

            throw new Exception($"There are an error to get dxUnit by query ({dxFilter}): {result.Error}");
        }

        public async Task<T> InsertAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<T> InsertOrUpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<T> UpdateAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<bool> IsItemExistingAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, IEnumerable<Guid> ids, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<IEnumerable<JObject>> GetItemsAsync(string typeName, string dxFilter, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var result = await dxPipelineExecutor.GetItemsAsync(typeName, dxFilter, context, ct);

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

            throw new Exception($"There are an error to get all dxModel by type ({typeName}) and query ({dxFilter}): {result.Error}");
        }

        public async Task<JObject> GetItemAsync(string typeName, Guid id, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<bool> DeleteAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }          

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

        public async Task<JObject> InsertAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<JObject> UpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

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

        public async Task<bool> DeleteAsync(JObject jObject, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }
                
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

        public async Task<JObject> InsertOrUpdateAsync(JObject jObject, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            var dxModel = DXModelConverter.ToDXModel(jObject);

            var objId = dxModel.DXMainElement.Item.ID;

            if (await IsItemExistingAsync(dxModel.DXMainElement.Attribute.Type, objId, context, ct))
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