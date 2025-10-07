using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Services.IntTests.DataServiceTests
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
            var ids = Enumerable.Range(0, 1000).Select(x => Guid.NewGuid()).ToList();

            var ids100 = ids.Take(100).ToList();

            var books = ids.Select(x =>
            {
                var id = Guid.NewGuid();

                return new TBookObject()
                {
                    ID = x,
                    TBookGenBlock = new TBookGenBlock()
                    {
                        ID = id,
                        Name = $"Name{id}"
                    },
                    TBookChapterBlock = new ESQLMultiItemsContainer<TBookChapterBlock>()
                    {
                        Announced = new List<TBookChapterBlock>()
                        {
                            new TBookChapterBlock()
                            {
                                ID = Guid.NewGuid(),
                                Number = 12345,
                                Text = "Seleucus VI Epiphanes (c. 115 – 94 BC) was a Seleucid monarch who reigned as King of Syria between 96 and 94 BC during the Hellenistic period. He was the son of Antiochus VIII and his Egyptian wife Tryphaena."
                            },
                            new TBookChapterBlock()
                            {
                                ID = Guid.NewGuid(),
                                Number = 9132423,
                                Text = "According to the ancient historian Appian, Seleucus VI was a violent ruler. He taxed his dominions extensively to support his wars, and resisted allowing the cities a measure of autonomy, as former kings allowed. His reign did not last long; in 94 BC, he was expelled from Antioch by Antiochus X, who followed him to the Cilician city of Mopsuestia, where his attempts to raise money led to riots that eventually claimed his life."
                            },
                            new TBookChapterBlock()
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
                var books = _dataService.GetItems<TBookObject>(ids100);
            }, $"GetItems({ids100.Count()})");

            EstimatePerformance(() =>
            {
                foreach (var id in ids100)
                {
                    var book = _dataService.GetItem<TBookObject>(id);
                }
            }, $"GetItem x {ids100.Count()}");

            EstimatePerformance(() =>
            {
                foreach (var book in books)
                {
                    _dataService.Delete(book);
                }
            }, $"Deleting x {ids.Count()}");

            Assert.Empty(_dataService.GetItems<TBookObject>(ids));
        }

        [Fact]
        public void GetItems_UsingDPBlockDescObject_ExistingBlocksWithAllInformation()
        {
            // Init

            // Action
            var blocks = _dataService.GetItems<DPBlockDescObject>();

            // Checking result
            Assert.NotEmpty(blocks);

            Assert.Equal(blocks.Count(), blocks.Where(x => x.DPObjectDescGenBlock != null).Count());
        }

        [Fact]
        public void GetItemNonParameterized_UsingDPBlockDescObject_ExistingBlockWithAllInformation()
        {
            // Init

            // Action           
            var block = _dataService.GetItem("DPBlockDescObject", new Guid("c5cf5513-9766-4cc6-84a0-b9a4717e36c2"), new EntityHandlerBaseContext());

            // Checking result
            Assert.NotNull(block);

            var genBlock = block.SingleItems.SingleOrDefault(x => x.Name == "DPObjectDescGenBlock");

            Assert.NotNull(genBlock);
        }

        [Fact]
        public void GetItems_UsingWhereExpression_CorrectValue()
        {
            // Init
            var objectID = new Guid("c60e25e6-2e6e-4d0b-8976-7b0aeb3d41d5");

            // Action
            var objs = _dataService.GetItems<DPEntityDescObject>($"ID = '{objectID}'", new EntityHandlerBaseContext());

            // Checking result
            Assert.Single(objs);

            var obj = objs.Single();

            Assert.Equal("DPEntityDescObject", obj.DPObjectDescGenBlock.Name);
        }

        [Fact]
        public void GetItems_UsingTypeName_CorrectValues()
        {
            // Init

            // Action
            var objs = _dataService.GetItems("DPEntityDescObject", new EntityHandlerBaseContext());

            // Checking result
            Assert.NotEmpty(objs);
        }
    }
}