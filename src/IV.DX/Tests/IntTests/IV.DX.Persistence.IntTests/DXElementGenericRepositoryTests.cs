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
        public void CrudDXElement_UsingSingleDXElement_DXElementIsProcessedCorrectly()
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
                Id = Guid.NewGuid(),
                DXUnitId = objectId,
                Number = 5,
                Text = "Page5"
            };

            // Action Insert
            this._dxElementGenericRepo.Insert("TBookUnit", page5);

            // Checking result
            var createdDXElement = this._dxElementGenericRepo.GetItem<TBookChapterElement>("TBookUnit", page5.Id);

            Assert.NotNull(createdDXElement);
            Assert.Equal(page5.Id, createdDXElement.Id);
            Assert.Equal(page5.DXUnitId, createdDXElement.DXUnitId);
            Assert.Equal(page5.Number, createdDXElement.Number);
            Assert.Equal(page5.Text, createdDXElement.Text);

            // Action Update 
            page5.Number = 6;
            page5.Text = "Page6";
            this._dxElementGenericRepo.Update("TBookUnit", page5);

            // Checking result
            var updatedDXElement = this._dxElementGenericRepo.GetItem<TBookChapterElement>("TBookUnit", page5.Id);

            Assert.NotNull(updatedDXElement);
            Assert.Equal(page5.Id, updatedDXElement.Id);
            Assert.Equal(page5.DXUnitId, updatedDXElement.DXUnitId);
            Assert.Equal(page5.Number, updatedDXElement.Number);
            Assert.Equal(page5.Text, updatedDXElement.Text);

            // Action Update            
            this._dxElementGenericRepo.Delete(page5);

            // Checking result
            var deletedDXElement = this._dxElementGenericRepo.GetItem<TBookChapterElement>("TBookUnit", page5.Id);

            Assert.Null(deletedDXElement);

            var existingBook = this._dxUnitGenericRepo.GetDXUnit<TBookUnit>(objectId);

            Assert.Equal(existingBook.TBookChapterElement.Announced.Count(), book.TBookChapterElement.Announced.Count());

            Assert.True(existingBook.TBookChapterElement.Announced.All(x => x.Id != page5.Id));
        }
    }
}
