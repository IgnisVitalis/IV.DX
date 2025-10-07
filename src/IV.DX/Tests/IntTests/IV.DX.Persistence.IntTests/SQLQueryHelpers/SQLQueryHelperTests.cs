using IV.DataProvider.Persistence.Shared.IntTests;
using IV.DataProvider.Persistence.Shared.IntTests.Factories.Test;
using IV.DataProvider.Persistence.Shared.IntTests.Models.Test;
using IV.DX.Contracts.Common.Models;
using IV.DX.Contracts.Persistence.ExpressionTree;
using IV.DX.Persistence.SQLQueryHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IV.DataProvider.Persistence.Repositories.IntTests.SQLQueryHelpers
{
    public class SQLQueryHelperTests : IntTestController
    {
        public IEnumerable<TUserObject> users;
        public IEnumerable<TDeviceObject> devices;
        public IEnumerable<TPassportObject> passports;
        public IEnumerable<TPositionObject> positions;
        public IEnumerable<TDocumentObject> documents;
        public IEnumerable<TBookObject> books;

        public SQLQueryHelperTests(ITestOutputHelper output)
            : base(output)
        {
            InitData();
        }

        private void InitData()
        {
            // Init           
            var user1 = TUserObjectFactory.GetItem(new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3"), "Victor", "Suvorov", new DateTime(1989, 9, 5));
            var user2 = TUserObjectFactory.GetItem(new Guid("dfb7bb88-30d9-46d7-9885-6ca8ae455e82"), "Svitlana", "Suvorova", new DateTime(1993, 11, 17));
            var user3 = TUserObjectFactory.GetItem(new Guid("60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff"), "Pavel", "Plamenev", new DateTime(1980, 1, 1));

            users = new List<TUserObject>() { user1, user2, user3 };

            var passport1 = TPassportObjectFactory.GetItem(new Guid("1f852be3-f851-4807-807f-cfd45a0b4093"), "6bcc2af44aa3", user1);
            var passport2 = TPassportObjectFactory.GetItem(new Guid("bd56ffdd-0d30-4d9f-b879-7875162fc7b6"), "6ca8ae455e82", user2);
            var passport3 = TPassportObjectFactory.GetItem(new Guid("459276b0-69db-43da-bf9d-c2683c4b4d39"), "4a82ceaa1cff", user3);

            passports = new List<TPassportObject>() { passport1, passport2, passport3 };

            var device1 = TDeviceObjectFactory.GetItem(new Guid("a03f744d-d5db-4d4e-95a8-d5fbf4bad2d7"), "Model1", new Guid("70f86100-bc9c-4b88-8f5f-759cedf85972"), user1);
            var device2 = TDeviceObjectFactory.GetItem(new Guid("53ced1ab-2582-4aee-b2bc-50e676eebde3"), "Model2", new Guid("487704ea-ee63-41be-9e01-d1841dd472b8"), user1);
            var device3 = TDeviceObjectFactory.GetItem(new Guid("58a98dbf-ce5d-43d1-adb2-670dea20c7bf"), "Model3", new Guid("75d78874-5f39-4e22-bc30-ccc6743f4622"), user2);
            var device4 = TDeviceObjectFactory.GetItem(new Guid("36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b"), "Model4", new Guid("8f030336-4861-4d9c-980a-38674fa2dcf5"), user2);
            var device5 = TDeviceObjectFactory.GetItem(new Guid("24d8f6ff-b411-4acc-8a35-5e958ce7f070"), "Model5", new Guid("9966eb62-5e20-4a49-9eb1-e54614abe807"), user3);
            var device6 = TDeviceObjectFactory.GetItem(new Guid("1c16f974-8e52-408b-9cac-acbb548864fa"), "Model6", new Guid("6b9cab10-692f-4f4c-81b7-570a40d2b561"), user3);

            devices = new List<TDeviceObject>() { device1, device2, device3, device4, device5, device6 };

            var position1 = TPositionObjectFactory.GetItem(new Guid("1f852be3-f851-4807-807f-cfd45a0b4093"), "Junior");
            var position2 = TPositionObjectFactory.GetItem(new Guid("14c92f6a-88b7-4d72-9777-5f655f0914bf"), "Middle");
            var position3 = TPositionObjectFactory.GetItem(new Guid("3040fe09-2ec2-4472-ae32-724f028b374e"), "Master");

            positions = new List<TPositionObject>() { position1, position2, position3 };

            var document1 = TDocumentObjectFactory.GetItem(new Guid("ce7a2422-7df4-426a-b1fe-2a2090443246"), "document1");
            var document2 = TDocumentObjectFactory.GetItem(new Guid("a844e32e-fcf3-4f7e-b138-19685347a150"), "document2");
            var document3 = TDocumentObjectFactory.GetItem(new Guid("ccb9da2b-12ea-41c0-96d1-a774a3f4b22b"), "document3");
            var document4 = TDocumentObjectFactory.GetItem(new Guid("02e1591c-375b-466c-b8fb-0bed19220707"), "document4");
            var document5 = TDocumentObjectFactory.GetItem(new Guid("6a7c4e0d-1163-41ca-8a1a-e25fe8797100"), "document5");
            var document6 = TDocumentObjectFactory.GetItem(new Guid("c2caacbe-f9c8-4409-8c65-535a3b530a3d"), "document6");

            documents = new List<TDocumentObject>() { document1, document2, document3, document4, document5, document6 };

            var book1 = TBookObjectFactory.GetItemWithText(new Guid("1b51edff-1d99-4043-9a69-209996729b69"), "book1", new List<string>() { "book1.page1", "book1.page2" });
            var book2 = TBookObjectFactory.GetItemWithText(new Guid("4782b530-6343-4d11-846a-65127cf71f3b"), "book2", new List<string>() { "book2.page1", "book2.page2", "book2.page3" });
            var book3 = TBookObjectFactory.GetItemWithText(new Guid("456fb3b7-6d98-40d2-a127-753d38fb5848"), "book3", new List<string>() { "book3.page1", "book3.page2", "book3.page3", "book3.page4" });

            books = new List<TBookObject>() { book1, book2, book3 };
        }

        private IEnumerable<DPRelationObject> GetAllRelations()
        {
            var result = this._genericRepo.GetItems<DPRelationObject>();

            return result;
        }

        [Fact]
        public void GetUser_UsingPassportSerialNumber_CorrectUser()
        {
            // Init
            var whereExpression = "R(Passport).TPassportGenBlock.SerialNumber = '6bcc2af44aa3'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TUserObject\" AS \"t_0_0\" LEFT JOIN \"TPassportObject\" AS \"t_1_0\" ON \"t_1_0\".\"User\" = \"t_0_0\".\"ID\" LEFT JOIN \"TPassportGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"SerialNumber\" = '6bcc2af44aa3';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TUserObject AS t_0_0 LEFT JOIN TPassportObject AS t_1_0 ON t_1_0.User = t_0_0.ID LEFT JOIN TPassportGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.SerialNumber = '6bcc2af44aa3';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedUser = this.users.Single(x => x.ID == new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TUserObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetItems<TUserObject>(whereExpression);

            // Checking result
            Assert.Single(usersExisting);

            var userExisting = usersExisting.Single();

            Assert.Equal(expectedUser.ID, userExisting.ID);
        }

        [Fact]
        public void GetPassport_UsingUserNameAndSurname_CorrectPassport()
        {
            // Init
            var whereExpression = "R(User).TUserGenBlock.Name = 'Svitlana' AND R(User).TUserGenBlock.Surname = 'Suvorova'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TPassportObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Svitlana' AND \"t_2_0\".\"Surname\" = 'Suvorova';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TPassportObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Svitlana' AND t_2_0.Surname = 'Suvorova';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedPassport = this.passports.Single(x => x.ID == new Guid("bd56ffdd-0d30-4d9f-b879-7875162fc7b6"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TPassportObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var passportsExisting = this._genericRepo.GetItems<TPassportObject>(whereExpression);

            // Checking result
            Assert.Single(passportsExisting);

            var passportExisting = passportsExisting.Single();

            Assert.Equal(expectedPassport.ID, passportExisting.ID);
        }

        [Fact]
        public void GetUser_UsingDeviceUUID_CorrectUser()
        {
            // Init
            var whereExpression = "R(Devices).TDeviceGenBlock.UUID = '9966eb62-5e20-4a49-9eb1-e54614abe807'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TUserObject\" AS \"t_0_0\" LEFT JOIN \"TDeviceObject\" AS \"t_1_0\" ON \"t_1_0\".\"User\" = \"t_0_0\".\"ID\" LEFT JOIN \"TDeviceGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"UUID\" = '9966eb62-5e20-4a49-9eb1-e54614abe807';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TUserObject AS t_0_0 LEFT JOIN TDeviceObject AS t_1_0 ON t_1_0.User = t_0_0.ID LEFT JOIN TDeviceGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.UUID = '9966eb62-5e20-4a49-9eb1-e54614abe807';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedUser = this.users.Single(x => x.ID == new Guid("60e7ebaa-66f8-41a5-ab40-4a82ceaa1cff"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TUserObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetItems<TUserObject>(whereExpression);

            // Checking result
            Assert.Single(usersExisting);

            var userExisting = usersExisting.Single();

            Assert.Equal(expectedUser.ID, userExisting.ID);
        }

        [Fact]
        public void GetDevices_UsingUserNameAndSurname_CorrectDevices()
        {
            // Init
            var whereExpression = "R(User).TUserGenBlock.Name = 'Svitlana' AND R(User).TUserGenBlock.Surname = 'Suvorova'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TDeviceObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Svitlana' AND \"t_2_0\".\"Surname\" = 'Suvorova';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TDeviceObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Svitlana' AND t_2_0.Surname = 'Suvorova';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedDevice1 = this.devices.Single(x => x.ID == new Guid("58a98dbf-ce5d-43d1-adb2-670dea20c7bf"));
            var expectedDevice2 = this.devices.Single(x => x.ID == new Guid("36ab0a14-f382-4c3a-aefa-fa5cb3c1e00b"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TDeviceObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var devicesExisting = this._genericRepo.GetItems<TDeviceObject>(whereExpression);

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
            var whereExpression = "R(Position).TPositionGenBlock.Name = 'Middle'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TUserObject\" AS \"t_0_0\" LEFT JOIN \"TPositionObject\" AS \"t_1_0\" ON \"t_1_0\".\"User\" = \"t_0_0\".\"ID\" LEFT JOIN \"TPositionGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Middle';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TUserObject AS t_0_0 LEFT JOIN TPositionObject AS t_1_0 ON t_1_0.User = t_0_0.ID LEFT JOIN TPositionGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Middle';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TUserObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetItems<TUserObject>(whereExpression);

            // Checking result
            Assert.Empty(usersExisting);
        }

        [Fact]
        public void GetUser_UsingPositionExistingUser_CorrectUser()
        {
            // Init
            var whereExpression = "R(Position).TPositionGenBlock.Name = 'Master'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TUserObject\" AS \"t_0_0\" LEFT JOIN \"TPositionObject\" AS \"t_1_0\" ON \"t_1_0\".\"User\" = \"t_0_0\".\"ID\" LEFT JOIN \"TPositionGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Master';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TUserObject AS t_0_0 LEFT JOIN TPositionObject AS t_1_0 ON t_1_0.User = t_0_0.ID LEFT JOIN TPositionGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Master';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedUser = this.users.Single(x => x.ID == new Guid("dfb7bb88-30d9-46d7-9885-6ca8ae455e82"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TUserObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetItems<TUserObject>(whereExpression);

            // Checking result
            Assert.Single(usersExisting);

            var userExisting = usersExisting.Single();

            Assert.Equal(expectedUser.ID, userExisting.ID);
        }

        [Fact]
        public void GetPosition_UsingUserWithEmptyPosition_Empty()
        {
            // Init
            var whereExpression = "R(User).TUserGenBlock.Name = 'Victor' AND R(User).TUserGenBlock.Surname = 'Suvorov'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TPositionObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Victor' AND \"t_2_0\".\"Surname\" = 'Suvorov';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TPositionObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Victor' AND t_2_0.Surname = 'Suvorov';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TPositionObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var positionsExisting = this._genericRepo.GetItems<TPositionObject>(whereExpression);

            // Checking result
            Assert.Empty(positionsExisting);
        }

        [Fact]
        public void GetPosition_UsingUserWithExistingPosition_CorrectPosition()
        {
            // Init
            var whereExpression = "R(User).TUserGenBlock.Name = 'Svitlana' AND R(User).TUserGenBlock.Surname = 'Suvorova'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TPositionObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Svitlana' AND \"t_2_0\".\"Surname\" = 'Suvorova';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TPositionObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Svitlana' AND t_2_0.Surname = 'Suvorova';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedPosition = this.positions.Single(x => x.ID == new Guid("3040fe09-2ec2-4472-ae32-724f028b374e"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TPositionObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var positionsExisting = this._genericRepo.GetItems<TPositionObject>(whereExpression);

            // Checking result
            Assert.Single(positionsExisting);

            var positionExisting = positionsExisting.Single();

            Assert.Equal(expectedPosition.ID, positionExisting.ID);
        }

        [Fact]
        public void GetDocuments_UsingUserWithoutDocuments_Empty()
        {
            // Init
            var whereExpression = "R(User).TUserGenBlock.Name = 'Pavel' AND R(User).TUserGenBlock.Surname = 'Plamenev'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TDocumentObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Pavel' AND \"t_2_0\".\"Surname\" = 'Plamenev';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TDocumentObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Pavel' AND t_2_0.Surname = 'Plamenev';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TDocumentObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var documentsExisting = this._genericRepo.GetItems<TDocumentObject>(whereExpression);

            // Checking result
            Assert.Empty(documentsExisting);
        }

        [Fact]
        public void GetDocuments_UsingUserWithExistingDocuments_CorrectDocuments()
        {
            // Init
            var whereExpression = "R(User).TUserGenBlock.Name = 'Svitlana' AND R(User).TUserGenBlock.Surname = 'Suvorova'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TDocumentObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Svitlana' AND \"t_2_0\".\"Surname\" = 'Suvorova';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TDocumentObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Svitlana' AND t_2_0.Surname = 'Suvorova';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedDocument1 = this.documents.Single(x => x.ID == new Guid("ce7a2422-7df4-426a-b1fe-2a2090443246"));
            var expectedDocument6 = this.documents.Single(x => x.ID == new Guid("c2caacbe-f9c8-4409-8c65-535a3b530a3d"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TDocumentObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var positionsExisting = this._genericRepo.GetItems<TDocumentObject>(whereExpression);

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
            var whereExpression = "R(Users).TUserGenBlock.Name = 'Pavel' AND R(Users).TUserGenBlock.Surname = 'Plamenev'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TBookObject\" AS \"t_0_0\" LEFT JOIN \"Relation_TUserObject_TBookObject_0\" AS \"t_1_0_int\" ON \"t_1_0_int\".\"Books\" = \"t_0_0\".\"ID\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_1_0_int\".\"Users\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Pavel' AND \"t_2_0\".\"Surname\" = 'Plamenev';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TBookObject AS t_0_0 LEFT JOIN Relation_TUserObject_TBookObject_0 AS t_1_0_int ON t_1_0_int.Books = t_0_0.ID LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_1_0_int.Users LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Pavel' AND t_2_0.Surname = 'Plamenev';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TBookObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var booksExisting = this._genericRepo.GetItems<TBookObject>(whereExpression);

            // Checking result
            Assert.Empty(booksExisting);
        }

        [Fact]
        public void GetBooks_UsingUserWithExistingBooks_CorrectBooks()
        {
            // Init
            var whereExpression = "R(Users).TUserGenBlock.Name = 'Svitlana' AND R(Users).TUserGenBlock.Surname = 'Suvorova'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TBookObject\" AS \"t_0_0\" LEFT JOIN \"Relation_TUserObject_TBookObject_0\" AS \"t_1_0_int\" ON \"t_1_0_int\".\"Books\" = \"t_0_0\".\"ID\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_1_0_int\".\"Users\" LEFT JOIN \"TUserGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'Svitlana' AND \"t_2_0\".\"Surname\" = 'Suvorova';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TBookObject AS t_0_0 LEFT JOIN Relation_TUserObject_TBookObject_0 AS t_1_0_int ON t_1_0_int.Books = t_0_0.ID LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_1_0_int.Users LEFT JOIN TUserGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'Svitlana' AND t_2_0.Surname = 'Suvorova';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedBook1 = this.books.Single(x => x.ID == new Guid("1b51edff-1d99-4043-9a69-209996729b69"));
            var expectedBook2 = this.books.Single(x => x.ID == new Guid("4782b530-6343-4d11-846a-65127cf71f3b"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TBookObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var booksExisting = this._genericRepo.GetItems<TBookObject>(whereExpression);

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
            var whereExpression = "R(Books).TBookGenBlock.Name = 'book3'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TUserObject\" AS \"t_0_0\" LEFT JOIN \"Relation_TUserObject_TBookObject_0\" AS \"t_1_0_int\" ON \"t_1_0_int\".\"Users\" = \"t_0_0\".\"ID\" LEFT JOIN \"TBookObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_1_0_int\".\"Books\" LEFT JOIN \"TBookGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'book3';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TUserObject AS t_0_0 LEFT JOIN Relation_TUserObject_TBookObject_0 AS t_1_0_int ON t_1_0_int.Users = t_0_0.ID LEFT JOIN TBookObject AS t_1_0 ON t_1_0.ID = t_1_0_int.Books LEFT JOIN TBookGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'book3';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TUserObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetItems<TUserObject>(whereExpression);
        }

        [Fact]
        public void GetUsers_UsingBookWithExistingUsersByName_CorrectUsers()
        {
            // Init
            var whereExpression = "R(Books).TBookGenBlock.Name = 'book1'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TUserObject\" AS \"t_0_0\" LEFT JOIN \"Relation_TUserObject_TBookObject_0\" AS \"t_1_0_int\" ON \"t_1_0_int\".\"Users\" = \"t_0_0\".\"ID\" LEFT JOIN \"TBookObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_1_0_int\".\"Books\" LEFT JOIN \"TBookGenBlock\" AS \"t_2_0\" ON \"t_2_0\".\"ObjectID\" = \"t_1_0\".\"ID\" WHERE \"t_2_0\".\"Name\" = 'book1';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TUserObject AS t_0_0 LEFT JOIN Relation_TUserObject_TBookObject_0 AS t_1_0_int ON t_1_0_int.Users = t_0_0.ID LEFT JOIN TBookObject AS t_1_0 ON t_1_0.ID = t_1_0_int.Books LEFT JOIN TBookGenBlock AS t_2_0 ON t_2_0.ObjectID = t_1_0.ID WHERE t_2_0.Name = 'book1';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedUser1 = this.users.Single(x => x.ID == new Guid("8d8b5eb0-9fc6-44c9-a185-6bcc2af44aa3"));
            var expectedUser2 = this.users.Single(x => x.ID == new Guid("dfb7bb88-30d9-46d7-9885-6ca8ae455e82"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TUserObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var usersExisting = this._genericRepo.GetItems<TUserObject>(whereExpression);

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
            var whereExpression = "R(User).R(Position).TPositionGenBlock.Name = 'Master'";
            string expectedSQLQuery = null;

            if (base._sqlQueryHelper is PGSQLQueryHelper)
            {
                expectedSQLQuery = "SELECT \"t_0_0\".\"ID\" FROM \"TPassportObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TPositionObject\" AS \"t_2_0\" ON \"t_2_0\".\"User\" = \"t_1_0\".\"ID\" LEFT JOIN \"TPositionGenBlock\" AS \"t_3_0\" ON \"t_3_0\".\"ObjectID\" = \"t_2_0\".\"ID\" WHERE \"t_3_0\".\"Name\" = 'Master';";
            }
            else if (base._sqlQueryHelper is MySQLQueryHelper)
            {
                expectedSQLQuery = "SELECT t_0_0.ID FROM TPassportObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TPositionObject AS t_2_0 ON t_2_0.User = t_1_0.ID LEFT JOIN TPositionGenBlock AS t_3_0 ON t_3_0.ObjectID = t_2_0.ID WHERE t_3_0.Name = 'Master';";
            }
            else
            {
                throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
            }

            var expectedPassport = this.passports.Single(x => x.ID == new Guid("bd56ffdd-0d30-4d9f-b879-7875162fc7b6"));
            var relations = this.GetAllRelations();

            // Action
            var query = this._sqlQueryHelper.GetQuery("TPassportObject", whereExpression, relations);

            // Checking result
            Assert.Equal(expectedSQLQuery, query);

            // Action            
            var passportsExisting = this._genericRepo.GetItems<TPassportObject>(whereExpression);

            // Checking result
            Assert.Single(passportsExisting);

            var passportExisting = passportsExisting.Single();

            Assert.Equal(expectedPassport.ID, passportExisting.ID);
        }

        [Fact]
        public void CheckOperators_UsingBaseOperators_CorrectQuery()
        {
            foreach (var operation in ESQLOperators.BaseOperators)
            {
                // Init
                var whereExpression = $"R(User).R(Position).TPositionGenBlock.Name {operation} 'Master'";
                string expectedSQLQuery = null;

                if (base._sqlQueryHelper is PGSQLQueryHelper)
                {
                    expectedSQLQuery = $"SELECT \"t_0_0\".\"ID\" FROM \"TPassportObject\" AS \"t_0_0\" LEFT JOIN \"TUserObject\" AS \"t_1_0\" ON \"t_1_0\".\"ID\" = \"t_0_0\".\"User\" LEFT JOIN \"TPositionObject\" AS \"t_2_0\" ON \"t_2_0\".\"User\" = \"t_1_0\".\"ID\" LEFT JOIN \"TPositionGenBlock\" AS \"t_3_0\" ON \"t_3_0\".\"ObjectID\" = \"t_2_0\".\"ID\" WHERE \"t_3_0\".\"Name\" {operation} 'Master';";
                }
                else if (base._sqlQueryHelper is MySQLQueryHelper)
                {
                    expectedSQLQuery = $"SELECT t_0_0.ID FROM TPassportObject AS t_0_0 LEFT JOIN TUserObject AS t_1_0 ON t_1_0.ID = t_0_0.User LEFT JOIN TPositionObject AS t_2_0 ON t_2_0.User = t_1_0.ID LEFT JOIN TPositionGenBlock AS t_3_0 ON t_3_0.ObjectID = t_2_0.ID WHERE t_3_0.Name {operation} 'Master';";
                }
                else
                {
                    throw new Exception($"Please define sql query for {base._sqlQueryHelper.GetType()}");
                }

                var relations = this.GetAllRelations();

                // Action
                var query = this._sqlQueryHelper.GetQuery("TPassportObject", whereExpression, relations);

                // Checking result
                Assert.Equal(expectedSQLQuery, query);

                // Action
                var passportsExisting = this._genericRepo.GetItems<TPassportObject>(whereExpression);

                Assert.NotNull(passportsExisting);
            }
        }
    }
}
