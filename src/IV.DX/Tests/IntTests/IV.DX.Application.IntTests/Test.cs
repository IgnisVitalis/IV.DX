using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests
{
    [Collection("DX:one-time")]
    public class Test : IntTestController
    {
        IDXUnitDataService _dataService;

        public Test(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();
        }

        [Fact]
        public async Task Test2()
        {
            // Init
            IDXUnitDataService dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();

            var dataSource1 = new DataSource("TBookUnit", new Guid("1b51edff-1d99-4043-9a69-209996729b69"));
            var dataSource2 = new DataSource("TUserUnit", new Guid("60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff"));

            var subject = new DataSourceManager(dataService);

            subject.Attach(dataSource1);
            subject.Attach(dataSource2);

            // Action

            await subject.Load();

            base.Output.WriteLine(dataSource1.Result.ToString());
            base.Output.WriteLine(dataSource2.Result.ToString());
        }

        [Fact]
        public async Task Test3()
        {
            // Init
            IDXUnitDataService dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();

            var dxUnitMetadata = await _dataService.GetItemAsync("DXUnitDefinitionUnit", new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"));
            var objectMetadata = await _dataService.GetItemAsync("DXUnitDefinitionUnit", new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"));
            //var dxUnitMetadata = await ESQLObjectApiClient.GetEntityAsync("DXUnitDefinitionUnit", new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"));
            //var objectMetadata = await ESQLObjectApiClient.GetEntityAsync("DXUnitDefinitionUnit", new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"));

            var item = await _dataService.GetItemAsync<DXUnitDefinitionUnit>(new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"));
            // Action

        }

        [Fact]
        public void Test4()
        {
            // Init
            IDXUnitDataService dataService = this.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            IDXStructureRepository dataStructureRepository = this.ServiceProvider.GetRequiredService<IDXStructureRepository>();
            IDXEnumCoreRepository enumCoreRepository = this.ServiceProvider.GetRequiredService<IDXEnumCoreRepository>();

            var enumInfo = dataStructureRepository.GetEnum("DXObjectKindEnum");

            var blockDefinition = DXModelDefinitionHelper.GetDXElementDefinition(enumInfo);

            var enums = enumCoreRepository.Get(blockDefinition);
        }
    }

    public class DataSourceManager
    {
        public IList<DataSource> Subscribers { get; private set; }

        public IDXUnitDataService DataService { get; private set; }

        public DataSourceManager(IDXUnitDataService dataService)
        {
            this.Subscribers = new List<DataSource>();
            this.DataService = dataService;
        }

        public void Attach(DataSource dataSource)
        {
            this.Subscribers.Add(dataSource);
        }

        public void Detach(DataSource dataSource)
        {
            this.Subscribers.Remove(dataSource);
        }

        public async Task Load()
        {
            foreach (var subscriber in this.Subscribers)
            {
                await subscriber.LoadAsync(this);
            }
        }
    }

    public class DataSource
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public JObject Result { get; set; }

        // -1 - Error occured
        // 0 - Init
        // 1 - Loading
        // 2 - Loaded

        public int State { get; private set; }

        public string Name { get; private set; }

        public DataSource(string type, Guid id)
        {
            this.Id = id;
            this.Type = type;
        }

        public async Task LoadAsync(DataSourceManager dataSourceManager)
        {
            this.State = 1;

            this.Result = await dataSourceManager.DataService.GetItemAsync(this.Type, this.Id);

            this.State = 2;
        }
    }

    public class Model
    {
        public IEnumerable<SingleDataBlock> SingleDataBlocks { get; set; }
        public IEnumerable<MultiDataBlock> MultiDataBlocks { get; set; }
    }

    public class SingleDataBlock
    {
        public DataBlock DataBlock { get; set; }
    }

    public class MultiDataBlock
    {
        public int Mode { get; set; }
        public IEnumerable<DataBlock> AnnouncedDataBlocks { get; set; }
        public IEnumerable<DataBlock> DestroyedDataBlocks { get; set; }
    }

    public class DataBlock
    {
        public Guid ID { get; set; }

        public IEnumerable<Property> Properties { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string CurrentValue { get; set; }
        public string PreviousValue { get; set; }

        public Property(JProperty jProperty)
        {
            this.Name = jProperty.Name;
            this.CurrentValue = jProperty.Value.ToString();
            this.PreviousValue = this.CurrentValue;
        }


    }
}
