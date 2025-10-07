using IV.DX.Contracts.Common.Helpers;
using System.Linq;
using Xunit;

namespace IV.DataProvider.Persistence.Common.UnitTests.Helpers
{
    public class StringHelperUnitTests
    {
        [Fact]
        public void SplitAndKeep_UsingSeveralCharDelimeters_CorrectResult()
        {
            // Init
            string str = "FirstPart.SecondPart,ThirdPart;FourthPart,FifthPart";
            char[] delims = new char[] { '.', ',', ';' };

            // Action
            var result = str.SplitAndKeep(delims).ToList();

            // Checking result
            Assert.Null(result[0].Value);
            Assert.Equal("FirstPart", result[0].Key);
            Assert.Equal('.', result[1].Value);
            Assert.Equal("SecondPart", result[1].Key);
            Assert.Equal(',', result[2].Value);
            Assert.Equal("ThirdPart", result[2].Key);
            Assert.Equal(';', result[3].Value);
            Assert.Equal("FourthPart", result[3].Key);
            Assert.Equal(',', result[4].Value);
            Assert.Equal("FifthPart", result[4].Key);
        }

        [Fact]
        public void SplitAndKeep_UsingSeveralStringDelimeters_CorrectResult()
        {
            // Init
            string str = "FirstPart AND SecondPart OR ThirdPart AND FourthPart OR FifthPart";
            string[] delims = new string[] { "AND", "OR" };

            // Action
            var result = str.SplitAndKeep(delims, System.StringSplitOptions.TrimEntries).ToList();

            // Checking result
            Assert.Null(result[0].Value);
            Assert.Equal("FirstPart", result[0].Key);
            Assert.Equal("AND", result[1].Value);
            Assert.Equal("SecondPart", result[1].Key);
            Assert.Equal("OR", result[2].Value);
            Assert.Equal("ThirdPart", result[2].Key);
            Assert.Equal("AND", result[3].Value);
            Assert.Equal("FourthPart", result[3].Key);
            Assert.Equal("OR", result[4].Value);
            Assert.Equal("FifthPart", result[4].Key);
        }
    }
}