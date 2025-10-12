using IV.DX.Contracts.Persistence.ExpressionTree;
using System.Linq;
using Xunit;

namespace IV.DX.Contracts.UnitTests
{
    public class OrientedTreeUnitTests
    {
        [Fact]
        public void Load_UsingExpressionsWithSamePath_CorrectTree()
        {
            // Init
            var type = "SomeType";
            var expression1 = "R(First).R(Second).Third.4";
            var expression2 = "R(First).R(2).R(Third).Fourth";
            var expression3 = "R(First).R(Second).3.4";
            var expression4 = "R(1).R(Second).R(3).Fourth";

            var expression5 = "R(1).R(Second).R(3).Fourth";
            var expression6 = "R(First).R(Second).3.4";

            var expressionFull = $"{expression1} AND {expression2} OR {expression3} AND {expression4} AND {expression5} OR {expression6}";

            // Action
            var instance = DXOrientedTree.CreateInstance(type);
            instance.Load(expressionFull);

            // Checking results
            Assert.Equal(14, instance.AllNodes.Count());

            Assert.Equal(4, instance.Expressions.Count());

            var expression1Loaded = instance.Expressions.First();
            var expression2Loaded = instance.Expressions.Skip(1).First();
            var expression3Loaded = instance.Expressions.Skip(2).First();
            var expression4Loaded = instance.Expressions.Skip(3).First();

            Assert.Equal(expression1, expression1Loaded.Key);
            Assert.Equal(DXLogicOperation.AND, expression1Loaded.Value);

            Assert.Equal(expression2, expression2Loaded.Key);
            Assert.Equal(DXLogicOperation.AND, expression2Loaded.Value);

            Assert.Equal(expression3, expression3Loaded.Key);
            Assert.Equal(DXLogicOperation.OR, expression3Loaded.Value);

            Assert.Equal(expression4, expression4Loaded.Key);
            Assert.Equal(DXLogicOperation.AND, expression4Loaded.Value);

            // Checking results
            var coreNode = instance.CoreNode;

            this.AssertCoreNode(coreNode, "SomeType");
            this.AssertNodeCoordinates(coreNode, 0, 0);

            Assert.True(instance.AllNodes.First() == coreNode);

            var childsFirstLevel = coreNode.Childs;

            Assert.Equal(2, childsFirstLevel.Count());

            var childFirstLevel0 = childsFirstLevel.First() as DXUnitNode;
            var childFirstLevel1 = childsFirstLevel.Skip(1).First() as DXUnitNode;

            this.AssertEntityNode(childFirstLevel0, "R(First)", coreNode);
            this.AssertEntityNode(childFirstLevel1, "R(1)", coreNode);
            this.AssertNodeCoordinates(childFirstLevel0, 1, 0);
            this.AssertNodeCoordinates(childFirstLevel1, 1, 1);

            Assert.True(instance.AllNodes.Skip(1).First() == childFirstLevel0);
            Assert.True(instance.AllNodes.Skip(10).First() == childFirstLevel1);

            var childsSecondLevel0 = childFirstLevel0.Childs;
            var childsSecondLevel1 = childFirstLevel1.Childs;

            Assert.Equal(2, childsSecondLevel0.Count());
            Assert.Single(childsSecondLevel1);

            var childSecondLevel0 = childsSecondLevel0.First() as DXUnitNode;
            var childSecondLevel1 = childsSecondLevel0.Skip(1).First() as DXUnitNode;

            this.AssertEntityNode(childSecondLevel0, "R(Second)", childFirstLevel0);
            this.AssertEntityNode(childSecondLevel1, "R(2)", childFirstLevel0);
            this.AssertNodeCoordinates(childSecondLevel0, 2, 0);
            this.AssertNodeCoordinates(childSecondLevel1, 2, 1);

            var childSecondLevel2 = childsSecondLevel1.First() as DXUnitNode;

            this.AssertEntityNode(childSecondLevel2, "R(Second)", childFirstLevel1);
            this.AssertNodeCoordinates(childSecondLevel2, 2, 2);

            Assert.True(instance.AllNodes.Skip(2).First() == childSecondLevel0);
            Assert.True(instance.AllNodes.Skip(5).First() == childSecondLevel1);
            Assert.True(instance.AllNodes.Skip(11).First() == childSecondLevel2);

            var childsThirdLevel0 = childSecondLevel0.Childs;
            var childsThirdLevel1 = childSecondLevel1.Childs;
            var childsThirdLevel2 = childSecondLevel2.Childs;

            Assert.Equal(2, childsThirdLevel0.Count());
            Assert.Single(childsThirdLevel1);
            Assert.Single(childsThirdLevel2);

            var childThirdLevel0 = childsThirdLevel0.First() as DXElementNode;
            var childThirdLevel1 = childsThirdLevel0.Skip(1).First() as DXElementNode;

            this.AssertBlockNode(childThirdLevel0, "Third", childSecondLevel0);
            this.AssertBlockNode(childThirdLevel1, "3", childSecondLevel0);
            this.AssertNodeCoordinates(childThirdLevel0, 3, 0);
            this.AssertNodeCoordinates(childThirdLevel1, 3, 2);

            var childThirdLevel2 = childsThirdLevel1.First() as DXUnitNode;

            this.AssertEntityNode(childThirdLevel2, "R(Third)", childSecondLevel1);
            this.AssertNodeCoordinates(childThirdLevel2, 3, 1);

            var childThirdLevel3 = childsThirdLevel2.First() as DXUnitNode;

            this.AssertEntityNode(childThirdLevel3, "R(3)", childSecondLevel2);
            this.AssertNodeCoordinates(childThirdLevel3, 3, 3);

            Assert.True(instance.AllNodes.Skip(3).First() == childThirdLevel0);
            Assert.True(instance.AllNodes.Skip(8).First() == childThirdLevel1);
            Assert.True(instance.AllNodes.Skip(6).First() == childThirdLevel2);
            Assert.True(instance.AllNodes.Skip(12).First() == childThirdLevel3);

            var childsFourthLevel0 = childThirdLevel0.Childs;
            var childsFourthLevel1 = childThirdLevel1.Childs;
            var childsFourthLevel2 = childThirdLevel2.Childs;
            var childsFourthLevel3 = childThirdLevel3.Childs;

            Assert.Single(childsFourthLevel0);
            Assert.Single(childsFourthLevel1);
            Assert.Single(childsFourthLevel2);
            Assert.Single(childsFourthLevel3);

            var childFourthLevel0 = childsFourthLevel0.First() as DXPropertyNode;

            this.AssertPropertyNode(childFourthLevel0, "4", childThirdLevel0, 0, DXLogicOperation.AND);
            this.AssertNodeCoordinates(childFourthLevel0, 4, 0);

            var childFourthLevel1 = childsFourthLevel1.First() as DXPropertyNode;

            this.AssertPropertyNode(childFourthLevel1, "4", childThirdLevel1, 2, DXLogicOperation.OR);
            this.AssertNodeCoordinates(childFourthLevel1, 4, 2);

            var childFourthLevel2 = childsFourthLevel2.First() as DXPropertyNode;

            this.AssertPropertyNode(childFourthLevel2, "Fourth", childThirdLevel2, 1, DXLogicOperation.AND);
            this.AssertNodeCoordinates(childFourthLevel2, 4, 1);

            var childFourthLevel3 = childsFourthLevel3.First() as DXPropertyNode;

            this.AssertPropertyNode(childFourthLevel3, "Fourth", childThirdLevel3, 3, DXLogicOperation.AND);
            this.AssertNodeCoordinates(childFourthLevel3, 4, 3);

            Assert.True(instance.AllNodes.Skip(4).First() == childFourthLevel0);
            Assert.True(instance.AllNodes.Skip(9).First() == childFourthLevel1);
            Assert.True(instance.AllNodes.Skip(7).First() == childFourthLevel2);
            Assert.True(instance.AllNodes.Skip(13).First() == childFourthLevel3);
        }

        private void AssertCoreNode(DXCoreNode node, string expectedValue)
        {
            Assert.NotNull(node);
            Assert.Equal(expectedValue, node.Value);
            Assert.Null(node.Mother);
        }

        private void AssertEntityNode(DXUnitNode node, string expectedValue, DXBaseNode expectedMotherNode)
        {
            Assert.NotNull(node);
            Assert.Equal(expectedValue, node.Value);
            Assert.Equal(expectedMotherNode, node.Mother);
        }

        private void AssertBlockNode(DXElementNode node, string expectedValue, DXBaseNode expectedMotherNode)
        {
            Assert.NotNull(node);
            Assert.Equal(expectedValue, node.Value);
            Assert.Equal(expectedMotherNode, node.Mother);
        }

        private void AssertPropertyNode(DXPropertyNode node, string expectedValue, DXBaseNode expectedMotherNode, int expectedOrder, DXLogicOperation expectedLogicOpeation)
        {
            Assert.NotNull(node);
            Assert.Equal(expectedValue, node.Value);
            Assert.Equal(expectedMotherNode, node.Mother);
            Assert.Equal(expectedLogicOpeation, node.LogicOperation);
            Assert.Equal(expectedOrder, node.Y);
        }

        private void AssertNodeCoordinates(DXBaseNode node, int xEpxected, int yExpected)
        {
            Assert.Equal(xEpxected, node.X);
            Assert.Equal(yExpected, node.Y);
        }
    }
}