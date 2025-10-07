using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DX.Contracts.Application;
using IV.DX.Contracts.Common.Converters;
using IV.DX.Contracts.Common.Helpers;
using IV.DX.Contracts.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Services.IntTests
{
    public class Test : IntTestController
    {
        public Test(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void Test2()
        {
            // Init
            IDataService dataService = this.ServiceProvider.GetService<IDataService>();

            var dataSource1 = new DataSource("TBookObject", new Guid("1b51edff-1d99-4043-9a69-209996729b69"));
            var dataSource2 = new DataSource("TUserObject", new Guid("60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff"));

            var subject = new DataSourceManager(dataService);

            subject.Attach(dataSource1);
            subject.Attach(dataSource2);

            // Action

            subject.Load();

            base.Output.WriteLine(dataSource1.Result.ConvertToJObject().ToString());
            base.Output.WriteLine(dataSource2.Result.ConvertToJObject().ToString());
        }

        [Fact]
        public void Test3()
        {
            // Init
            IDataService dataService = this.ServiceProvider.GetService<IDataService>();

            var entityMetadata = _dataService.GetItem("DPEntityDescObject", new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"), new EntityHandlerBaseContext()).ConvertToJObject().ToString();
            var objectMetadata = _dataService.GetItem("DPEntityDescObject", new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"), new EntityHandlerBaseContext()).ConvertToJObject().ToString();
            //var entityMetadata = await ESQLObjectApiClient.GetEntityAsync("DPEntityDescObject", new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"));
            //var objectMetadata = await ESQLObjectApiClient.GetEntityAsync("DPEntityDescObject", new Guid("2a30fc41-144d-45a8-b74a-e4ca528fc81c"));

            var item = _dataService.GetItem<DPEntityDescObject>(new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5"));
            // Action

        }

        [Fact]
        public void Test4()
        {
                       // Init
            IDataService dataService = this.ServiceProvider.GetService<IDataService>();
            IDataStructureRepository dataStructureRepository = this.ServiceProvider.GetService<IDataStructureRepository>();
            IEnumCoreRepository enumCoreRepository = this.ServiceProvider.GetService<IEnumCoreRepository>();

            var enumInfo = dataStructureRepository.GetEnum("DPObjectKindEnum");

            var blockDefinition = ModelConverter.GetESQLBlockDefinition(enumInfo);

            var enums = enumCoreRepository.Get(blockDefinition);
        }
    }

    public class DataSourceManager
    {
        public IList<DataSource> Subscribers { get; private set; }

        public IDataService DataService { get; private set; }

        public DataSourceManager(IDataService dataService)
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

        public void Load()
        {
            foreach (var subscriber in this.Subscribers)
            {
                subscriber.Load(this);
            }
        }
    }

    public class DataSource
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public ESQLModel Result { get; set; }

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

        public void Load(DataSourceManager dataSourceManager)
        {
            this.State = 1;

            this.Result = dataSourceManager.DataService.GetItem(this.Type, this.Id, new EntityHandlerBaseContext());

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
