using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Services
{
    internal sealed class DXEncryptionMigrationService(
        IDXStructureService structureService,
        IDXUnitDataReader dataReader,
        IDXUnitDataService dataService,
        IDXEncryptionKeyProvider keyProvider,
        ILogger<DXEncryptionMigrationService> logger) : IDXEncryptionMigrationService
    {
        public async Task<DXEncryptionMigrationResult> MigrateAsync(CancellationToken ct = default)
        {
            var unitNames = FindUnitTypesWithEncryptedColumns();
            var key = keyProvider.GetCurrent();

            logger.LogInformation(
                "Starting encryption migration for key {KeyId}. Found {UnitTypeCount} DX unit type(s) with encrypted columns.",
                key.KeyId,
                unitNames.Count);

            var reencrypted = 0;
            var failed = 0;

            foreach (var typeName in unitNames)
            {
                ct.ThrowIfCancellationRequested();

                JObject? block;
                try
                {
                    block = await dataReader.GetItemsAsync(typeName, ct: ct);
                }
                catch (Exception ex)
                {
                    failed++;
                    logger.LogWarning(ex,
                        "Failed to load DX unit type {TypeName} during encryption migration.",
                        typeName);
                    continue;
                }

                if (block != null)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        await dataService.UpdateAsync(block, ct: ct);
                        reencrypted++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        logger.LogWarning(ex,
                            "Failed to re-encrypt DX unit type {TypeName}.",
                            typeName);
                    }
                }
            }

            logger.LogInformation(
                "Encryption migration finished for key {KeyId}. Re-encrypted {Reencrypted} item(s), failed {Failed} item(s).",
                key.KeyId,
                reencrypted,
                failed);

            return new DXEncryptionMigrationResult
            {
                Reencrypted = reencrypted,
                Failed = failed
            };
        }

        private List<string> FindUnitTypesWithEncryptedColumns()
        {
            var encryptedElementIds = structureService.DXElements
                .Where(e => e.DXColumnDefinitionElement.Announced
                    .Any(c => c.ColumnType == DXColumnTypeEnum.EncryptedString))
                .Select(e => e.Id)
                .ToHashSet();

            return structureService.DXUnits
                .Where(u => u.DXElementInUnitDefinitionElement.Announced
                    .Any(e => encryptedElementIds.Contains(e.DXElementDefinitionUnit)))
                .Select(u => u.Name)
                .ToList();
        }

    }
}
