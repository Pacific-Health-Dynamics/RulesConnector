using System;
using System.Linq;
using SwiftLeap.RulesConnector;
using Xunit;

namespace RulesConnectorTests
{
    /**
    * Example of an custom formatter.
    */
    public class CustomFieldFormatter : IFieldFormatter
    {
        public string Format(object value)
        {
            return value?.ToString();
        }
    }

    public class ClaimLine
    {
        [QueryField(Formatter = typeof(CustomFieldFormatter))]
        public int Id { get; set; }

        public string MainProcedureIcd10 { get; set; }
        
        public string Gender { get; set; }

        [QueryField(Ignore = true)]
        public string Ignored { get; set; }

        public ClaimLine(int id, string mainProcedureIcd10, string gender)
        {
            Id = id;
            MainProcedureIcd10 = mainProcedureIcd10;
            Gender = gender;
        }
    }

    public class RulesConnectorTest
    {
        [Fact]
        public void TestPing()
        {
            QueryBuilder.Create("https://www.swiftleap.com/rules", 0, "example", "example")
                .Ping();
        }

        [Fact]
        public void TestQuery()
        {
            var results =
                QueryBuilder.Create("https://www.swiftleap.com/rules",
                        0,
                        "example",
                        "example")
                    .WithSelect("ClaimLine", "id", "ClaimLineId")
                    .WithSelect("ClaimLineHistory", "id", "ClaimLineHistoryId")
                    .WithDataSet("ClaimLine",
                        new ClaimLine(1, "0UT94ZZ", "female"),
                        new ClaimLine(2, "0UT94ZZ", "male"))
                    .Execute();
            Assert.True(results.Results.Any());
        }
    }
}