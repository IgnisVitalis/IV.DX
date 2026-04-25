using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.DataServiceTests
{
    [Collection("DX:one-time")]
    public class CommonTests : IntTestController
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXUnitDataReader _dataReader;

        public CommonTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            _dataService = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _dataReader = base.ServiceProvider.GetRequiredService<IDXUnitDataReader>();
        }

        [Fact]
        public async Task CheckPerformance()
        {
            var ids = Enumerable.Range(0, 1000).Select(x => Guid.NewGuid()).ToList();

            var ids100 = ids.Take(100).ToList();

            var books = ids.Select(x =>
            {
                var id = Guid.NewGuid();

                return new TBookUnit()
                {
                    Id = x,
                    TBookMainElement = new TBookMainElement()
                    {
                        Id = id,
                        DXUnitId = x,
                        Name = $"Name{id}"
                    },
                    TBookChapterElement = new DXMultiElementsContainer<TBookChapterElement>()
                    {
                        Announced = new HashSet<TBookChapterElement>()
                        {
                            new TBookChapterElement()
                            {
                                Id = Guid.NewGuid(),
                                DXUnitId = x,
                                Number = 12345,
                                Text = "Seleucus VI Epiphanes (c. 115 – 94 BC) was a Seleucid monarch who reigned as King of Syria between 96 and 94 BC during the Hellenistic period. He was the son of Antiochus VIII and his Egyptian wife Tryphaena."
                            },
                            new TBookChapterElement()
                            {
                                Id = Guid.NewGuid(),
                                DXUnitId = x,
                                Number = 9132423,
                                Text = "According to the ancient historian Appian, Seleucus VI was a violent ruler. He taxed his dominions extensively to support his wars, and resisted allowing the cities a measure of autonomy, as former kings allowed. His reign did not last long; in 94 BC, he was expelled from Antioch by Antiochus X, who followed him to the Cilician city of Mopsuestia, where his attempts to raise money led to riots that eventually claimed his life."
                            },
                            new TBookChapterElement()
                            {
                                Id = Guid.NewGuid(),
                                DXUnitId = x,
                                Number = 42543,
                                Text = "A period of civil war between his father and his uncle Antiochus IX ended in 96 BC when his father was assassinated. Antiochus IX then occupied the capital Antioch while Seleucus VI established his power base in western Cilicia. After his uncle was killed, Seleucus VI became the master of the capital but shared Syria with his brother Demetrius III and his cousin Antiochus X."
                            }
                        }
                    }
                };
            });

            await EstimatePerformanceAsync(async () =>
            {
                foreach (var book in books)
                {
                    await _dataService.InsertAsync(book);
                }
            }, $"Inserting x {ids.Count()}");

            await EstimatePerformanceAsync(async () =>
            {
                var books = await _dataReader.GetItemsAsync<TBookUnit>(ids100);
            }, $"GetItems({ids100.Count()})");

            await EstimatePerformanceAsync(async () =>
            {
                foreach (var id in ids100)
                {
                    var book = await _dataReader.GetItemAsync<TBookUnit>(id);
                }
            }, $"GetItem x {ids100.Count()}");

            await EstimatePerformanceAsync(async () =>
            {
                foreach (var book in books)
                {
                    await _dataService.DeleteAsync(book);
                }
            }, $"Deleting x {ids.Count()}");

            Assert.Empty(await _dataReader.GetItemsAsync<TBookUnit>(ids));
        }

        [Fact]
        public async Task GetItems_UsingDXElementDefinitionUnit_ExistingDXElementsWithAllInformation()
        {
            // Init

            // Action
            var dxElements = await _dataReader.GetItemsAsync<DXElementDefinitionUnit>();

            // Checking result
            Assert.NotEmpty(dxElements);

            Assert.Equal(dxElements.Count(), dxElements.Where(x => x != null).Count());
        }

        [Fact]
        public async Task GetItemNonParameterized_UsingDXElementDefinitionUnit_ExistingDXElementWithAllInformation()
        {
            // Init

            // Action           
            var dxElementJObject = await _dataReader.GetItemAsync("DXElementDefinitionUnit", new Guid("ce754889-4efb-4281-ad1f-14d710b30007"));

            var block = dxElementJObject.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = block?.Data?.Items?.SingleOrDefault();

            // Checking result
            Assert.NotNull(record);
        }

        [Fact]
        public async Task GetItems_UsingWhereExpression_CorrectValue()
        {
            // Init
            var objectId = new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5");

            // Action
            var objs = await _dataReader.GetItemsAsync<DXUnitDefinitionUnit>($"Id = '{objectId}'");

            // Checking result
            Assert.Single(objs);

            var obj = objs.Single();

            Assert.Equal("DXUnitDefinitionUnit", obj.Name);
        }

        [Fact]
        public async Task GetItems_UsingTypeName_CorrectValues()
        {
            // Init

            // Action
            var objs = await _dataReader.GetItemsAsync("DXUnitDefinitionUnit");

            // Checking result
            Assert.NotEmpty(objs);
        }
    }
}

