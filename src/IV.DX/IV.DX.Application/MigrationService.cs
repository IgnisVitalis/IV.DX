using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IV.DX.Application
{
    internal sealed class MigrationService : IDXMigrationService
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXElementCoreRepository _dxElementCoreRepo;
        private readonly IDXEnumDataService _enumDataService;
        private readonly IDXElementDataService _elementDataService;

        private readonly SemaphoreSlim _lock = new(1, 1);

        private static readonly Regex ScriptNameRegex = new(
            @"^(?<Version>\d+)_(?<Build>\d+)_(?<Number>\d+)_(?<Application>[A-Za-z0-9]+)_(?<Name>[A-Za-z0-9]+)\.(?<Extension>[A-Za-z0-9]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public MigrationService(
            IDXUnitGenericRepository genericRepo,
            IDXUnitDataService dataService,
              IDXElementCoreRepository dxElementCoreRepo,
              IDXEnumDataService enumDataService,
              IDXElementDataService elementDataService)
        {
            _genericRepo = genericRepo;
            _dataService = dataService;
            _dxElementCoreRepo = dxElementCoreRepo;
            _enumDataService = enumDataService;
            _elementDataService = elementDataService;
        }

        public async Task MigrateCustomAsync(string path, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                await LoadAsync(path, ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task MigrateCustomEmbeddedAsync(Assembly assembly, string path, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);

            try
            {
                var list = ResourceReader.ReadEmbeddedText(assembly, path);

                await LoadAsync(assembly, path, ct);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task MigrateCoreAsync(
            Assembly assembly,
            string preInitListPath,
            string postInitListPath,
            CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);

            try
            {
                IEnumerable<DXMigrationScriptsUnit> scriptsHistory;
                try
                {
                    scriptsHistory = GetScriptsHistoryIfExisting();
                    if (scriptsHistory != null && scriptsHistory.Any())
                        return;
                }
                catch
                {
                    var corePreInitList = ResourceReader.ReadEmbeddedText(assembly, preInitListPath);
                    var scriptsPreInit = GetMigrationScriptsFromEmbedded(assembly, preInitListPath, corePreInitList);

                    foreach (var script in scriptsPreInit)
                    {
                        try
                        {
                            switch (script.Extension)
                            {
                                case "unit":
                                    {
                                        var blocks = ParseUnitBlocks(script.Content);
                                        await ProcessUnitBlocksAsync(
                                            script,
                                            blocks,
                                            block => _dataService.InsertAsync(block, new DXUnitHandlerPreInitCoreContext(script), ct),
                                            deleteAction: null,
                                            ct).ConfigureAwait(false);
                                        break;
                                    }
                                case "element":
                                    {
                                        var blocks = ParseElementBlocks(script.Content);
                                        await ProcessElementBlocksAsync(
                                            script,
                                            blocks,
                                            block => _elementDataService.InsertOrUpdateAsync(block, ct),
                                            ct).ConfigureAwait(false);

                                        break;
                                    }
                                case "enum":
                                    {
                                        var blocks = ParseEnumBlocks(script.Content);
                                        await ProcessEnumBlocksAsync(
                                            script,
                                            blocks,
                                            block => _enumDataService.InsertOrUpdateAsync(block, ct),
                                            ct).ConfigureAwait(false);

                                        break;
                                    }
                                default:
                                    throw new Exception($"File extension '{script.Extension}' is not supported.");
                            }

                        }
                        catch (Exception exc)
                        {
                            throw new Exception($"Error occurred when Pre Init migration script '{script}' was applied.", exc);
                        }
                    }

                    var corePostInitList = ResourceReader.ReadEmbeddedText(assembly, postInitListPath);
                    var scriptsPostInit = GetMigrationScriptsFromEmbedded(assembly, postInitListPath, corePostInitList);

                    foreach (var script in scriptsPostInit)
                    {
                        try
                        {
                            await ProcessFileForPostInitCoreAsync(script, ct).ConfigureAwait(false);
                        }
                        catch (Exception exc)
                        {
                            throw new Exception($"Error occurred when Post Init migration script '{script}' was applied.", exc);
                        }
                    }

                    foreach (var script in scriptsPreInit)
                    {
                        await _dataService.InsertAsync(script, new DXUnitHandlerMigrationServiceContext(script), ct).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task LoadAsync(string path, CancellationToken ct)
        {
            var listJson = await File.ReadAllTextAsync(GetFullPath(path), ct).ConfigureAwait(false);
            var scripts = GetMigrationScriptsFromFs(path, listJson);

            await ProcessScripts(scripts, ct);
        }

        private async Task LoadAsync(Assembly assembly, string path, CancellationToken ct)
        {
            var listJson = ResourceReader.ReadEmbeddedText(assembly, path);
            var scripts = GetMigrationScriptsFromEmbedded(assembly, path, listJson);

            await ProcessScripts(scripts, ct);
        }

        private async Task ProcessScripts(IEnumerable<DXMigrationScriptsUnit> scripts, CancellationToken ct)
        {
            var scriptsHistory = GetScriptsHistoryIfExisting();
            var historySet = new HashSet<DXMigrationScriptsUnit>(scriptsHistory ?? Enumerable.Empty<DXMigrationScriptsUnit>());

            foreach (var script in scripts)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    if (historySet.Contains(script))
                        continue;

                    await ProcessByExtensionAsync(script, ct).ConfigureAwait(false);
                    await _dataService.InsertAsync(script, new DXUnitHandlerMigrationServiceContext(script), ct).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw new Exception($"Error occurred when migration script '{script}' was applied.", exc);
                }
            }
        }

        private async Task ProcessByExtensionAsync(DXMigrationScriptsUnit script, CancellationToken ct)
        {
            var ext = script.Extension?.ToLowerInvariant();
            switch (ext)
            {
                case "dat":   // insert or update
                    await ProcessFileToInsertOrUpdateAsync(script, ct).ConfigureAwait(false);
                    break;
                case "del":   // delete
                    await ProcessFileToInsertOrUpdateAsync(script, ct).ConfigureAwait(false);
                    break;

                default:
                    throw new NotSupportedException($"Migration script '{script}' has unsupported extension '{ext}'.");
            }
        }

        private IEnumerable<DXMigrationScriptsUnit> GetScriptsHistoryIfExisting()
            => _genericRepo.GetDXUnits<DXMigrationScriptsUnit>();

        private static bool TryParseScriptMeta(
            string fileName,
            out (string Version, string Build, string Number, string App, string Name, string Extension) meta)
        {
            var m = ScriptNameRegex.Match(fileName);
            if (!m.Success) { meta = default; return false; }

            meta = (
                m.Groups["Version"].Value,
                m.Groups["Build"].Value,
                m.Groups["Number"].Value,
                m.Groups["Application"].Value,
                m.Groups["Name"].Value,
                m.Groups["Extension"].Value
            );
            return true;
        }

        private static IEnumerable<DXMigrationScriptsUnit> GetMigrationScriptsFromEmbedded(
            Assembly assembly, string listPath, string listContent)
        {
            var baseDir = NormalizeDirectory(Path.GetDirectoryName(listPath));
            var list = JArray.Parse(listContent);

            return list.Select(item =>
            {
                var raw = item.ToString();
                var rawNorm = raw.Replace('\\', '/');
                var fileName = Path.GetFileName(rawNorm);

                if (!TryParseScriptMeta(fileName, out var meta))
                    throw new FormatException(
                        $"Script name '{raw}' has wrong format. Expected '<Version>_<Build>_<Number>_<Application>_<Name>.<Extension>'.");

                var fullResourcePath = string.IsNullOrEmpty(baseDir) || rawNorm.StartsWith(baseDir + "/", StringComparison.OrdinalIgnoreCase)
                    ? rawNorm
                    : $"{baseDir}/{rawNorm}";

                var content = ResourceReader.ReadEmbeddedText(assembly, fullResourcePath);

                var id = Guid.NewGuid();

                return new DXMigrationScriptsUnit
                {
                    ID = id,

                    FilePath = rawNorm,
                    Name = meta.Name,
                    Version = meta.Version,
                    Build = meta.Build,
                    Number = meta.Number,
                    AppName = meta.App,
                    Extension = meta.Extension,
                    Content = content
                };
            }).ToList();
        }

        private IEnumerable<DXMigrationScriptsUnit> GetMigrationScriptsFromFs(string listPath, string listContent)
        {
            var baseDir = new FileInfo(listPath).DirectoryName ?? string.Empty;
            var list = JArray.Parse(listContent);

            return list.Select(x => new FileInfo(GetFullPath(Path.Combine(baseDir, x.ToString()))))
                       .Select(fi =>
                       {
                           if (!TryParseScriptMeta(fi.Name, out var meta))
                               throw new FormatException(
                                   $"Script name '{fi.Name}' has wrong format. Expected '<Version>_<Build>_<Number>_<Application>_<Name>.<Extention>'.");

                           var id = Guid.NewGuid();
                           return new DXMigrationScriptsUnit
                           {
                               ID = id,

                               FilePath = fi.FullName,
                               Name = meta.Name,
                               Version = meta.Version,
                               Build = meta.Build,
                               Number = meta.Number,
                               AppName = meta.App,
                               Extension = meta.Extension,
                               Content = File.ReadAllText(fi.FullName)
                           };
                       })
                       .ToList();
        }

        private async Task ProcessFileForPreInitCoreAsync(DXMigrationScriptsUnit file, CancellationToken ct)
        {
            var blocks = ParseUnitBlocks(file.Content);
            await ProcessUnitBlocksAsync(
                file,
                blocks,
                block => _dataService.InsertAsync(block, new DXUnitHandlerPreInitCoreContext(file), ct),
                deleteAction: null,
                ct).ConfigureAwait(false);
        }

        private async Task ProcessFileForPostInitCoreAsync(DXMigrationScriptsUnit script, CancellationToken ct)
        {
            var ext = script.Extension?.ToLowerInvariant();
            switch (ext)
            {
                case "unit":
                    {
                        var blocks = ParseUnitBlocks(script.Content);
                        await ProcessUnitBlocksAsync(
                            script,
                            blocks,
                            block => _dataService.InsertOrUpdateAsync(block, new DXUnitHandlerPostInitCoreContext(script), ct),
                            deleteAction: null,
                            ct).ConfigureAwait(false);
                        break;
                    }
                case "element":
                    {
                        var blocks = ParseElementBlocks(script.Content);
                        await ProcessElementBlocksAsync(
                            script,
                            blocks,
                            block => _elementDataService.InsertOrUpdateAsync(block, ct),
                            ct).ConfigureAwait(false);
                        break;
                    }
                case "enum":
                    {
                        var blocks = ParseEnumBlocks(script.Content);
                        await ProcessEnumBlocksAsync(
                            script,
                            blocks,
                            block => _enumDataService.InsertOrUpdateAsync(block, ct),
                            ct).ConfigureAwait(false);
                        break;
                    }
                default:
                    throw new NotSupportedException($"Migration script '{script}' has unsupported extension '{ext}'.");
            }
        }

        private async Task ProcessFileToInsertOrUpdateAsync(DXMigrationScriptsUnit file, CancellationToken ct)
        {
            var blocks = ParseUnitBlocks(file.Content);
            await ProcessUnitBlocksAsync(
                file,
                blocks,
                block => _dataService.InsertOrUpdateAsync(block, new DXUnitHandlerMigrationServiceContext(file), ct),
                block => _dataService.DeleteAsync(block, new DXUnitHandlerMigrationServiceContext(file), ct),
                ct).ConfigureAwait(false);
        }

        private string GetMigrationErrorMessage(DXMigrationScriptsUnit file, JObject item)
        {
            return $"{file.ToString()}\nDXUnit with ID '{item["ID"]}' migration error";
        }

        private string GetMigrationErrorMessage(DXMigrationScriptsUnit file, Guid id)
        {
            return $"{file.ToString()}\nDXUnit with ID '{id}' migration error";
        }

        private static List<DXDataBlock<DXUnitRecord>> ParseUnitBlocks(string content)
        {
            return JsonConvert.DeserializeObject<List<DXDataBlock<DXUnitRecord>>>(content)
                ?? new List<DXDataBlock<DXUnitRecord>>();
        }

        private static List<DXDataBlock<DXEnumRecord>> ParseEnumBlocks(string content)
        {
            return JsonConvert.DeserializeObject<List<DXDataBlock<DXEnumRecord>>>(content)
                ?? new List<DXDataBlock<DXEnumRecord>>();
        }

        private static List<DXDataBlock<DXElementRecord>> ParseElementBlocks(string content)
        {
            return JsonConvert.DeserializeObject<List<DXDataBlock<DXElementRecord>>>(content)
                ?? new List<DXDataBlock<DXElementRecord>>();
        }

        private async Task ProcessUnitBlocksAsync(
            DXMigrationScriptsUnit script,
            IEnumerable<DXDataBlock<DXUnitRecord>> blocks,
            Func<DXDataBlock<DXUnitRecord>, Task> upsertAction,
            Func<DXDataBlock<DXUnitRecord>, Task>? deleteAction,
            CancellationToken ct)
        {
            foreach (var block in blocks)
            {
                if (block == null)
                    continue;

                ct.ThrowIfCancellationRequested();

                if (block.Data?.Upsert != null)
                {
                    foreach (var record in block.Data.Upsert)
                    {
                        if (record == null) continue;

                        var single = new DXDataBlock<DXUnitRecord>
                        {
                            Meta = block.Meta,
                            Data = new DXData<DXUnitRecord>
                            {
                                Upsert = new List<DXUnitRecord> { record }
                            }
                        };

                        try
                        {
                            await upsertAction(single).ConfigureAwait(false);
                        }
                        catch (Exception exc)
                        {
                            throw new Exception(this.GetMigrationErrorMessage(script, record.ID), exc);
                        }
                    }
                }

                if (deleteAction != null && block.Data?.Delete != null)
                {
                    foreach (var deleteRef in block.Data.Delete)
                    {
                        var single = new DXDataBlock<DXUnitRecord>
                        {
                            Meta = block.Meta,
                            Data = new DXData<DXUnitRecord>
                            {
                                Delete = new List<DXDeleteRef> { deleteRef }
                            }
                        };

                        try
                        {
                            await deleteAction(single).ConfigureAwait(false);
                        }
                        catch (Exception exc)
                        {
                            throw new Exception(this.GetMigrationErrorMessage(script, deleteRef.ID), exc);
                        }
                    }
                }
            }
        }

        private async Task ProcessEnumBlocksAsync(
            DXMigrationScriptsUnit script,
            IEnumerable<DXDataBlock<DXEnumRecord>> blocks,
            Func<DXDataBlock<DXEnumRecord>, Task> upsertAction,
            CancellationToken ct)
        {
            foreach (var block in blocks)
            {
                if (block == null)
                    continue;

                ct.ThrowIfCancellationRequested();

                if (block.Data?.Upsert == null)
                    continue;

                foreach (var record in block.Data.Upsert)
                {
                    if (record == null) continue;

                    var single = new DXDataBlock<DXEnumRecord>
                    {
                        Meta = block.Meta,
                        Data = new DXData<DXEnumRecord>
                        {
                            Upsert = new List<DXEnumRecord> { record }
                        }
                    };

                    try
                    {
                        await upsertAction(single).ConfigureAwait(false);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(this.GetMigrationErrorMessage(script, record.ID), exc);
                    }
                }
            }
        }

        private async Task ProcessElementBlocksAsync(
            DXMigrationScriptsUnit script,
            IEnumerable<DXDataBlock<DXElementRecord>> blocks,
            Func<DXDataBlock<DXElementRecord>, Task> upsertAction,
            CancellationToken ct)
        {
            foreach (var block in blocks)
            {
                if (block == null)
                    continue;

                ct.ThrowIfCancellationRequested();

                if (block.Data?.Upsert == null)
                    continue;

                foreach (var record in block.Data.Upsert)
                {
                    if (record == null) continue;

                    var single = new DXDataBlock<DXElementRecord>
                    {
                        Meta = block.Meta,
                        Data = new DXData<DXElementRecord>
                        {
                            Upsert = new List<DXElementRecord> { record }
                        }
                    };

                    try
                    {
                        await upsertAction(single).ConfigureAwait(false);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(this.GetMigrationErrorMessage(script, record.ID), exc);
                    }
                }
            }
        }

        private static string NormalizeDirectory(string? dir)
            => string.IsNullOrWhiteSpace(dir) ? string.Empty : dir.Replace('\\', '/');

        private static string GetFullPath(string path)
            => Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }
}
