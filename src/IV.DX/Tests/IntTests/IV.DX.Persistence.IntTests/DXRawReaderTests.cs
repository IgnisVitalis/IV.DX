using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Persistence.IntTests
{
    [Collection("DX:one-time")]
    public class DXRawReaderTests : IntTestController
    {
        IDXRawReader _dxRawReader;
        ISQLQueryBuilder _sqlQueryBuilder;

        public DXRawReaderTests(DXTestFixture fx, ITestOutputHelper output)
            : base(fx, output)
        {
            this._dxRawReader = this.ServiceProvider.GetRequiredService<IDXRawReader>();
            this._sqlQueryBuilder = this.ServiceProvider.GetRequiredService<ISQLQueryBuilder>();
        }

        [Fact]
        public void Get_UsingColumnsFromRelatedDXUnitsWithSameDXElement_Ok()
        {
            // Init
            var columns = new Dictionary<string, string>
            {
                { "Name", "Name" } ,
                { "AccountName", "U2U(Account).Name"},
                { "RoleNameFromMembership", "DXRoleElement.E2U(Role).Name" } ,
                { "RoleNameFromAccount", "U2U(Account).DXRoleElement.E2U(Role).Name" } ,
            };

            var sql = this._sqlQueryBuilder.BuildSQLExpression("DXMembershipUnit", columns);
            var aliasMatches = Regex.Matches(
                sql,
                "^(FROM|LEFT JOIN)\\s+\"[^\"]+\"\\s+AS\\s+\"(?<alias>[^\"]+)\"",
                RegexOptions.Multiline);

            var aliases = aliasMatches.Select(m => m.Groups["alias"].Value).ToList();
            Assert.Equal(aliases.Distinct().Count(), aliases.Count);

            // Action
            var result = this._dxRawReader.Get("DXMembershipUnit", columns);

            // Assert

        }

        [Fact]
        public void BuildSQLExpression_UsingWrongExpression_ThrowsReadableError()
        {
            var columns = new Dictionary<string, string>
            {
                { "RoleNameFromAccountWrong", "E2U(Account).DXRoleElement.E2U(Role).Name" }
            };

            var ex = Assert.Throws<InvalidOperationException>(() =>
                this._sqlQueryBuilder.BuildSQLExpression("DXMembershipUnit", columns));

            Assert.Contains("RoleNameFromAccountWrong", ex.Message);
            Assert.Contains("E2U(Account)", ex.Message);
            Assert.Contains("DXMembershipUnit", ex.Message);
        }
    }
}
