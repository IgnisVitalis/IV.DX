using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class DXUnitHelperTests
    {
        private readonly MyObject dxUnit;
        private readonly DXModel dxModel;

        public DXUnitHelperTests()
        {
            var objectId = new Guid("032169A1-FDFA-45B8-B0EC-381A6888FF35");

            this.dxUnit = new MyObject()
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
                    Content = new JObject(
                        new JProperty(Constants.SystemPropertyTypeName, "MyObjectDefinition"),
                        new JProperty(Constants.ID, objectId),
                        new JProperty(Constants.TimeStamp, new DateTime(2020, 12, 1))),
                }
            };

            dxModel = new DXModel(ownItem)
            {
                DXSingleElements = new HashSet<DXSingleElement>()
                {
                    new DXSingleElement
                    {
                        Name = "MyDXElementSingleItem",
                        Attribute = new DXElementAttribute("MyDXElementDefinition"),
                        Item = new DXItem()
                        {
                            ID = new Guid("B2669009-536A-4252-A920-CCEF4456A08A"),
                            DXUnitID = objectId,
                            Content = new JObject(
                                new JProperty(Constants.SystemPropertyTypeName, "MyDXElementDefinition"),
                                new JProperty(Constants.ID, new Guid("B2669009-536A-4252-A920-CCEF4456A08A")),
                                new JProperty(Constants.DXUnitID,  objectId),
                                new JProperty(Constants.TimeStamp, new DateTime(2020, 12, 1)),
                                new JProperty("Name", "Name1"),
                                new JProperty("Value", 1),
                                new JProperty("Date", new DateTime(2020, 12, 1)))
                        }
                    }
                },
                DXMultiElements = new HashSet<DXMultiElement>()
                {
                    new DXMultiElement()
                    {
                        Name = "MyDXElementMultiItems",
                        Attribute = new DXElementAttribute("MyDXElementDefinition"),
                        Mode = MultiElementsMode.Target,
                        Announced = new HashSet<DXItem>()
                        {
                            new DXItem()
                            {
                                ID = new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                                DXUnitID = objectId,
                                Content = new JObject(
                                    new JProperty(Constants.SystemPropertyTypeName, "MyDXElementDefinition"),
                                    new JProperty(Constants.ID, new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A")),
                                    new JProperty(Constants.DXUnitID,  objectId),
                                    new JProperty(Constants.TimeStamp, new DateTime(2020, 12, 2)),
                                    new JProperty("Name", "Name2"),
                                    new JProperty("Value", 2),
                                    new JProperty("Date", new DateTime(2020, 12, 2)))
                            },
                            new DXItem()
                            {
                                ID = new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                                DXUnitID = objectId,
                                Content = new JObject(
                                    new JProperty(Constants.SystemPropertyTypeName, "MyDXElementDefinition"),
                                    new JProperty(Constants.ID, new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2")),
                                    new JProperty(Constants.DXUnitID, objectId),
                                    new JProperty(Constants.TimeStamp, new DateTime(2020, 12, 3)),
                                    new JProperty("Name", "Name3"),
                                    new JProperty("Value", 3),
                                    new JProperty("Date", new DateTime(2020, 12, 3)))
                            }
                        },
                        Deleted = new HashSet<DXItem>()
                        {
                            new DXItem()
                            {
                                ID = new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                                DXUnitID = objectId,
                                Content = new JObject(
                                    new JProperty(Constants.SystemPropertyTypeName, "MyDXElementDefinition"),
                                    new JProperty(Constants.ID, new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6")),
                                    new JProperty(Constants.DXUnitID, objectId),
                                    new JProperty(Constants.TimeStamp, new DateTime(2020, 12, 4)),
                                    new JProperty("Name", "Name4"),
                                    new JProperty("Value", 4),
                                    new JProperty("Date", new DateTime(2020, 12, 4)))
                            }
                        }
                    }
                }
            };
        }
        
        [Fact]
        public void DXModelParse_UsingJObject_Ok()
        {
            // Init
            var json = File.ReadAllText("Converters/Assets/MyObjectDefinition.json");
            var jObject = JObject.Parse(json);

            // Action
            var result = DXModelConverter.Parse(jObject);

            // Assert
            Assert.True(DXModel.DeepEquals(this.dxModel, result));
        }


        [Fact]
        public void ConvertToJObject_UsingDXModel_Ok()
        {
            // Init
            var json = File.ReadAllText("Converters/Assets/MyObjectDefinition.json");
            var jObject = JObject.Parse(json);

            // Action
            var result = this.dxModel.ConvertToJObject();

            // Assert
            Assert.True(JHelper.DeepEquals(jObject, result));
        }

        [Fact]
        public void DXUnitParse_UsingDXModel_Ok()
        {
            // Init                     

            // Action            
            var result = DXUnit.Parse<MyObject>(this.dxModel);

            // Assert
            this.Compare(this.dxUnit, result);
        }

        [Fact]
        public void DXUnitParse_UsingJObject_Ok()
        {
            // Init
            var json = File.ReadAllText("Converters/Assets/MyObjectDefinition.json");
            var jObject = JObject.Parse(json);

            // Action
            var result = DXUnit.Parse<MyObject>(jObject);

            // Assert
            this.Compare(this.dxUnit, result);
        }

        [Fact]
        public void ConvertToDXModel_UsingDXUnit_Ok()
        {
            // Init                     

            // Action            
            var result = this.dxUnit.ConvertToDXModel();

            // Assert
            Assert.True(DXModel.DeepEquals(this.dxModel, result));
        }

        [Fact]
        public void ConvertToJObject_UsingDXUnit_Ok()
        {
            // Init
            var json = File.ReadAllText("Converters/Assets/MyObjectDefinition.json");
            var jObject = JObject.Parse(json);

            // Action            
            var result = this.dxUnit.ConvertToJObject();

            // Assert
            Assert.True(JHelper.DeepEquals(jObject, result));
        }

        private void Compare(MyObject dxUnit, MyObject result)
        {
            Assert.NotNull(result);

            Assert.Equal(dxUnit.ID, result.ID);
            Assert.Equal(dxUnit.TimeStamp, result.TimeStamp);

            Assert.Equal(dxUnit.MyDXElementSingleItem.ID, result.MyDXElementSingleItem.ID);
            Assert.Equal(dxUnit.MyDXElementSingleItem.TimeStamp, result.MyDXElementSingleItem.TimeStamp);
            Assert.Equal(dxUnit.MyDXElementSingleItem.Name, result.MyDXElementSingleItem.Name);
            Assert.Equal(dxUnit.MyDXElementSingleItem.Value, result.MyDXElementSingleItem.Value);

            Assert.Equal(dxUnit.MyDXElementMultiItems.Announced.Count(), result.MyDXElementMultiItems.Announced.Count());
            Assert.Equal(dxUnit.MyDXElementMultiItems.Deleted.Count(), result.MyDXElementMultiItems.Deleted.Count());
            Assert.Equal(dxUnit.MyDXElementMultiItems.Mode, result.MyDXElementMultiItems.Mode);

            Assert.True(result.MyDXElementMultiItems.Announced.Count > 0);
            Assert.True(result.MyDXElementMultiItems.Deleted.Count > 0);

            foreach (var multiElement in dxUnit.MyDXElementMultiItems.Announced)
            {
                var multiElementFromResult = result.MyDXElementMultiItems.Announced.SingleOrDefault(x => x.ID == multiElement.ID);

                Assert.NotNull(multiElementFromResult);
                Assert.Equal(multiElement.ID, multiElementFromResult.ID);
                Assert.Equal(multiElement.DXUnitID, multiElementFromResult.DXUnitID);
                Assert.Equal(multiElement.TimeStamp, multiElementFromResult.TimeStamp);
                Assert.Equal(multiElement.Name, multiElementFromResult.Name);
                Assert.Equal(multiElement.Value, multiElementFromResult.Value);
            }

            foreach (var multiElement in dxUnit.MyDXElementMultiItems.Deleted)
            {
                var multiElementFromResult = result.MyDXElementMultiItems.Deleted.SingleOrDefault(x => x.ID == multiElement.ID);

                Assert.NotNull(multiElementFromResult);
                Assert.Equal(multiElement.ID, multiElementFromResult.ID);
                Assert.Equal(multiElement.DXUnitID, multiElementFromResult.DXUnitID);
                Assert.Equal(multiElement.TimeStamp, multiElementFromResult.TimeStamp);
                Assert.Equal(multiElement.Name, multiElementFromResult.Name);
                Assert.Equal(multiElement.Value, multiElementFromResult.Value);
            }
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