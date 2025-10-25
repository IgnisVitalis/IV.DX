using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class ModelConverterTests
    {
        private readonly MyObject dxUnitect;
        private readonly DXModel dxModel;

        public ModelConverterTests()
        {
            var objectId = new Guid("032169A1-FDFA-45B8-B0EC-381A6888FF35");

            this.dxUnitect = new MyObject()
            {
                ID = objectId,
                TimeStamp = new DateTime(2020, 12, 1),
                MyDXElementSingleItem = new MyDXElement()
                {
                    ID = new Guid("B2669009-536A-4252-A920-CCEF4456A08A"),
                    DXUnitID = objectId,
                    Date = new DateTime(2020, 12, 1),
                    Name = "Name1",
                    Value = 1,
                    TimeStamp = new DateTime(2020, 12, 1)
                },
                MyDXElementMultiItems = new DXMultiElementsContainer<MyDXElement>()
                {
                    Mode = MultiElementsMode.Target,
                    Announced = new HashSet<MyDXElement>()
                    {
                        new MyDXElement()
                        {
                            ID = new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                            DXUnitID = objectId,
                            TimeStamp = new DateTime(2020, 12, 2),
                            Name = "Name2",
                            Value = 2,
                            Date = new DateTime(2020, 12, 2),
                        },
                        new MyDXElement()
                        {
                            ID = new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                            DXUnitID = objectId,
                            TimeStamp =new DateTime(2020, 12, 3),
                            Name = "Name3",
                            Value = 3,
                            Date = new DateTime(2020, 12, 3),
                        },
                    },
                    Deleted = new HashSet<MyDXElement>()
                    {
                        new MyDXElement()
                        {
                            ID = new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                            DXUnitID = objectId,
                            TimeStamp =  new DateTime(2020, 12, 4),
                            Name = "Name4",
                            Value = 4,
                            Date = new DateTime(2020, 12, 4),
                        }
                    },
                }
            };

            var ownItem = new DXMainElement(new DXUnitAttribute("MyObjectDefinition"))
            {
                Item = new DXItem()
                {
                    ID = objectId,
                    DXUnitID = objectId,
                    Content = new JObject(new JProperty("TimeStamp", new DateTime(2020, 12, 1))),
                }
            };

            dxModel = new DXModel(ownItem)
            {
                DXSingleElements = new HashSet<DXSingleElement>()
                {
                    new DXSingleElement
                    {
                        Name = "MyDXElementSingleItem",
                        ElementInfo = new DXElementAttribute("MyDXElementDefinition"),
                        Item = new DXItem()
                        {
                            ID = new Guid("B2669009-536A-4252-A920-CCEF4456A08A"),
                            DXUnitID = objectId,
                            Content = new JObject(
                                new JProperty("Name", "Name1"),
                                new JProperty("Value", 1),
                                new JProperty("Date", new DateTime(2020, 12, 1)),
                                new JProperty("ID", new Guid("B2669009-536A-4252-A920-CCEF4456A08A")),
                                new JProperty("DXUnitID",  objectId),
                                new JProperty("TimeStamp", new DateTime(2020, 12, 1)))
                        }
                    }
                },
                DXMultiElements = new HashSet<DXMultiElement>()
                {
                    new DXMultiElement()
                    {
                        Name = "MyDXElementMultiItems",
                        DXElementInfo = new DXElementAttribute("MyDXElementDefinition"),
                        Mode = MultiElementsMode.Target,
                        Announced = new HashSet<DXItem>()
                        {
                            new DXItem()
                            {
                                ID = new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                                DXUnitID = objectId,
                                Content = new JObject(
                                    new JProperty("Name", "Name2"),
                                    new JProperty("Value", 2),
                                    new JProperty("Date", new DateTime(2020, 12, 2)),
                                    new JProperty("ID", new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A")),
                                    new JProperty("DXUnitID",  objectId),
                                    new JProperty("TimeStamp", new DateTime(2020, 12, 2))),
                            },
                            new DXItem()
                            {
                                ID = new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                                DXUnitID = objectId,
                                Content = new JObject(
                                    new JProperty("Name", "Name3"),
                                    new JProperty("Value", 3),
                                    new JProperty("Date", new DateTime(2020, 12, 3)),
                                    new JProperty("ID", new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2")),
                                    new JProperty("DXUnitID", objectId),
                                    new JProperty("TimeStamp", new DateTime(2020, 12, 3)))
                            }
                        },
                        Deleted = new HashSet<DXItem>()
                        {
                            new DXItem()
                            {
                                ID = new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                                DXUnitID = objectId,
                                Content = new JObject(
                                    new JProperty("Name", "Name4"),
                                    new JProperty("Value", 4),
                                    new JProperty("Date", new DateTime(2020, 12, 4)),
                                    new JProperty("ID", new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6")),
                                    new JProperty("DXUnitID", objectId),
                                    new JProperty("TimeStamp", new DateTime(2020, 12, 4)))
                            }
                        }
                    }
                }
            };
        }


        [Fact]
        public void ConvertToDXModel_UsingStongType_CorrectDXModel()
        {
            // Init                     

            // Action            
            var result = this.dxUnitect.ConvertToDXModel();

            // Checking result
            Assert.True(DXModel.DeepEquals(this.dxModel, result));
        }

        [Fact]
        public void ConvertTodxUnitect_UsingStongType_CorrectdxUnitect()
        {
            // Init                     

            // Action            
            var dxUnitectResult = DXUnitHelper.CreateInstance<MyObject>(this.dxModel);
            var result = this.dxUnitect.ConvertToDXModel();

            // Checking result
            Assert.True(DXModel.DeepEquals(this.dxModel, result));
        }
    }

    [DXUnit("MyObjectDefinition")]
    internal class MyObject : DXUnit
    {
        public MyDXElement MyDXElementSingleItem { get; set; }

        public DXMultiElementsContainer<MyDXElement> MyDXElementMultiItems { get; set; }
    }

    [DXElement("MyDXElementDefinition")]
    internal class MyDXElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; }
        [DXColumn("Value")]
        public int Value { get; set; }
        [DXColumn("Date")]
        public DateTime Date { get; set; }
    }
}