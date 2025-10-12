using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DXElementGenericRepositoryTests : IntTestController
    {
        IDXElementGenericRepository _dxElementGenericRepo;
        IDXUnitGenericRepository _dxUnitGenericRepo;

        public DXElementGenericRepositoryTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dxElementGenericRepo = this.ServiceProvider.GetRequiredService<IDXElementGenericRepository>();
            this._dxUnitGenericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
        }

        [Fact]
        public void CrudBlock_UsingSingleDXElement_DXElementIsProcessedCorrectly()
        {
            // Init
            var objectId = new Guid("57499CB1-1C08-4480-A274-2C71CE943B43");
            var book = TBookUnitFactory.GetItemWithText(objectId, "MyBook", new[] { "Page1", "Page2", "Page3", "Page4" });

            base._finalizationAction = new Action(() =>
            {
                this._dxUnitGenericRepo.Delete(book);
            });

            this._dxUnitGenericRepo.Insert(book);

            var page5 = new TBookChapterElement()
            {
                ID = Guid.NewGuid(),
                ObjectID = objectId,
                Number = 5,
                Text = "Page5"
            };

            // Action Insert
            this._dxElementGenericRepo.InsertBlock("TBookUnit", page5);

            // Checking result
            var createdBlock = this._dxElementGenericRepo.GetBlock<TBookChapterElement>(page5.ID);

            Assert.NotNull(createdBlock);
            Assert.Equal(page5.ID, createdBlock.ID);
            Assert.Equal(page5.ObjectID, createdBlock.ObjectID);
            Assert.Equal(page5.Number, createdBlock.Number);
            Assert.Equal(page5.Text, createdBlock.Text);

            // Action Update 
            page5.Number = 6;
            page5.Text = "Page6";
            this._dxElementGenericRepo.UpdateBlock("TBookUnit", page5);

            // Checking result
            var updatedBlock = this._dxElementGenericRepo.GetBlock<TBookChapterElement>(page5.ID);

            Assert.NotNull(updatedBlock);
            Assert.Equal(page5.ID, updatedBlock.ID);
            Assert.Equal(page5.ObjectID, updatedBlock.ObjectID);
            Assert.Equal(page5.Number, updatedBlock.Number);
            Assert.Equal(page5.Text, updatedBlock.Text);

            // Action Update            
            this._dxElementGenericRepo.DeleteBlock(page5);

            // Checking result
            var deletedBlock = this._dxElementGenericRepo.GetBlock<TBookChapterElement>(page5.ID);

            Assert.Null(deletedBlock);

            var existingBook = this._dxUnitGenericRepo.GetDXUnit<TBookUnit>(objectId);

            Assert.Equal(existingBook.TBookChapterElement.Announced.Count(), book.TBookChapterElement.Announced.Count());

            Assert.True(existingBook.TBookChapterElement.Announced.All(x => x.ID != page5.ID));
        }
    }
}
