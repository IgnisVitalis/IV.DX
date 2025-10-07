using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace IV.DataProvider.Persistence.Common.UnitTests.Converters
{
    public class ModelConverterTests
    {
        private readonly MyObject esqlObject;
        private readonly ESQLModel esqlModel;

        public ModelConverterTests()
        {
            var objectId = new Guid("032169A1-FDFA-45B8-B0EC-381A6888FF35");

            this.esqlObject = new MyObject()
            {
                ID = objectId,
                TimeStamp = new DateTime(2020, 12, 1),
                MyBlockSingleItem = new MyBlock()
                {
                    ID = new Guid("B2669009-536A-4252-A920-CCEF4456A08A"),
                    ObjectID = objectId,
                    Date = new DateTime(2020, 12, 1),
                    Name = "Name1",
                    Value = 1,
                    TimeStamp = new DateTime(2020, 12, 1)
                },
                MyBlockMultiItems = new ESQLMultiItemsContainer<MyBlock>()
                {
                    Mode = ModeForMultiItems.Target,
                    Announced = new List<MyBlock>()
                    {
                        new MyBlock()
                        {
                            ID = new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                            ObjectID = objectId,
                            TimeStamp = new DateTime(2020, 12, 2),
                            Name = "Name2",
                            Value = 2,
                            Date = new DateTime(2020, 12, 2),
                        },
                        new MyBlock()
                        {
                            ID = new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                            ObjectID = objectId,
                            TimeStamp =new DateTime(2020, 12, 3),
                            Name = "Name3",
                            Value = 3,
                            Date = new DateTime(2020, 12, 3),
                        },
                    },
                    Deleted = new List<MyBlock>()
                    {
                        new MyBlock()
                        {
                            ID = new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                            ObjectID = objectId,
                            TimeStamp =  new DateTime(2020, 12, 4),
                            Name = "Name4",
                            Value = 4,
                            Date = new DateTime(2020, 12, 4),
                        }
                    },
                }
            };

            var ownItem = new ESQLMainItem(new DXUnitAttribute("MyObjectDefinition"))
            {
                Item = new ESQLItem()
                {
                    ID = objectId,
                    ObjectID = objectId,
                    Content = new JObject(new JProperty("TimeStamp", new DateTime(2020, 12, 1))),
                }
            };

            esqlModel = new ESQLModel(ownItem)
            {
                SingleItems = new List<ESQLSingleItem>()
                {
                    new ESQLSingleItem
                    {
                        Name = "MyBlockSingleItem",
                        BlockInfo = new DXElementAttribute("MyBlockDefinition"),
                        Item = new ESQLItem()
                        {
                            ID = new Guid("B2669009-536A-4252-A920-CCEF4456A08A"),
                            ObjectID = objectId,
                            Content = new JObject(
                                new JProperty("Name", "Name1"),
                                new JProperty("Value", 1),
                                new JProperty("Date", new DateTime(2020, 12, 1)),
                                new JProperty("ID", new Guid("B2669009-536A-4252-A920-CCEF4456A08A")),
                                new JProperty("ObjectID",  new Guid("032169a1-fdfa-45b8-b0ec-381a6888ff35")),
                                new JProperty("TimeStamp", new DateTime(2020, 12, 1)))
                        }
                    }
                },
                MultiItems = new List<ESQLMultiItem>()
                {
                    new ESQLMultiItem()
                    {
                        Name = "MyBlockMultiItems",
                        BlockInfo = new DXElementAttribute("MyBlockDefinition"),
                        Mode = ModeForMultiItems.Target,
                        Announced = new List<ESQLItem>()
                        {
                            new ESQLItem()
                            {
                                ID = new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                                ObjectID = objectId,
                                Content = new JObject(
                                    new JProperty("Name", "Name2"),
                                    new JProperty("Value", 2),
                                    new JProperty("Date", new DateTime(2020, 12, 2)),
                                    new JProperty("ID", new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A")),
                                    new JProperty("ObjectID",  new Guid("032169a1-fdfa-45b8-b0ec-381a6888ff35")),
                                    new JProperty("TimeStamp", new DateTime(2020, 12, 2))),
                            },
                            new ESQLItem()
                            {
                                ID = new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                                ObjectID = objectId,
                                Content = new JObject(
                                    new JProperty("Name", "Name3"),
                                    new JProperty("Value", 3),
                                    new JProperty("Date", new DateTime(2020, 12, 3)),
                                    new JProperty("ID", new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2")),
                                    new JProperty("ObjectID", new Guid("032169a1-fdfa-45b8-b0ec-381a6888ff35")),
                                    new JProperty("TimeStamp", new DateTime(2020, 12, 3)))
                            }
                        },
                        Deleted = new List<ESQLItem>()
                        {
                            new ESQLItem()
                            {
                                ID = new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                                ObjectID = objectId,
                                Content = new JObject(
                                    new JProperty("Name", "Name4"),
                                    new JProperty("Value", 4),
                                    new JProperty("Date", new DateTime(2020, 12, 4)),
                                    new JProperty("ID", new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6")),
                                    new JProperty("ObjectID",  new Guid("032169a1-fdfa-45b8-b0ec-381a6888ff35")),
                                    new JProperty("TimeStamp", new DateTime(2020, 12, 4)))
                            }
                        }
                    }
                }
            };
        }


        [Fact]
        public void ConvertToESQLModel_UsingStongType_CorrectESQLModel()
        {
            // Init                     

            // Action            
            var result = this.esqlObject.ConvertToESQLModel();

            // Checking result
            Assert.True(ESQLModel.DeepEquals(this.esqlModel, result));
        }

        [Fact]
        public void ConvertToESQLObject_UsingStongType_CorrectESQLObject()
        {
            // Init                     

            // Action            
            var esqlObjectResult = DXUnitHelper.CreateInstance<MyObject>(this.esqlModel);
            var result = this.esqlObject.ConvertToESQLModel();

            // Checking result
            Assert.True(ESQLModel.DeepEquals(this.esqlModel, result));
        }
    }

    [DXUnit("MyObjectDefinition")]
    internal class MyObject : ESQLObject
    {
        public MyBlock MyBlockSingleItem { get; set; }

        public ESQLMultiItemsContainer<MyBlock> MyBlockMultiItems { get; set; }
    }

    [DXElement("MyBlockDefinition")]
    internal class MyBlock : ESQLBlock
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Value")]
        public int Value { get; set; }
        [DXColumn("Date")]
        public DateTime Date { get; set; }
    }
}