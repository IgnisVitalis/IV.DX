using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DataProvider.Persistence.Shared.IntTests.Factories;
using IV.DataProvider.Persistence.Shared.IntTests.Factories.Test;
using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using IV.DX.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Repositories.IntTests
{
    public class GenericRepositoryTests : IntTestController
    {
        public GenericRepositoryTests(ITestOutputHelper output)
            : base(output)
        {

        }

        [Fact]
        private void CRUD_UsingEnity_EntityIsProcessedCorrectly_Action()
        {
            // Init
            var objectId = new Guid("8C29571C-3784-4D11-AD3E-F1D055023FD6");

            var item = ObjectFactory.GetItem(objectId, "SomeTestName");

            base._finalizationAction = new Action(() =>
            {
                this._genericRepo.Delete(item);
            });

            // Action Insert
            base._genericRepo.Insert(item);

            // Checking result
            var result = base._genericRepo.GetItem<DPEntityDescObject>(objectId);

            Assert.NotNull(result);
            Assert.Equal(objectId, result.ID);
            Assert.NotNull(result.DPObjectDescGenBlock);
            Assert.True(result.DPObjectDescGenBlock.ID != default(Guid));
            Assert.Equal(objectId, result.DPObjectDescGenBlock.ObjectID);
            Assert.Equal("SomeTestName", result.DPObjectDescGenBlock.Name);
            Assert.True(result is DPEntityDescObject);

            // Action Update
            item.DPObjectDescGenBlock.Name = "UpdatedSomeTestName";
            base._genericRepo.Update(item);

            // Checking result
            result = base._genericRepo.GetItem<DPEntityDescObject>(objectId);

            Assert.NotNull(result);
            Assert.Equal(objectId, result.ID);
            Assert.NotNull(result.DPObjectDescGenBlock);
            Assert.True(result.DPObjectDescGenBlock.ID != default(Guid));
            Assert.Equal(objectId, result.DPObjectDescGenBlock.ObjectID);
            Assert.Equal("UpdatedSomeTestName", result.DPObjectDescGenBlock.Name);
            Assert.True(result is DPEntityDescObject);

            // Action Delete
            base._genericRepo.Delete(item);

            // Checking result
            result = base._genericRepo.GetItem<DPEntityDescObject>(objectId);

            Assert.Null(result);
        }


        // TODO: this test should be update to check multifragments also. Also need to use another entity because DPObjectDescObject can be used in another tests.
        [Fact]
        public void CRUD_UsingEnities_EntitysAreProcessedCorrectly()
        {
            // Init
            var objectId1 = new Guid("3DBA464B-542F-484B-A121-2D2FFEE9FEAC");

            DPObjectDescObject item1 = ObjectFactory.GetItem(objectId1, "SomeTestName1");

            var objectId2 = new Guid("789E3EFA-EB65-498D-A1FC-C2CCA046DC62");

            DPObjectDescObject item2 = ObjectFactory.GetItem(objectId2, "SomeTestName2");

            IEnumerable<DPObjectDescObject> items = new List<DPObjectDescObject>()
            {
                item1, item2
            };

            base._finalizationAction = new Action(() =>
            {
                foreach (var item in items)
                {
                    this._genericRepo.Delete(item);
                }
            });

            // Action Insert
            base._genericRepo.Insert(item1);
            base._genericRepo.Insert(item2);

            // Checking result
            var result = base._genericRepo.GetItems<DPEntityDescObject>();

            var resultItem1 = result.SingleOrDefault(x => x.ID == objectId1);

            Assert.NotNull(resultItem1);
            Assert.Equal(objectId1, resultItem1.ID);
            Assert.NotNull(resultItem1.DPObjectDescGenBlock);
            Assert.True(resultItem1.DPObjectDescGenBlock.ID != default(Guid));
            Assert.Equal(objectId1, resultItem1.DPObjectDescGenBlock.ObjectID);
            Assert.Equal("SomeTestName1", resultItem1.DPObjectDescGenBlock.Name);
            Assert.True(resultItem1 is DPEntityDescObject);

            var resultItem2 = result.SingleOrDefault(x => x.ID == objectId2);

            Assert.NotNull(resultItem2);
            Assert.Equal(objectId2, resultItem2.ID);
            Assert.NotNull(resultItem2.DPObjectDescGenBlock);
            Assert.True(resultItem2.DPObjectDescGenBlock.ID != default(Guid));
            Assert.Equal(objectId2, resultItem2.DPObjectDescGenBlock.ObjectID);
            Assert.Equal("SomeTestName2", resultItem2.DPObjectDescGenBlock.Name);
            Assert.True(resultItem2 is DPEntityDescObject);

            // Action Update
            item1.DPObjectDescGenBlock.Name = "UpdatedSomeTestName1";
            base._genericRepo.Update(item1);

            item2.DPObjectDescGenBlock.Name = "UpdatedSomeTestName2";
            base._genericRepo.Update(item2);

            // Checking result
            result = base._genericRepo.GetItems<DPEntityDescObject>();

            resultItem1 = result.SingleOrDefault(x => x.ID == objectId1);

            Assert.NotNull(resultItem1);
            Assert.Equal(objectId1, resultItem1.ID);
            Assert.NotNull(resultItem1.DPObjectDescGenBlock);
            Assert.True(resultItem1.DPObjectDescGenBlock.ID != default(Guid));
            Assert.Equal(objectId1, resultItem1.DPObjectDescGenBlock.ObjectID);
            Assert.Equal("UpdatedSomeTestName1", resultItem1.DPObjectDescGenBlock.Name);
            Assert.True(resultItem1 is DPEntityDescObject);

            resultItem2 = result.SingleOrDefault(x => x.ID == objectId2);

            Assert.NotNull(resultItem2);
            Assert.Equal(objectId2, resultItem2.ID);
            Assert.NotNull(resultItem2.DPObjectDescGenBlock);
            Assert.True(resultItem2.DPObjectDescGenBlock.ID != default(Guid));
            Assert.Equal(objectId2, resultItem2.DPObjectDescGenBlock.ObjectID);
            Assert.Equal("UpdatedSomeTestName2", resultItem2.DPObjectDescGenBlock.Name);
            Assert.True(resultItem2 is DPEntityDescObject);

            // Action Delete
            base._genericRepo.Delete(item1);

            // Checking result
            resultItem1 = base._genericRepo.GetItem<DPEntityDescObject>(objectId1);

            Assert.Null(resultItem1);

            // Action Delete
            base._genericRepo.Delete(item2);

            // Checking result
            resultItem2 = base._genericRepo.GetItem<DPEntityDescObject>(objectId2);

            Assert.Null(resultItem2);
        }

        [Fact]
        public void CrudBlock_UsingSingleESQLBlock_BlockIsProcessedCorrectly()
        {
            // Init
            var objectId = new Guid("57499CB1-1C08-4480-A274-2C71CE943B43");
            var book = TBookObjectFactory.GetItemWithText(objectId, "MyBook", new[] { "Page1", "Page2", "Page3", "Page4" });

            base._finalizationAction = new Action(() =>
            {
                this._genericRepo.Delete(book);
            });

            base._genericRepo.Insert(book);

            var page5 = new TBookChapterBlock()
            {
                ID = Guid.NewGuid(),
                ObjectID = objectId,
                Number = 5,
                Text = "Page5"
            };

            // Action Insert
            this._genericRepo.InsertBlock("TBookObject", page5);

            // Checking result
            var createdBlock = this._genericRepo.GetBlock<TBookChapterBlock>(page5.ID);

            Assert.NotNull(createdBlock);
            Assert.Equal(page5.ID, createdBlock.ID);
            Assert.Equal(page5.ObjectID, createdBlock.ObjectID);
            Assert.Equal(page5.Number, createdBlock.Number);
            Assert.Equal(page5.Text, createdBlock.Text);

            // Action Update 
            page5.Number = 6;
            page5.Text = "Page6";
            this._genericRepo.UpdateBlock("TBookObject", page5);

            // Checking result
            var updatedBlock = this._genericRepo.GetBlock<TBookChapterBlock>(page5.ID);

            Assert.NotNull(updatedBlock);
            Assert.Equal(page5.ID, updatedBlock.ID);
            Assert.Equal(page5.ObjectID, updatedBlock.ObjectID);
            Assert.Equal(page5.Number, updatedBlock.Number);
            Assert.Equal(page5.Text, updatedBlock.Text);

            // Action Update            
            this._genericRepo.DeleteBlock(page5);

            // Checking result
            var deletedBlock = this._genericRepo.GetBlock<TBookChapterBlock>(page5.ID);

            Assert.Null(deletedBlock);

            var existingBook = this._genericRepo.GetItem<TBookObject>(objectId);

            Assert.Equal(existingBook.TBookChapterBlock.Announced.Count(), book.TBookChapterBlock.Announced.Count());

            Assert.True(existingBook.TBookChapterBlock.Announced.All(x => x.ID != page5.ID));
        }
    }
}