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
        public ClaimLine(int id, string mainProcedureIcd10, string gender)
        {
            Id = id;
            MainProcedureIcd10 = mainProcedureIcd10;
            Gender = gender;
        }

        [QueryField(Formatter = typeof(CustomFieldFormatter), Description = "A unique id")]
        public int Id { get; set; }

        public string MainProcedureIcd10 { get; set; }

        public string Gender { get; set; }

        [QueryField(Ignore = true)] public string Ignored { get; set; }
    }

    public class RulesConnectorTest
    {
        
        private const string Url = "https://www.pacifichealthdynamics.com.au/cat";
        private const string User = "example";
        private const string Password = "example";
        private const int Tenant = 1;
        

        [Fact]
        public void TestPing()
        {
            QueryBuilder.Create(Url,
                    Tenant,
                    User,
                    Password)
                .Ping();
        }
        
        [Fact]
        public void TestSyncSchema()
        {
            QueryBuilder.Create(Url,
                    Tenant,
                    User,
                    Password)
                .WithDataSet<ClaimLine>("ClaimLine")
                .WithDataSet<ClaimLine>("ClaimLineHistory")
                .SyncSchema("default");
        }

        [Fact]
        public void TestQuery()
        {
            var results =
                QueryBuilder.Create(Url,
                        Tenant,
                        User,
                        Password)
                    .WithSelect("ClaimLine", "id", "ClaimLineId")
                    .WithSelect("ClaimLineHistory", "id", "ClaimLineHistoryId")
                    .WithDataSetRows("ClaimLine",
                        new ClaimLine(1, "0UT94ZZ", "female"),
                        new ClaimLine(2, "0UT94ZZ", "male"))
                    .Execute();
            Assert.True(results.Results.Any());
        }
    }
}
