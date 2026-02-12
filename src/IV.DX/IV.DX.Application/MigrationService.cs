using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IV.DX.Application
{
    internal sealed class MigrationService : IDXMigrationService
    {
        private static readonly Regex ScriptNameRegex = new(
            @"^(?<Version>\d+)_(?<Build>\d+)_(?<Number>\d+)_(?<Application>[A-Za-z0-9]+)_(?<Name>[A-Za-z0-9]+)\.(?<Extension>[A-Za-z0-9]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitCoreRepository _coreRepo;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXEnumDataService _enumDataService;
        private readonly IDXElementDataService _elementDataService;

        private readonly SemaphoreSlim _lock = new(1, 1);

        public MigrationService(
            IDXUnitGenericRepository genericRepo,
            IDXUnitDataService dataService,
            IDXUnitCoreRepository coreRepo,
              IDXEnumDataService enumDataService,
              IDXElementDataService elementDataService)
        {
            _genericRepo = genericRepo;
            _dataService = dataService;
            _coreRepo = coreRepo;
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
                            await ProcessFileForPreInitCoreAsync(script, ct).ConfigureAwait(false);

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
                case "dx":    // new unified format
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
            var blocks = ParseBlocks(file.Content);
            await ProcessBlocksByKindAsync(
                file,
                blocks,
                unitUpsert: block => _dataService.InsertAsync(block, new DXUnitHandlerPreInitCoreContext(file), ct),
                unitDelete: null,
                enumUpsert: block => _enumDataService.InsertOrUpdateAsync(block, ct),
                elementUpsert: block => _elementDataService.InsertOrUpdateAsync(block, ct),
                ct).ConfigureAwait(false);
        }

        private async Task ProcessFileForPostInitCoreAsync(DXMigrationScriptsUnit script, CancellationToken ct)
        {
            var blocks = ParseBlocks(script.Content);
            await ProcessBlocksByKindAsync(
                script,
                blocks,
                unitUpsert: block => _dataService.InsertOrUpdateAsync(block, new DXUnitHandlerPostInitCoreContext(script), ct),
                unitDelete: null,
                enumUpsert: block => _enumDataService.InsertOrUpdateAsync(block, ct),
                elementUpsert: block => _elementDataService.InsertOrUpdateAsync(block, ct),
                ct).ConfigureAwait(false);
        }

        private async Task ProcessFileToInsertOrUpdateAsync(DXMigrationScriptsUnit file, CancellationToken ct)
        {
            var blocks = ParseBlocks(file.Content);
            await ProcessBlocksByKindAsync(
                file,
                blocks,
                unitUpsert: block => _dataService.InsertOrUpdateAsync(block, new DXUnitHandlerMigrationServiceContext(file), ct),
                unitDelete: block => _dataService.DeleteAsync(block, new DXUnitHandlerMigrationServiceContext(file), ct),
                enumUpsert: block => _enumDataService.InsertOrUpdateAsync(block, ct),
                elementUpsert: block => _elementDataService.InsertOrUpdateAsync(block, ct),
                ct).ConfigureAwait(false);
        }

        private string GetMigrationErrorMessage(DXMigrationScriptsUnit file, Guid id)
        {
            return $"{file.ToString()}\nDXUnit with ID '{id}' migration error";
        }

        private static List<JToken> ParseBlocks(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<JToken>();

            var token = JToken.Parse(content);
            if (token is JArray array)
                return array.ToList();

            return new List<JToken> { token };
        }

        private static string? GetBlockKind(JToken token)
            => token["Meta"]?["Kind"]?.Value<string>();

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

                var deleteRefById = new Dictionary<Guid, DXDeleteRef>();

                if (deleteAction != null
                    && block.Meta?.Op?.Equals("Sync", StringComparison.OrdinalIgnoreCase) == true
                    && !string.IsNullOrWhiteSpace(block.Meta.DXFilter)
                    && !string.IsNullOrWhiteSpace(block.Meta.Type))
                {
                    var existingIds = new HashSet<Guid>(_coreRepo.GetItemIDs(block.Meta.Type, block.Meta.DXFilter));

                    var incomingIds = new HashSet<Guid>(
                        block.Data?.Items?.Where(r => r != null).Select(r => r.ID) ?? Enumerable.Empty<Guid>());

                    existingIds.ExceptWith(incomingIds);

                    foreach (var id in existingIds)
                    {
                        if (id == Guid.Empty)
                            continue;

                        deleteRefById.TryAdd(id, new DXDeleteRef { ID = id });
                    }
                }

                if (block.Data?.Items != null)
                {
                    foreach (var record in block.Data.Items)
                    {
                        if (record == null) continue;

                        var single = new DXDataBlock<DXUnitRecord>
                        {
                            Meta = block.Meta,
                            Data = new DXData<DXUnitRecord>
                            {
                                Items = new List<DXUnitRecord> { record }
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
                    foreach (var deleteRef in block.Data.Delete.Where(x => x != null))
                    {
                        if (deleteRef.ID == Guid.Empty)
                            continue;

                        deleteRefById[deleteRef.ID] = deleteRef;
                    }
                }

                if (deleteAction != null && deleteRefById.Count > 0)
                {
                    foreach (var deleteRef in deleteRefById.Values)
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

                if (block.Data?.Items == null)
                    continue;

                foreach (var record in block.Data.Items)
                {
                    if (record == null) continue;

                    var single = new DXDataBlock<DXEnumRecord>
                    {
                        Meta = block.Meta,
                        Data = new DXData<DXEnumRecord>
                        {
                            Items = new List<DXEnumRecord> { record }
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

                if (block.Data?.Items == null)
                    continue;

                foreach (var record in block.Data.Items)
                {
                    if (record == null) continue;

                    var single = new DXDataBlock<DXElementRecord>
                    {
                        Meta = block.Meta,
                        Data = new DXData<DXElementRecord>
                        {
                            Items = new List<DXElementRecord> { record }
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

        private async Task ProcessBlocksByKindAsync(
            DXMigrationScriptsUnit script,
            IEnumerable<JToken> blocks,
            Func<DXDataBlock<DXUnitRecord>, Task> unitUpsert,
            Func<DXDataBlock<DXUnitRecord>, Task>? unitDelete,
            Func<DXDataBlock<DXEnumRecord>, Task> enumUpsert,
            Func<DXDataBlock<DXElementRecord>, Task> elementUpsert,
            CancellationToken ct)
        {
            foreach (var token in blocks)
            {
                if (token == null)
                    continue;

                ct.ThrowIfCancellationRequested();

                var kind = GetBlockKind(token);
                if (string.IsNullOrWhiteSpace(kind))
                    throw new NotSupportedException($"Migration script '{script}' has block without Meta.Kind.");

                if (string.Equals(kind, "DXUnit", StringComparison.OrdinalIgnoreCase))
                {
                    var block = token.ToObject<DXDataBlock<DXUnitRecord>>();
                    if (block == null) continue;

                    await ProcessUnitBlocksAsync(
                        script,
                        new[] { block },
                        unitUpsert,
                        unitDelete,
                        ct).ConfigureAwait(false);
                    continue;
                }

                if (string.Equals(kind, "DXEnum", StringComparison.OrdinalIgnoreCase))
                {
                    var block = token.ToObject<DXDataBlock<DXEnumRecord>>();
                    if (block == null) continue;

                    await ProcessEnumBlocksAsync(
                        script,
                        new[] { block },
                        enumUpsert,
                        ct).ConfigureAwait(false);
                    continue;
                }

                if (string.Equals(kind, "DXElement", StringComparison.OrdinalIgnoreCase))
                {
                    var block = token.ToObject<DXDataBlock<DXElementRecord>>();
                    if (block == null) continue;

                    await ProcessElementBlocksAsync(
                        script,
                        new[] { block },
                        elementUpsert,
                        ct).ConfigureAwait(false);
                    continue;
                }

                throw new NotSupportedException($"Migration script '{script}' has unsupported block kind '{kind}'.");
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

