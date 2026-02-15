using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Pipeline;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal class DXUnitDataService(
        IDXUnitCoreRepository coreRepo,
        IDXPipelineExecutor dxPipelineExecutor,
        IDXUnitTypeAccessChecker unitTypeAccessChecker) : IDXUnitDataService
    {
        public async Task<T> InsertAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccess(AttributeReader.GetDXUnitTypeName(dxUnit.GetType()));

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
            EnsureWriteAccess(typeName);

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

            EnsureWriteAccess(AttributeReader.GetDXUnitTypeName(dxUnit.GetType()));

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

            EnsureReadAccess(typeName);

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

        public async Task<bool> DeleteAsync<T>(T dxUnit, DXHandlerBaseContext? context = default, CancellationToken ct = default) where T : DXUnit, new()
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccess(AttributeReader.GetDXUnitTypeName(dxUnit.GetType()));

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

            EnsureWriteAccess(ExtractTypeName(jObject));

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

            EnsureWriteAccess(ExtractTypeName(jObject));

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

            EnsureWriteAccess(ExtractTypeName(jObject));
                
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

            EnsureWriteAccess(ExtractTypeName(jObject));

            var block = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            if (block == null)
                throw new Exception("Invalid DXDataBlock payload.");

            var processed = await InsertOrUpdateAsync(block, context, ct);
            return JObject.FromObject(processed);
        }

        public async Task<DXDataBlock<DXUnitRecord>> InsertAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccess(block?.Meta?.Type);

            var result = await dxPipelineExecutor.InsertAsync(block, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to insert dxUnit: {result.Error}");
            }
        }

        public async Task<DXDataBlock<DXUnitRecord>> UpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccess(block?.Meta?.Type);

            var result = await dxPipelineExecutor.UpdateAsync(block, context, ct);

            if (result.IsSuccess && result.Value != null)
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"There are an error to update dxUnit: {result.Error}");
            }
        }

        public async Task<bool> DeleteAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = default, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            EnsureWriteAccess(block?.Meta?.Type);

            var result = await dxPipelineExecutor.DeleteAsync(block, context, ct);

            if (result.IsSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<DXDataBlock<DXUnitRecord>> InsertOrUpdateAsync(DXDataBlock<DXUnitRecord> block, DXHandlerBaseContext? context = null, CancellationToken ct = default)
        {
            if (context == null)
            {
                context = new DXHandlerContext();
            }

            ArgumentNullException.ThrowIfNull(block);
            var typeName = block.Meta?.Type;
            EnsureWriteAccess(typeName);

            var output = new List<DXUnitRecord>();

            if (block.Data?.Items != null)
            {
                foreach (var record in block.Data.Items)
                {
                    if (record == null) continue;

                    var itemIsExisting = !string.IsNullOrWhiteSpace(typeName)
                        && await this.IsItemExistingAsync(typeName, record.ID, context, ct);

                    var singleBlock = new DXDataBlock<DXUnitRecord>
                    {
                        Meta = block.Meta,
                        Data = new DXData<DXUnitRecord>
                        {
                            Items = new List<DXUnitRecord> { record }
                        }
                    };

                    var processed = itemIsExisting
                        ? await UpdateAsync(singleBlock, context, ct)
                        : await InsertAsync(singleBlock, context, ct);

                    if (processed.Data?.Items != null)
                        output.AddRange(processed.Data.Items);
                }
            }

            if (block.Data?.Delete != null && block.Data.Delete.Count > 0)
            {
                var deleteBlock = new DXDataBlock<DXUnitRecord>
                {
                    Meta = block.Meta,
                    Data = new DXData<DXUnitRecord>
                    {
                        Delete = block.Data.Delete
                    }
                };

                await DeleteAsync(deleteBlock, context, ct);
            }

            return new DXDataBlock<DXUnitRecord>
            {
                Meta = block.Meta,
                Data = new DXData<DXUnitRecord>
                {
                    Items = output.Count == 0 ? null : output,
                    Delete = block.Data?.Delete
                }
            };
        }

        private static string? ExtractTypeName(JObject jObject)
        {
            return jObject["Meta"]?["Type"]?.ToString();
        }

        private void EnsureReadAccess(string? typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                unitTypeAccessChecker.EnsureAccess(typeName, DXUnitTypeAccessOperation.Read);
            }
        }

        private void EnsureWriteAccess(string? typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                unitTypeAccessChecker.EnsureAccess(typeName, DXUnitTypeAccessOperation.Write);
            }
        }
    }
}

