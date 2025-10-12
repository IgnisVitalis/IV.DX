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
                .GetSingleAttribute<DXUnitAttribute>
                (typeof(DXObjectDefinitionUnit));

            // Checking results
            Assert.NotNull(attr);
            Assert.True(attr.ObjectName == "DXObjectDefinitionUnit");
        }

        [Fact]
        public void GetDXElementDefinitionAttribute_FromDXUnitDefinitionMainElement_AttributeWithCorrectValues()
        {
            // Init

            // Action
            var attr = AttributeReader
                .GetSingleAttribute<DXElementAttribute>
                (typeof(DXUnitDefinitionMainElement));

            // Checking results
            Assert.NotNull(attr);
            Assert.True(attr.Name == "DXUnitDefinitionMainElement");
        }

        [Fact]
        public void GetDXColumnDefinitionAttributes_FromDXUnitDefinitionMainElement_AttributesWithCorrectValues()
        {
            // Init

            // Action
            var attributes = AttributeReader
                    .GetAllSinglePropertyAttributes<DXColumnAttribute>
                    (typeof(DXUnitDefinitionMainElement));

            // Checking result
            Assert.True(attributes.Count() == 6);

            var displayValueAttr = attributes.SingleOrDefault(x => x.Name == "DisplayValue");
            Assert.NotNull(displayValueAttr);

            var namePropertyAttr = attributes.SingleOrDefault(x => x.Name == "Name");
            Assert.NotNull(namePropertyAttr);

            var idPropertyAttr = attributes.SingleOrDefault(x => x.Name == "ID");
            Assert.NotNull(idPropertyAttr);

            var objectIdPropertyAttr = attributes.SingleOrDefault(x => x.Name == "ObjectID");
            Assert.NotNull(objectIdPropertyAttr);

            var kindPropertyAttr = attributes.SingleOrDefault(x => x.Name == "Kind");
            Assert.NotNull(kindPropertyAttr);

            var timeStampePropertyAttr = attributes.SingleOrDefault(x => x.Name == "TimeStamp");
            Assert.NotNull(timeStampePropertyAttr);
        }
    }
}