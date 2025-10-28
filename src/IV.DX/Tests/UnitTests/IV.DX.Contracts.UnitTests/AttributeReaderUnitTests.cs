using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Helpers;
using IV.DX.Kernel.Models;
using System.Linq;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class AttributeReaderUnitTests
    {
        public AttributeReaderUnitTests()
        {

        }

        [Fact]
        public void GetdxUnitectDefinitionAttribute_FromDXObjectDefinitionUnit_AttributeWithCorrectValues()
        {
            // Init

            // Action
            var attr = AttributeReader
                .GetAttribute<DXUnitAttribute>
                (typeof(DXObjectDefinitionUnit));

            // Checking results
            Assert.NotNull(attr);
            Assert.True(attr.Type == "DXObjectDefinitionUnit");
        }

        [Fact]
        public void GetDXElementDefinitionAttribute_FromDXObjectDefinitionMainElement_AttributeWithCorrectValues()
        {
            // Init

            // Action
            var attr = AttributeReader
                .GetAttribute<DXElementAttribute>
                (typeof(DXObjectDefinitionMainElement));

            // Checking results
            Assert.NotNull(attr);
            Assert.True(attr.Type == "DXObjectDefinitionMainElement");
        }

        [Fact]
        public void GetDXColumnDefinitionAttributes_FromDXObjectDefinitionMainElement_AttributesWithCorrectValues()
        {
            // Init

            // Action
            var attributes = AttributeReader
                    .GetAttributesOnProperties<DXColumnAttribute>
                    (typeof(DXObjectDefinitionMainElement));

            // Checking result
            Assert.True(attributes.Count() == 6);

            var displayValueAttr = attributes.SingleOrDefault(x => x.Name == "DisplayValue");
            Assert.NotNull(displayValueAttr);

            var namePropertyAttr = attributes.SingleOrDefault(x => x.Name == "Name");
            Assert.NotNull(namePropertyAttr);

            var idPropertyAttr = attributes.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idPropertyAttr);

            var objectIdPropertyAttr = attributes.SingleOrDefault(x => x.Name == "DXUnitID");
            Assert.NotNull(objectIdPropertyAttr);

            var kindPropertyAttr = attributes.SingleOrDefault(x => x.Name == "Kind");
            Assert.NotNull(kindPropertyAttr);

            var timeStampePropertyAttr = attributes.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(timeStampePropertyAttr);
        }
    }
}