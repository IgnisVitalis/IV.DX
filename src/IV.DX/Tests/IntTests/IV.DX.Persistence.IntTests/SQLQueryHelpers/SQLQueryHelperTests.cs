using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.SQLQueryHelpers;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests.SQLQueryHelpers
{
    [Collection("DX:one-time")]
    public class SQLQueryHelperTests : IntTestController
    {
        public IEnumerable<TUserUnit> users;
        public IEnumerable<TDeviceUnit> devices;
        public IEnumerable<TPassportUnit> passports;
        public IEnumerable<TPositionUnit> positions;
        public IEnumerable<TDocumentUnit> documents;
        public IEnumerable<TBookUnit> books;

        IDXUnitGenericRepository _genericRepo;
        ISQLQueryBuilder _sqlQueryBuilder;

        const int dxUnitStartIndex = 23;
        const int dxElementStartIndex = 42;

        string tableRelation_TUserUnit_TBookUnit_0 = "Relation_TUserUnit_TBookUnit_0";
        string tableAlias_TPassportUnit_0 = $"T_{dxUnitStartIndex}_0";//TPassportUnit
        string tableAlias_TPositionUnit_0 = $"T_{dxUnitStartIndex + 1}_0";//TPositionUnit        
        string tableAlias_TDocumentUnit_0 = $"T_{dxUnitStartIndex + 2}_0";//TDocumentUnit
        string tableAlias_TComputerUnit_0 = $"T_{dxUnitStartIndex + 3}_0";//TComputerUnit         
        string tableAlias_TDeviceUnit_0 = $"T_{dxUnitStartIndex + 4}_0";//TDeviceUnit        
        string tableAlias_TBookUnit_0 = $"T_{dxUnitStartIndex + 5}_0";//TBookUnit   
        string tableAlias_TUserUnit_0 = $"T_{dxUnitStartIndex + 6}_0";//TUserUnit
        string tableAlias_TPassportMainElement_0 = $"T_{dxElementStartIndex}_0";//TPassportMainElement
        string tableAlias_TPositionMainElement_0 = $"T_{dxElementStartIndex + 1}_0";//TPositionMainElement
        string tableAlias_TUserMainElement_0 = $"T_{dxElementStartIndex + 2}_0";//TUserMainElement
        string tableAlias_TDeviceMainElement_0 = $"T_{dxElementStartIndex + 3}_0";//TDeviceMainElement
        string tableAlias_TBookMainElement_0 = $"T_{dxElementStartIndex + 4}_0";//TBookMainElement


        public SQLQueryHelperTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._genericRepo = this.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            this._sqlQueryBuilder = this.ServiceProvider.GetRequiredService<ISQLQueryBuilder>();

            InitData();
        }

        private void InitData()
        {
            // Init           
            var user1 = TUserUnitFactory.GetItem("Victor", "Suvorov", new DateTime(1989, 9, 5));
            var user2 = TUserUnitFactory.GetItem("Svitlana", "Suvorova", new DateTime(1993, 11, 17));
            var user3 = TUserUnitFactory.GetItem("Pavel", "Plamenev", new DateTime(1980, 1, 1));

            users = new List<TUserUnit>() { user1, user2, user3 };

            var passport1 = TPassportUnitFactory.GetItem("6bcc2af44aa3", user1);
            var passport2 = TPassportUnitFactory.GetItem("6ca8ae455e82", user2);
            var passport3 = TPassportUnitFactory.GetItem("4a82ceaa1cff", user3);

            passports = new List<TPassportUnit>() { passport1, passport2, passport3 };

            var device1 = TDeviceUnitFactory.GetItem("Model1", new Guid("70f86100-bc9c-4b88-8f5f-759cedf85972"), user1);
            var device2 = TDeviceUnitFactory.GetItem("Model2", new Guid("487704ea-ee63-41be-9e01-d1841dd472b8"), user1);
            var device3 = TDeviceUnitFactory.GetItem("Model3", new Guid("75d78874-5f39-4e22-bc30-ccc6743f4622"), user2);
            var device4 = TDeviceUnitFactory.GetItem("Model4", new Guid("8f030336-4861-4d9c-980a-38674fa2dcf5"), user2);
            var device5 = TDeviceUnitFactory.GetItem("Model5", new Guid("9966eb62-5e20-4a49-9eb1-e54614abe807"), user3);
            var device6 = TDeviceUnitFactory.GetItem("Model6", new Guid("6b9cab10-692f-4f4c-81b7-570a40d2b561"), user3);

            devices = new List<TDeviceUnit>() { device1, device2, device3, device4, device5, device6 };

            var position1 = TPositionUnitFactory.GetItem("Junior");
            var position2 = TPositionUnitFactory.GetItem("Middle");
            var position3 = TPositionUnitFactory.GetItem("Master");

            positions = new List<TPositionUnit>() { position1, position2, position3 };

            var document1 = TDocumentUnitFactory.GetItem("document1");
            var document2 = TDocumentUnitFactory.GetItem("document2");
            var document3 = TDocumentUnitFactory.GetItem("document3");
            var document4 = TDocumentUnitFactory.GetItem("document4");
            var document5 = TDocumentUnitFactory.GetItem("document5");
            var document6 = TDocumentUnitFactory.GetItem("document6");

            documents = new List<TDocumentUnit>() { document1, document2, document3, document4, document5, document6 };

            var book1 = TBookUnitFactory.GetItemWithText(new Guid("1b51edff-1d99-4043-9a69-209996729b69"), "book1", new List<string>() { "book1.page1", "book1.page2" });
            var book2 = TBookUnitFactory.GetItemWithText(new Guid("4782b530-6343-4d11-846a-65127cf71f3b"), "book2", new List<string>() { "book2.page1", "book2.page2", "book2.page3" });
            var book3 = TBookUnitFactory.GetItemWithText(new Guid("456fb3b7-6d98-40d2-a127-753d38fb5848"), "book3", new List<string>() { "book3.page1", "book3.page2", "book3.page3", "book3.page4" });

            books = new List<TBookUnit>() { book1, book2, book3 };
        }

        [Fact]
        public void GetUser_UsingPassportSerialNumber_CorrectUser()
        {
            // Init
            var dxFilter = "U2U(Passport).TPassportMainElement.SerialNumber = '6bcc2af44aa3'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\" ON \"{tableAlias_TPassportUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TPassportMainElement\" AS \"{tableAlias_TPassportMainElement_0}\" ON \"{tableAlias_TPassportMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TPassportUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TPassportMainElement_0}\".\"SerialNumber\" = '6bcc2af44aa3'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Single(usersExisting);

            Assert.Equal(new Guid("018fa54a-203e-7407-9bd0-cd287e850b03"), usersExisting.Single().Id);
        }

        [Fact]
        public void GetPassport_UsingUserNameAndSurname_CorrectPassport()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPassportUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TPassportUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TPassportUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPassportUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var passportsExisting = this._genericRepo.GetDXUnits<TPassportUnit>(dxFilter);

            // Checking result
            Assert.Single(passportsExisting);

            Assert.Equal(new Guid("018fa54a-37ae-709b-8890-76a6adc2a56b"), passportsExisting.Single().Id);
        }

        [Fact]
        public void GetUser_UsingDeviceUUID_CorrectUser()
        {
            // Init
            var dxFilter = "U2U(Devices).TDeviceMainElement.UUID = '9966eb62-5e20-4a49-9eb1-e54614abe807'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TDeviceUnit\" AS \"{tableAlias_TDeviceUnit_0}\" ON \"{tableAlias_TDeviceUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TDeviceMainElement\" AS \"{tableAlias_TDeviceMainElement_0}\" ON \"{tableAlias_TDeviceMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TDeviceUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TDeviceMainElement_0}\".\"UUID\" = '9966eb62-5e20-4a49-9eb1-e54614abe807'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Single(usersExisting);

            Assert.Equal(new Guid("018fa54a-109e-770b-927a-71016b1c6517"), usersExisting.Single().Id);
        }

        [Fact]
        public void GetDevices_UsingUserNameAndSurname_CorrectDevices()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDeviceUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TDeviceUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDeviceUnit\" AS \"{tableAlias_TDeviceUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TDeviceUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TDeviceUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var devicesExisting = this._genericRepo.GetDXUnits<TDeviceUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, devicesExisting.Count());

            Assert.Single(devicesExisting, x => x.Id == new Guid("58a98dbf-ce5d-43d1-adb2-670dea20c7bf"));
            Assert.Single(devicesExisting, x => x.Id == new Guid("36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b"));
        }

        // TODO: need to update expectedSQLQuery to check.
        [Fact]
        public void GetComputers_UsingUserNameAndSurname_Empty()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDeviceUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TDeviceUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDeviceUnit\" AS \"{tableAlias_TDeviceUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TDeviceUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TComputerUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            //Assert.Equal(expectedSQLQuery, query);

            // Action            
            var devicesExisting = this._genericRepo.GetDXUnits<TComputerUnit>(dxFilter);

            // Checking result
            Assert.Empty(devicesExisting);
        }

        [Fact]
        public void GetUser_UsingPositionWithEmptyUser_Empty()
        {
            // Init
            var dxFilter = "U2U(Position).TPositionMainElement.Name = 'Middle'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TPositionUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" = 'Middle'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Empty(usersExisting);
        }

        [Fact]
        public void GetUser_UsingPositionExistingUser_CorrectUser()
        {
            // Init
            var dxFilter = "U2U(Position).TPositionMainElement.Name = 'Master'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TPositionUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" = 'Master'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Single(usersExisting);

            Assert.Equal(new Guid("018fa54a-186e-733a-a2b1-3f643f84ac4c"), usersExisting.Single().Id);
        }

        [Fact]
        public void GetPosition_UsingUserWithEmptyPosition_Empty()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Victor' AND U2U(User).TUserMainElement.Surname = 'Suvorov'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPositionUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TPositionUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TPositionUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Victor'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorov'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPositionUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var positionsExisting = this._genericRepo.GetDXUnits<TPositionUnit>(dxFilter);

            // Checking result
            Assert.Empty(positionsExisting);
        }

        [Fact]
        public void GetPosition_UsingUserWithExistingPosition_CorrectPosition()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPositionUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TPositionUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TPositionUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPositionUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var positionsExisting = this._genericRepo.GetDXUnits<TPositionUnit>(dxFilter);

            // Checking result
            Assert.Single(positionsExisting);

            Assert.Equal(new Guid("018fa54a-3f7e-751e-824e-87f774fc0447"), positionsExisting.Single().Id);
        }

        [Fact]
        public void GetDocuments_UsingUserWithoutDocuments_Empty()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Pavel' AND U2U(User).TUserMainElement.Surname = 'Plamenev'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDocumentUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TDocumentUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDocumentUnit\" AS \"{tableAlias_TDocumentUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TDocumentUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Pavel'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Plamenev'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TDocumentUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var documentsExisting = this._genericRepo.GetDXUnits<TDocumentUnit>(dxFilter);

            // Checking result
            Assert.Empty(documentsExisting);
        }

        [Fact]
        public void GetDocuments_UsingUserWithExistingDocuments_CorrectDocuments()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDocumentUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TDocumentUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDocumentUnit\" AS \"{tableAlias_TDocumentUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TDocumentUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TDocumentUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var documentsExisting = this._genericRepo.GetDXUnits<TDocumentUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, documentsExisting.Count());

            Assert.Single(documentsExisting, x => x.Id == new Guid("ce7a2422-7df4-426a-b1fe-2a2090443246"));
            Assert.Single(documentsExisting, x => x.Id == new Guid("c2caacbe-f9c8-4409-8c65-535a3b530a3d"));
        }

        [Fact]
        public void GetBooks_UsingUserWithoutBooks_Empty()
        {
            // Init
            var dxFilter = "U2U(Users).TUserMainElement.Name = 'Pavel' AND U2U(Users).TUserMainElement.Surname = 'Plamenev'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TBookUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TBookUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\" = \"{tableAlias_TBookUnit_0}\".\"Id\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Pavel'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Plamenev'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TBookUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var booksExisting = this._genericRepo.GetDXUnits<TBookUnit>(dxFilter);

            // Checking result
            Assert.Empty(booksExisting);
        }

        [Fact]
        public void GetBooks_UsingUserWithExistingBooks_CorrectBooks()
        {
            // Init
            var dxFilter = "U2U(Users).TUserMainElement.Name = 'Svitlana' AND U2U(Users).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TBookUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TBookUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\" = \"{tableAlias_TBookUnit_0}\".\"Id\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TBookUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var booksExisting = this._genericRepo.GetDXUnits<TBookUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, booksExisting.Count());

            Assert.Single(booksExisting, x => x.Id == new Guid("1b51edff-1d99-4043-9a69-209996729b69"));
            Assert.Single(booksExisting, x => x.Id == new Guid("4782b530-6343-4d11-846a-65127cf71f3b"));
        }

        [Fact]
        public void GetUsers_UsingBookWithoutUsersByName_Empty()
        {
            // Init
            var dxFilter = "U2U(Books).TBookMainElement.Name = 'book3'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\" ON \"{tableAlias_TBookUnit_0}\".\"Id\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\"\nLEFT JOIN \"TBookMainElement\" AS \"{tableAlias_TBookMainElement_0}\" ON \"{tableAlias_TBookMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TBookUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TBookMainElement_0}\".\"Name\" = 'book3'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);
        }

        [Fact]
        public void GetUsers_UsingBookWithExistingUsersByName_CorrectUsers()
        {
            // Init
            var dxFilter = "U2U(Books).TBookMainElement.Name = 'book1'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\" ON \"{tableAlias_TBookUnit_0}\".\"Id\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\"\nLEFT JOIN \"TBookMainElement\" AS \"{tableAlias_TBookMainElement_0}\" ON \"{tableAlias_TBookMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TBookUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TBookMainElement_0}\".\"Name\" = 'book1'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, usersExisting.Count());

            Assert.Single(usersExisting, x => x.Id == new Guid("018fa54a-203e-7407-9bd0-cd287e850b03"));
            Assert.Single(usersExisting, x => x.Id == new Guid("018fa54a-186e-733a-a2b1-3f643f84ac4c"));
        }

        [Fact]
        public void GetPassport_UsingPositionWithUser_CorrectPassport()
        {
            // Init
            var dxFilter = "U2U(User).U2U(Position).TPositionMainElement.Name = 'Master'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPassportUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TPassportUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TPassportUnit_0}\".\"User\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TPositionUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" = 'Master'";

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPassportUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action
            var passportsExisting = this._genericRepo.GetDXUnits<TPassportUnit>(dxFilter);

            // Checking result
            Assert.Single(passportsExisting);

            Assert.Equal(new Guid("018fa54a-37ae-709b-8890-76a6adc2a56b"), passportsExisting.Single().Id);
        }

        [Fact]
        public void CheckOperators_UsingBaseOperators_CorrectQuery()
        {
            foreach (var operation in DXSQLOperators.BaseOperators)
            {
                // Init
                var dxFilter = $"U2U(User).U2U(Position).TPositionMainElement.Name {operation} 'Master'";
                string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPassportUnit_0}\".\"Id\" AS \"Id\",\n\"{tableAlias_TPassportUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"Id\" = \"{tableAlias_TPassportUnit_0}\".\"User\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"Id\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitId\" = \"{tableAlias_TPositionUnit_0}\".\"Id\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" {operation} 'Master'";

                // Action
                var query = this._sqlQueryBuilder.BuildSQLExpression("TPassportUnit", SQLQueryBuilder.BaseColumns, dxFilter);

                // Checking result
                Assert.Equal(expectedSQLQuery, query);

                // Action
                var passportsExisting = this._genericRepo.GetDXUnits<TPassportUnit>(dxFilter);

                Assert.NotNull(passportsExisting);
            }
        }

        [Fact]
        public void F()
        {
            string dxFilter = "U2U(Users).TUserMainElement.Name = 'Svitlana' AND U2U(Users).TUserMainElement.Surname = 'Suvorova'";

            Dictionary<string, string> columns = new Dictionary<string, string>();

            columns.Add("BookName", "TBookMainElement.Name");
            columns.Add("Name", "U2U(Users).TUserMainElement.Name");
            columns.Add("Surname", "U2U(Users).TUserMainElement.Surname");

            base.EstimatePerformanceAsync(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    var sqlWhereExpression = this._sqlQueryBuilder.BuildSQLExpression("TBookUnit", columns, dxFilter);
                }
                return Task.CompletedTask;
            }, "").Wait();


        }
    }
}
