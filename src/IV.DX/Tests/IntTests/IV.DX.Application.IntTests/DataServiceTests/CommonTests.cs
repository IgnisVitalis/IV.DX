using IV.DX.Shared.IntTests.Models.Test;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using IV.DX.Shared.IntTests;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.DataServiceTests
{
    public class CommonTests : IntTestController
    {
        public CommonTests(ITestOutputHelper output)
            : base(output)
        {

        }

        [Fact]
        public void CheckPerformance()
        {
            var ids = Enumerable.Range(0, 100).Select(x => Guid.NewGuid()).ToList();

            var ids100 = ids.Take(100).ToList();

            var books = ids.Select(x =>
            {
                var id = Guid.NewGuid();

                return new TBookUnit()
                {
                    ID = x,
                    TBookMainElement = new TBookMainElement()
                    {
                        ID = id,
                        Name = $"Name{id}"
                    },
                    TBookChapterElement = new DXMultiElementsContainer<TBookChapterElement>()
                    {
                        Announced = new List<TBookChapterElement>()
                        {
                            new TBookChapterElement()
                            {
                                ID = Guid.NewGuid(),
                                Number = 12345,
                                Text = "Seleucus VI Epiphanes (c. 115 – 94 BC) was a Seleucid monarch who reigned as King of Syria between 96 and 94 BC during the Hellenistic period. He was the son of Antiochus VIII and his Egyptian wife Tryphaena."
                            },
                            new TBookChapterElement()
                            {
                                ID = Guid.NewGuid(),
                                Number = 9132423,
                                Text = "According to the ancient historian Appian, Seleucus VI was a violent ruler. He taxed his dominions extensively to support his wars, and resisted allowing the cities a measure of autonomy, as former kings allowed. His reign did not last long; in 94 BC, he was expelled from Antioch by Antiochus X, who followed him to the Cilician city of Mopsuestia, where his attempts to raise money led to riots that eventually claimed his life."
                            },
                            new TBookChapterElement()
                            {
                                ID = Guid.NewGuid(),
                                Number = 42543,
                                Text = "A period of civil war between his father and his uncle Antiochus IX ended in 96 BC when his father was assassinated. Antiochus IX then occupied the capital Antioch while Seleucus VI established his power base in western Cilicia. After his uncle was killed, Seleucus VI became the master of the capital but shared Syria with his brother Demetrius III and his cousin Antiochus X."
                            }
                        }
                    }
                };
            });

            EstimatePerformance(() =>
            {
                foreach (var book in books)
                {
                    _dataService.Insert(book);
                }
            }, $"Inserting x {ids.Count()}");

            EstimatePerformance(() =>
            {
                var books = _dataService.GetItems<TBookUnit>(ids100);
            }, $"GetItems({ids100.Count()})");

            EstimatePerformance(() =>
            {
                foreach (var id in ids100)
                {
                    var book = _dataService.GetItem<TBookUnit>(id);
                }
            }, $"GetItem x {ids100.Count()}");

            EstimatePerformance(() =>
            {
                foreach (var book in books)
                {
                    _dataService.Delete(book);
                }
            }, $"Deleting x {ids.Count()}");

            Assert.Empty(_dataService.GetItems<TBookUnit>(ids));
        }

        [Fact]
        public void GetItems_UsingDXElementDefinitionUnit_ExistingBlocksWithAllInformation()
        {
            // Init

            // Action
            var blocks = _dataService.GetItems<DXElementDefinitionUnit>();

            // Checking result
            Assert.NotEmpty(blocks);

            Assert.Equal(blocks.Count(), blocks.Where(x => x.DXUnitDefinitionMainElement != null).Count());
        }

        [Fact]
        public void GetItemNonParameterized_UsingDXElementDefinitionUnit_ExistingBlockWithAllInformation()
        {
            // Init

            // Action           
            var block = _dataService.GetItem("DXElementDefinitionUnit", new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2"), new DXUnitHandlerBaseContextOld());

            // Checking result
            Assert.NotNull(block);

            var genBlock = block.SingleItems.SingleOrDefault(x => x.Name == "DXUnitDefinitionMainElement");

            Assert.NotNull(genBlock);
        }

        [Fact]
        public void GetItems_UsingWhereExpression_CorrectValue()
        {
            // Init
            var objectID = new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5");

            // Action
            var objs = _dataService.GetItems<DXUnitDefinitionUnit>($"ID = '{objectID}'", new DXUnitHandlerBaseContextOld());

            // Checking result
            Assert.Single(objs);

            var obj = objs.Single();

            Assert.Equal("DXUnitDefinitionUnit", obj.DXUnitDefinitionMainElement.Name);
        }

        [Fact]
        public void GetItems_UsingTypeName_CorrectValues()
        {
            // Init

            // Action
            var objs = _dataService.GetItems("DXUnitDefinitionUnit", new DXUnitHandlerBaseContextOld());

            // Checking result
            Assert.NotEmpty(objs);
        }
    }
}