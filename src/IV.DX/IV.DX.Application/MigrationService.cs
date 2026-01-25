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
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitGenericRepository _genericRepo;

        private readonly SemaphoreSlim _lock = new(1, 1);

        private static readonly Regex ScriptNameRegex = new(
            @"^(?<Version>\d+)_(?<Build>\d+)_(?<Number>\d+)_(?<Application>[A-Za-z0-9]+)_(?<Name>[A-Za-z0-9]+)\.(?<Extension>[A-Za-z0-9]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public MigrationService(
            IDXUnitGenericRepository genericRepo,
            IDXUnitDataService dataService)
        {
            _genericRepo = genericRepo ?? throw new ArgumentNullException(nameof(genericRepo));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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
            var ext = script.Extention?.ToLowerInvariant();
            switch (ext)
            {
                case "ann":   // insert or update
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
                    Extention = meta.Extension,
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
                                   $"Script name '{fi.Name}' has wrong format. Expected '<Version>_<Build>_<Number>_<Application>_<Name>.<Extension>'.");

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
                               Extention = meta.Extension,
                               Content = File.ReadAllText(fi.FullName)
                           };
                       })
                       .ToList();
        }

        private async Task ProcessFileForPreInitCoreAsync(DXMigrationScriptsUnit file, CancellationToken ct)
        {
            var jarray = JArray.Parse(file.Content);
            foreach (JObject item in jarray)
            {
                try
                {
                    await _dataService.InsertAsync(item, new DXUnitHandlerPreInitCoreContext(file), ct).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw new Exception(this.GetMigrationErrorMessage(file, item), exc);
                }
            }
        }

        private async Task ProcessFileForPostInitCoreAsync(DXMigrationScriptsUnit file, CancellationToken ct)
        {
            var jarray = JArray.Parse(file.Content);
            foreach (JObject item in jarray)
            {
                try
                {
                    await _dataService.InsertAsync(item, new DXUnitHandlerPostInitCoreContext(file), ct).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw new Exception(this.GetMigrationErrorMessage(file, item), exc);
                }
            }
        }

        private async Task ProcessFileToInsertOrUpdateAsync(DXMigrationScriptsUnit file, CancellationToken ct)
        {
            var jarray = JArray.Parse(file.Content);
            foreach (JObject item in jarray)
            {
                try
                {
                    await _dataService.InsertOrUpdateAsync(item, new DXUnitHandlerMigrationServiceContext(file), ct).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    throw new Exception(this.GetMigrationErrorMessage(file, item), exc);
                }
            }
        }

        private string GetMigrationErrorMessage(DXMigrationScriptsUnit file, JObject item)
        {
            return $"{file.ToString()}\nDXUnit with ID '{item["ID"]}' migration error";
        }

        private static string NormalizeDirectory(string? dir)
            => string.IsNullOrWhiteSpace(dir) ? string.Empty : dir.Replace('\\', '/');

        private static string GetFullPath(string path)
            => Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }
}