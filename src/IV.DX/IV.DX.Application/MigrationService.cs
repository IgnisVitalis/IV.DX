using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Runtime;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IV.DX.Application
{
    internal class MigrationService : IDXMigrationService
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly Mutex mutex = new Mutex(false, "1e12dbb9-37ff-4e13-a9b1-5efa33cea05f");

        public MigrationService(
            IDXUnitGenericRepository genericRepo,
            IDXUnitDataService dataService)
        {
            this._genericRepo = genericRepo;
            this._dataService = dataService;
        }

        public void LoadStructure(string path)
        {
            mutex.WaitOne();
            this.Load(path);
            mutex.ReleaseMutex();
        }

        private void Load(string path)
        {
            var scriptsHistory = this.GetScriptsHistoryIfExisting();

            var coreFiles = File.ReadAllText(this.GetFullPath(path));

            var scripts = GetMirgationScripts(coreFiles);

            foreach (var migrationScript in scripts)
            {
                try
                {
                    if (scriptsHistory.SingleOrDefault(x => x.Equals(migrationScript.DXMigrationScriptsMainElement)) != null)
                        continue;

                    if (migrationScript.DXMigrationScriptsMainElement.Extention == "dat")
                    {
                        this.ProcessFileToInsertOrUpdate(migrationScript);
                    }
                    else if (migrationScript.DXMigrationScriptsMainElement.Extention == "dd")
                    {
                        this.ProcessFileToInsert(migrationScript);
                    }
                    else if (migrationScript.DXMigrationScriptsMainElement.Extention == "rd")
                    {
                        this.ProcessFileToInsert(migrationScript);
                    }
                    else if (migrationScript.DXMigrationScriptsMainElement.Extention == "od")
                    {
                        this.ProcessFileToInsert(migrationScript);
                    }
                    else
                    {
                        throw new Exception($"Migration script {migrationScript} could be processed.");
                    }

                    this._dataService.InsertAsync(migrationScript, new DXUnitHandlerMigrationServiceContext(migrationScript)).Wait();
                }
                catch (Exception exc)
                {
                    throw new Exception($"Error is occured when migration script '{migrationScript}' was applied.", exc);
                }
            }
        }

        public void LoadCoreStructure()
        {
            mutex.WaitOne();

            IEnumerable<DXMigrationScriptsMainElement> scriptsHistory;

            try
            {
                scriptsHistory = this.GetScriptsHistoryIfExisting();
            }
            catch (Exception)
            {
                var corePreInitFiles = File.ReadAllText(this.GetFullPath("MigrationScripts/CorePreInit.json"));
                var scriptsPreInit = GetMirgationScripts(corePreInitFiles);

                foreach (var script in scriptsPreInit)
                {
                    try
                    {
                        this.ProcessFileForPreInitCore(script);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception($"Error is occured when Pre Init migration script '{script}' was applied.", exc);
                    }
                }

                var corePostInitFiles = File.ReadAllText(this.GetFullPath("MigrationScripts/CorePostInit.json"));

                var scriptsPostInit = GetMirgationScripts(corePostInitFiles);

                foreach (var script in scriptsPostInit)
                {
                    try
                    {
                        this.ProcessFileForPostInitCore(script);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception($"Error is occured when Post Init migration script '{script}' was applied.", exc);
                    }
                }

                foreach (var script in scriptsPreInit)
                {
                    this._dataService.InsertAsync(script, new DXUnitHandlerMigrationServiceContext(script)).Wait();
                }
            }

            mutex.ReleaseMutex();
        }

        private IEnumerable<DXMigrationScriptsMainElement> GetScriptsHistoryIfExisting()
        {
            var result = this._genericRepo.GetDXUnits<DXMigrationScriptsUnit>().Select(x => x.DXMigrationScriptsMainElement);

            return result;
        }

        private IEnumerable<DXMigrationScriptsUnit> GetMirgationScripts(string path)
        {
            var jArray = JArray.Parse(path);

            var pattern = @"(?<Version>[0-9]+)_(?<Build>[0-9]+)_(?<Number>[0-9]+)_(?<Application>[a-zA-z]+)_(?<Name>[a-zA-z]+)\.(?<Extention>[a-z]+)";
            Regex regex = new Regex(pattern);

            var migrationScripts = jArray.Select(x => new FileInfo(this.GetFullPath($"MigrationScripts/{x}"))).Select(x =>
            {
                if (!regex.IsMatch(x.Name))
                    throw new Exception($"Script name {x.Name} has wrong format. Please use this format '<Verion>_<Build>_<Number>_<Application>_<Name>.<Extention>'");

                var match = regex.Match(x.Name);

                var id = Guid.NewGuid();

                return new DXMigrationScriptsUnit()
                {
                    ID = id,
                    DXMigrationScriptsMainElement = new DXMigrationScriptsMainElement()
                    {
                        ID = Guid.NewGuid(),
                        ObjectID = id,
                        FilePath = x.FullName,
                        Name = match.Groups["Name"].Value,
                        Version = match.Groups["Version"].Value,
                        Build = match.Groups["Build"].Value,
                        Number = match.Groups["Number"].Value,
                        AppName = match.Groups["Application"].Value,
                        Extention = match.Groups["Extention"].Value
                    }
                };
            }).ToList();

            return migrationScripts;
        }

        private void ProcessFileForPreInitCore(DXMigrationScriptsUnit file)
        {
            var content = File.ReadAllText(file.DXMigrationScriptsMainElement.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.InsertAsync(item, new DXUnitHandlerPreInitCoreContext(file)).Wait();
            }
        }

        private void ProcessFileForPostInitCore(DXMigrationScriptsUnit file)
        {
            var content = File.ReadAllText(file.DXMigrationScriptsMainElement.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.InsertAsync(item, new DXUnitHandlerPostInitCoreContext(file)).Wait();
            }
        }

        private void ProcessFileToInsert(DXMigrationScriptsUnit relFile)
        {
            var content = File.ReadAllText(relFile.DXMigrationScriptsMainElement.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.InsertAsync(item, new DXUnitHandlerMigrationServiceContext(relFile)).Wait();
            }
        }

        private void ProcessFileToInsertOrUpdate(DXMigrationScriptsUnit datFile)
        {
            var content = File.ReadAllText(datFile.DXMigrationScriptsMainElement.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.InsertOrUpdateAsync(item, new DXUnitHandlerMigrationServiceContext(datFile)).Wait();
            }
        }

        private string GetFullPath(string pathToFile)
        {
            if (Path.IsPathRooted(pathToFile))
                return pathToFile;

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathToFile);
        }
    }
}