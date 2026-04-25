using IV.DX.Kernel;
using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Converters.DXObjectConverters;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class DXRecordConverterTests
    {
        private readonly MyObject dxUnit;

        public DXRecordConverterTests()
        {
            var objectId = new Guid("032169A1-FDFA-45B8-B0EC-381A6888FF35");

            this.dxUnit = new MyObject()
            {
                Id = objectId,
                TimeStamp = new DateTime(2020, 12, 1),
                MyDXElementSingleItem = new MyDXElement()
                {
                    Id = new Guid("B2669009-536A-4252-A920-CCEF4456A08A"),
                    DXUnitId = objectId,
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
                            Id = new Guid("293E5981-2E59-4714-95E5-52D9FF5EF76A"),
                            DXUnitId = objectId,
                            TimeStamp = new DateTime(2020, 12, 2),
                            Name = "Name2",
                            Value = 2,
                            Date = new DateTime(2020, 12, 2),
                        },
                        new MyDXElement()
                        {
                            Id = new Guid("30F22EFA-6402-4C94-BCDD-CA4D2E8D40C2"),
                            DXUnitId = objectId,
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
                            Id = new Guid("7FD1CCDA-FEB4-435A-95DB-39B656FE12A6"),
                            DXUnitId = objectId,
                            TimeStamp =  new DateTime(2020, 12, 4),
                            Name = "Name4",
                            Value = 4,
                            Date = new DateTime(2020, 12, 4),
                        }
                    },
                }
            };

        }

        [Fact]
        public void ConvertToRecord_UsingDXUnit_Ok()
        {
            var block = DXRecordWriter.ToBlock(this.dxUnit);
            var record = block.Data?.Items?.SingleOrDefault();

            Assert.NotNull(record);
            Assert.Equal(this.dxUnit.Id, record!.Id);
            Assert.Equal(this.dxUnit.TimeStamp, record.TimeStamp);
            Assert.NotNull(record.DXElements);
        }

        [Fact]
        public void DXUnitParse_UsingRecord_Ok()
        {
            var block = DXRecordWriter.ToBlock(this.dxUnit, new DXRecordWriteOptions { IncludeDeleteFields = true });
            var record = block.Data?.Items?.Single();
            var result = (MyObject)DXRecordConverter.ToDXUnit(record!, typeof(MyObject));

            // Assert
            this.Compare(this.dxUnit, result);
        }

        [Fact]
        public void DXUnitParse_UsingJObject_Ok()
        {
            var block = DXRecordWriter.ToBlock(this.dxUnit, new DXRecordWriteOptions { IncludeDeleteFields = true });
            var jObject = JObject.FromObject(block);
            var parsed = jObject.ToObject<DXDataBlock<DXUnitRecord>>();
            var record = parsed?.Data?.Items?.Single();
            var result = (MyObject)DXRecordConverter.ToDXUnit(record!, typeof(MyObject));

            // Assert
            this.Compare(this.dxUnit, result);
        }

        private void Compare(MyObject dxUnit, MyObject result)
        {
            Assert.NotNull(result);

            Assert.Equal(dxUnit.Id, result.Id);
            Assert.Equal(dxUnit.TimeStamp, result.TimeStamp);

            Assert.Equal(dxUnit.MyDXElementSingleItem.Id, result.MyDXElementSingleItem.Id);
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
                var multiElementFromResult = result.MyDXElementMultiItems.Announced.SingleOrDefault(x => x.Id == multiElement.Id);

                Assert.NotNull(multiElementFromResult);
                Assert.Equal(multiElement.Id, multiElementFromResult!.Id);
                Assert.Equal(multiElement.DXUnitId, multiElementFromResult.DXUnitId);
                Assert.Equal(multiElement.TimeStamp, multiElementFromResult.TimeStamp);
                Assert.Equal(multiElement.Name, multiElementFromResult.Name);
                Assert.Equal(multiElement.Value, multiElementFromResult.Value);
            }

            foreach (var multiElement in dxUnit.MyDXElementMultiItems.Deleted)
            {
                var multiElementFromResult = result.MyDXElementMultiItems.Deleted.SingleOrDefault(x => x.Id == multiElement.Id);

                Assert.NotNull(multiElementFromResult);
                Assert.Equal(multiElement.Id, multiElementFromResult!.Id);
                Assert.Equal(multiElement.DXUnitId, multiElementFromResult.DXUnitId);
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
        public MyDXElement MyDXElementSingleItem { get; set; } = null!;

        public DXMultiElementsContainer<MyDXElement> MyDXElementMultiItems { get; set; } = null!;
    }

    [DXElement("MyDXElementDefinition")]
    internal class MyDXElement : DXElement
    {
        [DXColumn("Name")]
        public string Name { get; set; } = null!;
        [DXColumn("Value")]
        public int Value { get; set; }
        [DXColumn("Date")]
        public DateTime Date { get; set; }
    }
}

