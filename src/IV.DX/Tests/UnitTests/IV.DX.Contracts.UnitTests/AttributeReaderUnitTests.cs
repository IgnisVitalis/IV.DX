using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using System.Linq;
using Xunit;

namespace IV.DataProvider.Persistence.Common.IntTests.Helpers
{
    public class AttributeReaderUnitTests
    {
        public AttributeReaderUnitTests()
        {

        }

        [Fact]
        public void GetESQLObjectDefinitionAttribute_FromDXObjectDefinitionUnit_AttributeWithCorrectValues()
        {
            // Init

            // Action
            var attr = AttributeReader
                .GetSingleAttribute<ESQLObjectDefinitionAttribute>
                (typeof(DXObjectDefinitionUnit));

            // Checking results
            Assert.NotNull(attr);
            Assert.True(attr.ObjectName == "DXObjectDefinitionUnit");
        }

        [Fact]
        public void GetESQLBlockDefinitionAttribute_FromDXUnitDefinitionMainElement_AttributeWithCorrectValues()
        {
            // Init

            // Action
            var attr = AttributeReader
                .GetSingleAttribute<ESQLBlockDefinitionAttribute>
                (typeof(DXUnitDefinitionMainElement));

            // Checking results
            Assert.NotNull(attr);
            Assert.True(attr.BlockName == "DXUnitDefinitionMainElement");
        }

        [Fact]
        public void GetESQLColumnDefinitionAttributes_FromDXUnitDefinitionMainElement_AttributesWithCorrectValues()
        {
            // Init

            // Action
            var attributes = AttributeReader
                    .GetAllSinglePropertyAttributes<ESQLColumnDefinitionAttribute>
                    (typeof(DXUnitDefinitionMainElement));

            // Checking result
            Assert.True(attributes.Count() == 6);

            var displayValueAttr = attributes.SingleOrDefault(x => x.ColumnName == "DisplayValue");
            Assert.NotNull(displayValueAttr);

            var namePropertyAttr = attributes.SingleOrDefault(x => x.ColumnName == "Name");
            Assert.NotNull(namePropertyAttr);

            var idPropertyAttr = attributes.SingleOrDefault(x => x.ColumnName == "ID");
            Assert.NotNull(idPropertyAttr);

            var objectIdPropertyAttr = attributes.SingleOrDefault(x => x.ColumnName == "ObjectID");
            Assert.NotNull(objectIdPropertyAttr);

            var kindPropertyAttr = attributes.SingleOrDefault(x => x.ColumnName == "Kind");
            Assert.NotNull(kindPropertyAttr);

            var timeStampePropertyAttr = attributes.SingleOrDefault(x => x.ColumnName == "TimeStamp");
            Assert.NotNull(timeStampePropertyAttr);
        }
    }
}