using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXModelConverters;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Converters.JObjectConverters;
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
                Item = new DXItem(
                            "MyObjectDefinition",
                            objectId,
                            objectId,
                            new DateTime(2020, 12, 1),
                            new Dictionary<string, object>()
                            {
                                { Constants.SystemPropertyTypeName, "MyObjectDefinition" },
                                { Constants.ID, objectId },
                                { Constants.TimeStamp, new DateTime(2020, 12, 1)}
                            })
            };

            var dxSingleElement1 = new DXSingleElement(
                 "MyDXElementSingleItem",
                  new DXElementAttribute("MyDXElementDefinition"),
                  new DXItem("MyDXElementDefinition", new Guid("B2669009-536A-4252-A920-CCEF4456A08A"), objectId, new DateTime(2020, 12, 1), new Dictionary<string, object>()
                            {
                      { Constants.SystemPropertyTypeName, "MyDXElementDefinition"},
                            {Constants.ID, new Guid("B2669009-536A-4252-A920-CCEF4456A08A") },
                            {Constants.DXUnitID, objectId},
                            {Constants.TimeStamp, new DateTime(2020, 12, 1)},
                            { "Name", "Name1"},
                            { "Value", 1},
                            { "Date", new DateTime(2020, 12, 1)}
                  }), true);

            var dxXSingleElements = new HashSet<DXSingleElement>()
            {
                dxSingleElement1
            };

            var dxMultiElements = new HashSet<DXMultiElement>()
            {
                new DXMultiElement(
                    "MyDXElementMultiItems",
                    new DXElementAttribute("MyDXElementDefinition"),
                    MultiElementsMode.Target,
                    new HashSet<DXItem>()
                    {
                        new DXItem(
                            "MyDXElementDefinition",
                            new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                            objectId,
                            new DateTime(2020, 12, 2),
                            new Dictionary<string, object>()
                            {
                                {Constants.SystemPropertyTypeName, "MyDXElementDefinition" },
                                {Constants.ID, new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A") },
                                {Constants.DXUnitID,  objectId },
                                {Constants.TimeStamp, new DateTime(2020, 12, 2) },
                                {"Name", "Name2" },
                                {"Value", 2 },
                                {"Date", new DateTime(2020, 12, 2) }
                            }),
                        new DXItem(
                            "MyDXElementDefinition",
                            new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                            objectId,
                            new DateTime(2020, 12, 3),
                            new Dictionary<string, object>()
                            {
                                {Constants.SystemPropertyTypeName, "MyDXElementDefinition" },
                                {Constants.ID, new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2") },
                                {Constants.DXUnitID, objectId },
                                {Constants.TimeStamp, new DateTime(2020, 12, 3) },
                                {"Name", "Name3" },
                                {"Value", 3 },
                                {"Date", new DateTime(2020, 12, 3) }
                            })
                    },
                    new HashSet<DXItem>()
                    {
                        new DXItem(
                            "MyDXElementDefinition",
                            new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                            objectId,
                            new DateTime(2020, 12, 4),
                            new Dictionary<string, object>()
                            {
                                {Constants.SystemPropertyTypeName, "MyDXElementDefinition" },
                                {Constants.ID, new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6") },
                                {Constants.DXUnitID, objectId },
                                {Constants.TimeStamp, new DateTime(2020, 12, 4) },
                                {"Name", "Name4" },
                                {"Value", 4 },
                                {"Date", new DateTime(2020, 12, 4) }
                            })
                    }, false)
            };

            dxModel = new DXModel(ownItem, dxXSingleElements, dxMultiElements);
        }

        [Fact]
        public void DXModelParse_UsingJObject_Ok()
        {
            // Init
            var json = File.ReadAllText("Converters/Assets/MyObjectDefinition.json");
            var jObject = JObject.Parse(json);

            // Action
            var result = DXModelConverter.ToDXModel(jObject);

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
            var result = this.dxModel.ToJObject();

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
            var result = DXModelConverter.ToDXModel(this.dxUnit);

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
            var result = JObjectConverter.ToJObject(dxUnit);

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
        [DXRequired]
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