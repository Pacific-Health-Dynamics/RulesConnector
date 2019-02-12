using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SwiftLeap.RulesConnector
{
    public class QueryBuilder
    {
        private readonly Query _query = new Query();
        private string _endpoint;
        private string _password;
        private int _tenantId;
        private string _user;

        private QueryBuilder()
        {
        }


        public static QueryBuilder Create(string endpoint, int tenantId, string user, string password)
        {
            return new QueryBuilder {_endpoint = endpoint, _password = password, _user = user, _tenantId = tenantId};
        }

        public QueryBuilder WithSelect(string dataSetName, string columnName, string alias)
        {
            var select = new QuerySelect();
            select.DataSetName = dataSetName;
            select.ColumnName = columnName;
            select.Alias = alias;
            _query.Select.Add(select);
            return this;
        }

        public QueryBuilder WithDataSet<T>(string dataSetName, params T[] rows)
        {
            return WithDataSet(dataSetName, (IEnumerable<T>) rows);
        }

        public QueryBuilder WithDataSet<T>(string dataSetName, IEnumerable<T> rows)
        {
            var queryRows = new List<QueryRow>();

            foreach (var next in rows)
            {
                if (next == null)
                    continue;
                var row = new QueryRow {Values = Describe(next)};
                queryRows.Add(row);
            }

            _query.DataSets.Add(new QueryDataSet {Name = dataSetName, Rows = queryRows});
            return this;
        }

        public async Task<QueryResults> Execute()
        {
            if (_query.Select.Count < 1)
                throw new QueryException("No selection");
            if (_query.DataSets.Count < 1)
                throw new QueryException("No data-sets");

            var uri = new Uri(_endpoint);

            var json = JsonConvert.SerializeObject(_query);

            var userCredentials = Encoding.UTF8.GetBytes(_user + ":" + _password);
            var basicAuth = "Basic " + Convert.ToBase64String(userCredentials);

            using (var client = new HttpClient())
            {
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    stringContent.Headers.Add("X-TenantId", _tenantId.ToString());
                    stringContent.Headers.Add("Authorization", basicAuth);

                    using (var response = await client.PostAsync(uri, stringContent))
                    {
                        response.EnsureSuccessStatusCode();
                        json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<QueryResults>(json);
                    }
                }
            }
        }

        private static IDictionary<string, string> Describe(object obj)
        {
            var defaultFieldFormatter = new DefaultFieldFormatter();
            IDictionary<string, string> map = new Dictionary<string, string>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var field = prop.GetCustomAttribute<QueryFieldAttribute>();
                if (field != null && field.Ignore)
                    continue;

                FieldFormatter formatter;
                var propName = "";

                if (field != null)
                {
                    formatter = (FieldFormatter) Activator.CreateInstance(field.Formatter);
                    propName = field.Alias;
                }
                else
                {
                    formatter = defaultFieldFormatter;
                }

                if (propName.Length < 1)
                    propName = ToPropName(prop.Name);

                if (!map.ContainsKey(propName))
                    map.Add(propName, formatter.Format(prop.GetMethod.Invoke(obj, null)));
            }

            return map;
        }

        private static string ToPropName(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1);
        }
    }
}