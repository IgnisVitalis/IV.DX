using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Persistence.SQLQueryHelpers;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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

        const int dxUnitStartIndex = 14;
        const int dxElementStartIndex = 30;

        string tableRelation_TUserUnit_TBookUnit_0 = "Relation_TUserUnit_TBookUnit_0";
        string tableAlias_TPassportUnit_0 = $"T_{dxUnitStartIndex}_0";//TPassportUnit
        string tableAlias_TPositionUnit_0 = $"T_{dxUnitStartIndex + 1}_0";//TPositionUnit        
        string tableAlias_TDocumentUnit_0 = $"T_{dxUnitStartIndex + 2}_0";//TDocumentUnit
        string tableAlias_TDeviceUnit_0 = $"T_{dxUnitStartIndex + 3}_0";//TDeviceUnit
        string tableAlias_TBookUnit_0 = $"T_{dxUnitStartIndex + 4}_0";//TBookUnit     
        string tableAlias_TUserUnit_0 = $"T_{dxUnitStartIndex + 5}_0";//TUserUnit
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
            var user1 = TUserUnitFactory.GetItem(new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3"), "Victor", "Suvorov", new DateTime(1989, 9, 5));
            var user2 = TUserUnitFactory.GetItem(new Guid("dfb7bb88-30d9-46d7-9885-6ca8ae455e82"), "Svitlana", "Suvorova", new DateTime(1993, 11, 17));
            var user3 = TUserUnitFactory.GetItem(new Guid("60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff"), "Pavel", "Plamenev", new DateTime(1980, 1, 1));

            users = new List<TUserUnit>() { user1, user2, user3 };

            var passport1 = TPassportUnitFactory.GetItem(new Guid("1f852be3-f851-4807-807f-cfd45a0b4093"), "6bcc2af44aa3", user1);
            var passport2 = TPassportUnitFactory.GetItem(new Guid("bd56ffdd-0d30-4d9f-b879-7875162fc7b6"), "6ca8ae455e82", user2);
            var passport3 = TPassportUnitFactory.GetItem(new Guid("459276b0-69db-43da-bf9d-c2683c4b4d39"), "4a82ceaa1cff", user3);

            passports = new List<TPassportUnit>() { passport1, passport2, passport3 };

            var device1 = TDeviceUnitFactory.GetItem(new Guid("a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7"), "Model1", new Guid("70f86100-bc9c-4b88-8f5f-759cedf85972"), user1);
            var device2 = TDeviceUnitFactory.GetItem(new Guid("53ced1ab-2582-4aee-b2bc-50e676eebde3"), "Model2", new Guid("487704ea-ee63-41be-9e01-d1841dd472b8"), user1);
            var device3 = TDeviceUnitFactory.GetItem(new Guid("58a98dbf-ce5d-43d1-adb2-670dea20c7bf"), "Model3", new Guid("75d78874-5f39-4e22-bc30-ccc6743f4622"), user2);
            var device4 = TDeviceUnitFactory.GetItem(new Guid("36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b"), "Model4", new Guid("8f030336-4861-4d9c-980a-38674fa2dcf5"), user2);
            var device5 = TDeviceUnitFactory.GetItem(new Guid("24d8f6ff-b411-4acc-8a35-5e958ce7f070"), "Model5", new Guid("9966eb62-5e20-4a49-9eb1-e54614abe807"), user3);
            var device6 = TDeviceUnitFactory.GetItem(new Guid("1c16f974-8e52-408b-9cac-acbb548864fa"), "Model6", new Guid("6b9cab10-692f-4f4c-81b7-570a40d2b561"), user3);

            devices = new List<TDeviceUnit>() { device1, device2, device3, device4, device5, device6 };

            var position1 = TPositionUnitFactory.GetItem(new Guid("1f852be3-f851-4807-807f-cfd45a0b4093"), "Junior");
            var position2 = TPositionUnitFactory.GetItem(new Guid("14c92f6a-88b7-4d72-9777-5f655f0914bf"), "Middle");
            var position3 = TPositionUnitFactory.GetItem(new Guid("3040fe09-2ec2-4472-ae32-724f028b374e"), "Master");

            positions = new List<TPositionUnit>() { position1, position2, position3 };

            var document1 = TDocumentUnitFactory.GetItem(new Guid("ce7a2422-7df4-426a-b1fe-2a2090443246"), "document1");
            var document2 = TDocumentUnitFactory.GetItem(new Guid("a844e32e-fcf3-4f7e-b138-19685347a150"), "document2");
            var document3 = TDocumentUnitFactory.GetItem(new Guid("ccb9da2b-12ea-41c0-96d1-a774a3f4b22b"), "document3");
            var document4 = TDocumentUnitFactory.GetItem(new Guid("02e1591c-375b-466c-b8fb-0bed19220707"), "document4");
            var document5 = TDocumentUnitFactory.GetItem(new Guid("6a7c4e0d-1163-41ca-8a1a-e25fe8797100"), "document5");
            var document6 = TDocumentUnitFactory.GetItem(new Guid("c2caacbe-f9c8-4409-8c65-535a3b530a3d"), "document6");

            documents = new List<TDocumentUnit>() { document1, document2, document3, document4, document5, document6 };

            var book1 = TBookUnitFactory.GetItemWithText(new Guid("1b51edff-1d99-4043-9a69-209996729b69"), "book1", new List<string>() { "book1.page1", "book1.page2" });
            var book2 = TBookUnitFactory.GetItemWithText(new Guid("4782b530-6343-4d11-846a-65127cf71f3b"), "book2", new List<string>() { "book2.page1", "book2.page2", "book2.page3" });
            var book3 = TBookUnitFactory.GetItemWithText(new Guid("456fb3b7-6d98-40d2-a127-753d38fb5848"), "book3", new List<string>() { "book3.page1", "book3.page2", "book3.page3", "book3.page4" });

            books = new List<TBookUnit>() { book1, book2, book3 };
        }

        private IEnumerable<DXRelationDefinitionUnit> GetAllRelations()
        {
            var result = this._genericRepo.GetDXUnits<DXRelationDefinitionUnit>();

            return result;
        }

        [Fact]
        public void GetUser_UsingPassportSerialNumber_CorrectUser()
        {
            // Init
            var dxFilter = "U2U(Passport).TPassportMainElement.SerialNumber = '6bcc2af44aa3'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\" ON \"{tableAlias_TPassportUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TPassportMainElement\" AS \"{tableAlias_TPassportMainElement_0}\" ON \"{tableAlias_TPassportMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TPassportUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TPassportMainElement_0}\".\"SerialNumber\" = '6bcc2af44aa3'";

            var expectedUser = this.users.Single(x => x.ID == new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Single(usersExisting);

            var userExisting = usersExisting.Single();

            Assert.Equal(expectedUser.ID, userExisting.ID);
        }

        [Fact]
        public void GetPassport_UsingUserNameAndSurname_CorrectPassport()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPassportUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TPassportUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TPassportUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            var expectedPassport = this.passports.Single(x => x.ID == new Guid("bd56ffdd-0d30-4d9f-b879-7875162fc7b6"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPassportUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var passportsExisting = this._genericRepo.GetDXUnits<TPassportUnit>(dxFilter);

            // Checking result
            Assert.Single(passportsExisting);

            var passportExisting = passportsExisting.Single();

            Assert.Equal(expectedPassport.ID, passportExisting.ID);
        }

        [Fact]
        public void GetUser_UsingDeviceUUID_CorrectUser()
        {
            // Init
            var dxFilter = "U2U(Devices).TDeviceMainElement.UUID = '9966eb62-5e20-4a49-9eb1-e54614abe807'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TDeviceUnit\" AS \"{tableAlias_TDeviceUnit_0}\" ON \"{tableAlias_TDeviceUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TDeviceMainElement\" AS \"{tableAlias_TDeviceMainElement_0}\" ON \"{tableAlias_TDeviceMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TDeviceUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TDeviceMainElement_0}\".\"UUID\" = '9966eb62-5e20-4a49-9eb1-e54614abe807'";

            var expectedUser = this.users.Single(x => x.ID == new Guid("60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Single(usersExisting);

            var userExisting = usersExisting.Single();

            Assert.Equal(expectedUser.ID, userExisting.ID);
        }

        [Fact]
        public void GetDevices_UsingUserNameAndSurname_CorrectDevices()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Svitlana' AND U2U(User).TUserMainElement.Surname = 'Suvorova'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDeviceUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TDeviceUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDeviceUnit\" AS \"{tableAlias_TDeviceUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TDeviceUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            var expectedDevice1 = this.devices.Single(x => x.ID == new Guid("58a98dbf-ce5d-43d1-adb2-670dea20c7bf"));
            var expectedDevice2 = this.devices.Single(x => x.ID == new Guid("36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TDeviceUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var devicesExisting = this._genericRepo.GetDXUnits<TDeviceUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, devicesExisting.Count());

            var passportExisting1 = devicesExisting.Single(x => x.ID == expectedDevice1.ID);

            Assert.Equal(expectedDevice1.ID, passportExisting1.ID);

            var passportExisting2 = devicesExisting.Single(x => x.ID == expectedDevice2.ID);

            Assert.Equal(expectedDevice2.ID, passportExisting2.ID);
        }

        [Fact]
        public void GetUser_UsingPositionWithEmptyUser_Empty()
        {
            // Init
            var dxFilter = "U2U(Position).TPositionMainElement.Name = 'Middle'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TPositionUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" = 'Middle'";

            var relations = this.GetAllRelations();

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
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TPositionUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" = 'Master'";

            var expectedUser = this.users.Single(x => x.ID == new Guid("dfb7bb88-30d9-46d7-9885-6ca8ae455e82"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Single(usersExisting);

            var userExisting = usersExisting.Single();

            Assert.Equal(expectedUser.ID, userExisting.ID);
        }

        [Fact]
        public void GetPosition_UsingUserWithEmptyPosition_Empty()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Victor' AND U2U(User).TUserMainElement.Surname = 'Suvorov'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPositionUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TPositionUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TPositionUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Victor'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorov'";

            var relations = this.GetAllRelations();

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
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPositionUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TPositionUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TPositionUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            var expectedPosition = this.positions.Single(x => x.ID == new Guid("3040fe09-2ec2-4472-ae32-724f028b374e"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPositionUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var positionsExisting = this._genericRepo.GetDXUnits<TPositionUnit>(dxFilter);

            // Checking result
            Assert.Single(positionsExisting);

            var positionExisting = positionsExisting.Single();

            Assert.Equal(expectedPosition.ID, positionExisting.ID);
        }

        [Fact]
        public void GetDocuments_UsingUserWithoutDocuments_Empty()
        {
            // Init
            var dxFilter = "U2U(User).TUserMainElement.Name = 'Pavel' AND U2U(User).TUserMainElement.Surname = 'Plamenev'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDocumentUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TDocumentUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDocumentUnit\" AS \"{tableAlias_TDocumentUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TDocumentUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Pavel'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Plamenev'";

            var relations = this.GetAllRelations();

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
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TDocumentUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TDocumentUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TDocumentUnit\" AS \"{tableAlias_TDocumentUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TDocumentUnit_0}\".\"User\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            var expectedDocument1 = this.documents.Single(x => x.ID == new Guid("ce7a2422-7df4-426a-b1fe-2a2090443246"));
            var expectedDocument6 = this.documents.Single(x => x.ID == new Guid("c2caacbe-f9c8-4409-8c65-535a3b530a3d"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TDocumentUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var positionsExisting = this._genericRepo.GetDXUnits<TDocumentUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, positionsExisting.Count());

            var positionExisting1 = positionsExisting.Single(x => x.ID == expectedDocument1.ID);

            Assert.Equal(expectedDocument1.ID, positionExisting1.ID);

            var positionExisting6 = positionsExisting.Single(x => x.ID == expectedDocument6.ID);

            Assert.Equal(expectedDocument6.ID, positionExisting6.ID);
        }

        [Fact]
        public void GetBooks_UsingUserWithoutBooks_Empty()
        {
            // Init
            var dxFilter = "U2U(Users).TUserMainElement.Name = 'Pavel' AND U2U(Users).TUserMainElement.Surname = 'Plamenev'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TBookUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TBookUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\" = \"{tableAlias_TBookUnit_0}\".\"ID\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Pavel'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Plamenev'";

            var relations = this.GetAllRelations();

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
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TBookUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TBookUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\" = \"{tableAlias_TBookUnit_0}\".\"ID\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\"\nLEFT JOIN \"TUserMainElement\" AS \"{tableAlias_TUserMainElement_0}\" ON \"{tableAlias_TUserMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TUserMainElement_0}\".\"Name\" = 'Svitlana'  AND  \"{tableAlias_TUserMainElement_0}\".\"Surname\" = 'Suvorova'";

            var expectedBook1 = this.books.Single(x => x.ID == new Guid("1b51edff-1d99-4043-9a69-209996729b69"));
            var expectedBook2 = this.books.Single(x => x.ID == new Guid("4782b530-6343-4d11-846a-65127cf71f3b"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TBookUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var booksExisting = this._genericRepo.GetDXUnits<TBookUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, booksExisting.Count());

            var bookExisting1 = booksExisting.Single(x => x.ID == expectedBook1.ID);

            Assert.Equal(expectedBook1.ID, bookExisting1.ID);

            var bookExisting2 = booksExisting.Single(x => x.ID == expectedBook2.ID);

            Assert.Equal(expectedBook2.ID, bookExisting2.ID);
        }

        [Fact]
        public void GetUsers_UsingBookWithoutUsersByName_Empty()
        {
            // Init
            var dxFilter = "U2U(Books).TBookMainElement.Name = 'book3'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\" ON \"{tableAlias_TBookUnit_0}\".\"ID\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\"\nLEFT JOIN \"TBookMainElement\" AS \"{tableAlias_TBookMainElement_0}\" ON \"{tableAlias_TBookMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TBookUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TBookMainElement_0}\".\"Name\" = 'book3'";

            var relations = this.GetAllRelations();

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
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TUserUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TUserUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\"\nLEFT JOIN \"{tableRelation_TUserUnit_TBookUnit_0}\" AS \"{tableRelation_TUserUnit_TBookUnit_0}\" ON \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Users\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TBookUnit\" AS \"{tableAlias_TBookUnit_0}\" ON \"{tableAlias_TBookUnit_0}\".\"ID\" = \"{tableRelation_TUserUnit_TBookUnit_0}\".\"Books\"\nLEFT JOIN \"TBookMainElement\" AS \"{tableAlias_TBookMainElement_0}\" ON \"{tableAlias_TBookMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TBookUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TBookMainElement_0}\".\"Name\" = 'book1'";

            var expectedUser1 = this.users.Single(x => x.ID == new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3"));
            var expectedUser2 = this.users.Single(x => x.ID == new Guid("dfb7bb88-30d9-46d7-9885-6ca8ae455e82"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TUserUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetDXUnits<TUserUnit>(dxFilter);

            // Checking result
            Assert.Equal(2, usersExisting.Count());

            var userExisting1 = usersExisting.Single(x => x.ID == expectedUser1.ID);

            Assert.Equal(expectedUser1.ID, userExisting1.ID);

            var userExisting2 = usersExisting.Single(x => x.ID == expectedUser2.ID);

            Assert.Equal(expectedUser2.ID, userExisting2.ID);
        }

        [Fact]
        public void GetPassport_UsingPositionWithUser_CorrectPassport()
        {
            // Init
            var dxFilter = "U2U(User).U2U(Position).TPositionMainElement.Name = 'Master'";
            string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPassportUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TPassportUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TPassportUnit_0}\".\"User\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TPositionUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" = 'Master'";

            var expectedPassport = this.passports.Single(x => x.ID == new Guid("bd56ffdd-0d30-4d9f-b879-7875162fc7b6"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryBuilder.BuildSQLExpression("TPassportUnit", SQLQueryBuilder.BaseColumns, dxFilter);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var passportsExisting = this._genericRepo.GetDXUnits<TPassportUnit>(dxFilter);

            // Checking result
            Assert.Single(passportsExisting);

            var passportExisting = passportsExisting.Single();

            Assert.Equal(expectedPassport.ID, passportExisting.ID);
        }

        [Fact]
        public void CheckOperators_UsingBaseOperators_CorrectQuery()
        {
            foreach (var operation in DXSQLOperators.BaseOperators)
            {
                // Init
                var dxFilter = $"U2U(User).U2U(Position).TPositionMainElement.Name {operation} 'Master'";
                string expectedSQLQuery = $"SELECT\n\"{tableAlias_TPassportUnit_0}\".\"ID\" AS \"ID\",\n\"{tableAlias_TPassportUnit_0}\".\"TimeStamp\" AS \"TimeStamp\"\nFROM\n\"TPassportUnit\" AS \"{tableAlias_TPassportUnit_0}\"\nLEFT JOIN \"TUserUnit\" AS \"{tableAlias_TUserUnit_0}\" ON \"{tableAlias_TUserUnit_0}\".\"ID\" = \"{tableAlias_TPassportUnit_0}\".\"User\"\nLEFT JOIN \"TPositionUnit\" AS \"{tableAlias_TPositionUnit_0}\" ON \"{tableAlias_TPositionUnit_0}\".\"User\" = \"{tableAlias_TUserUnit_0}\".\"ID\"\nLEFT JOIN \"TPositionMainElement\" AS \"{tableAlias_TPositionMainElement_0}\" ON \"{tableAlias_TPositionMainElement_0}\".\"DXUnitID\" = \"{tableAlias_TPositionUnit_0}\".\"ID\"\nWHERE\n\"{tableAlias_TPositionMainElement_0}\".\"Name\" {operation} 'Master'";

                var relations = this.GetAllRelations();

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

            base.EstimatePerformanceAsync(async () =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    var sqlWhereExpression = this._sqlQueryBuilder.BuildSQLExpression("TBookUnit", columns, dxFilter);
                }
            }, "").Wait();


        }
    }
}
