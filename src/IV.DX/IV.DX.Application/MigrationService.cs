using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IV.DX.Application
{
    public class MigrationService : IMigrationService
    {
        private readonly IDataService _dataService;
        private readonly IGenericRepository _genericRepo;
        private readonly Mutex mutex = new Mutex(false, "1e12dbb9-37ff-4e13-a9b1-5efa33cea05f");

        public MigrationService(
            IGenericRepository genericRepo,
            IDataService dataService)
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
                    if (scriptsHistory.SingleOrDefault(x => x.Equals(migrationScript.DPMigrationScriptsGenBlock)) != null)
                        continue;

                    if (migrationScript.DPMigrationScriptsGenBlock.Extention == "dat")
                    {
                        this.ProcessFileToInsertOrUpdate(migrationScript);
                    }
                    else if (migrationScript.DPMigrationScriptsGenBlock.Extention == "dd")
                    {
                        this.ProcessFileToInsert(migrationScript);
                    }
                    else if (migrationScript.DPMigrationScriptsGenBlock.Extention == "rd")
                    {
                        this.ProcessFileToInsert(migrationScript);
                    }
                    else if (migrationScript.DPMigrationScriptsGenBlock.Extention == "od")
                    {
                        this.ProcessFileToInsert(migrationScript);
                    }
                    else
                    {
                        throw new Exception($"Migration script {migrationScript} could be processed.");
                    }

                    this._dataService.Insert(migrationScript, new EntityHandlerMigrationServiceContext(migrationScript));
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

            IEnumerable<DPMigrationScriptsGenBlock> scriptsHistory;

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
                    this.ProcessFileForPreInitCore(script);
                }

                var corePostInitFiles = File.ReadAllText(this.GetFullPath("MigrationScripts/CorePostInit.json"));

                var scriptsPostInit = GetMirgationScripts(corePostInitFiles);

                foreach (var script in scriptsPostInit)
                {
                    this.ProcessFileForPostInitCore(script);
                }

                foreach (var script in scriptsPreInit)
                {
                    this._dataService.Insert(script, new EntityHandlerMigrationServiceContext(script));
                }
            }

            mutex.ReleaseMutex();
        }

        private IEnumerable<DPMigrationScriptsGenBlock> GetScriptsHistoryIfExisting()
        {
            var result = this._genericRepo.GetItems<DPMigrationScriptsObject>().Select(x => x.DPMigrationScriptsGenBlock);

            return result;
        }

        private IEnumerable<DPMigrationScriptsObject> GetMirgationScripts(string path)
        {
            var jArray = JArray.Parse(path);

            var pattern = @"(?<Version>[0-9]+)_(?<Build>[0-9]+)_(?<Number>[0-9]+)_(?<Application>[a-zA-z]+)_(?<Name>[a-zA-z]+)\.(?<Extention>[a-z]+)";
            Regex regex = new Regex(pattern);

            var migrationScripts = jArray.Select(x => new FileInfo(this.GetFullPath($"MigrationScripts/{x}"))).Select(x =>
            {
                if (!regex.IsMatch(x.Name))
                    throw new Exception($"Script name {x.Name} has wrong format. Please use this format '<Verion>_<Build>_<Number>_<Application>_<Name>.<Extention>'");

                var match = regex.Match(x.Name);

                return new DPMigrationScriptsObject()
                {
                    ID = Guid.NewGuid(),
                    DPMigrationScriptsGenBlock = new DPMigrationScriptsGenBlock()
                    {
                        ID = Guid.NewGuid(),
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

        private void ProcessFileForPreInitCore(DPMigrationScriptsObject file)
        {
            var content = File.ReadAllText(file.DPMigrationScriptsGenBlock.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.Insert(item.ToString(), new EntityHandlerPreInitCoreContext(file));
            }
        }

        private void ProcessFileForPostInitCore(DPMigrationScriptsObject file)
        {
            var content = File.ReadAllText(file.DPMigrationScriptsGenBlock.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.Insert(item.ToString(), new EntityHandlerPostInitCoreContext(file));
            }
        }

        private void ProcessFileToInsert(DPMigrationScriptsObject relFile)
        {
            var content = File.ReadAllText(relFile.DPMigrationScriptsGenBlock.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.Insert(item.ToString(), new EntityHandlerMigrationServiceContext(relFile));
            }
        }

        private void ProcessFileToInsertOrUpdate(DPMigrationScriptsObject datFile)
        {
            var content = File.ReadAllText(datFile.DPMigrationScriptsGenBlock.FilePath);

            var jarray = JArray.Parse(content);

            foreach (JObject item in jarray)
            {
                this._dataService.InsertOrUpdate(item.ToString(), new EntityHandlerMigrationServiceContext(datFile));
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