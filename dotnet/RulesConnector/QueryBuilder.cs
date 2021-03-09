using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace SwiftLeap.RulesConnector
{
    public class QueryBuilder
    {
        private static readonly JsonSerializerSettings SerializationConfig = new DefaultJsonSerializerSettings();
        private readonly Query _query = new Query();
        private string _baseUrl;
        private string _password;
        private int _tenantId;
        private string _user;
        private IDictionary<string, Type> _dataSets = new Dictionary<string, Type>();

        private QueryBuilder()
        {
        }


        public static QueryBuilder Create(string baseUrl, int tenantId, string user, string password)
        {
            return new QueryBuilder {_baseUrl = baseUrl, _password = password, _user = user, _tenantId = tenantId};
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

        //Convenient method
        public QueryBuilder WithDataSetRows<T>(string dataSetName, params T[] rows)
        {
            return WithDataSet(dataSetName, rows);
        }

        public QueryBuilder WithDataSet<T>(string dataSetName)
        {
            if(!_dataSets.ContainsKey(dataSetName))
                _dataSets[dataSetName] = typeof(T);
            return this;
        }

        public QueryBuilder WithDataSet<T>(string dataSetName, IEnumerable<T> rows)
        {
            IList<SchemaColumnDef> fields = null;
            var queryRows = new List<QueryRow>();

            foreach (var next in rows)
            {
                if (next == null)
                    continue;
                if (fields == null)
                {
                    fields = DescribeFields(typeof(T)).ToList();
                    if(!_dataSets.ContainsKey(dataSetName))
                        _dataSets[dataSetName] = typeof(T);
                }

                var row = new QueryRow {Values = Describe(fields, next)};
                queryRows.Add(row);
            }

            _query.DataSets.Add(new QueryDataSet {Name = dataSetName, Rows = queryRows});
            return this;
        }


        public void Ping()
        {
            try
            {
                Do(client => client.GetAsync("api/v1/system/ping"));
            }
            catch (Exception ex)
            {
                throw UnwrapException(ex);
            }
        }

        public void SyncSchema(string name)
        {
            if (_dataSets.Count < 1)
                throw new QueryException("No data-sets");
            
            try
            {
                var schema = new Schema();
                schema.Name = name;

                foreach (var entry in _dataSets)
                {
                    var fields = DescribeFields(entry.Value).ToList();
                    schema.DataSets.Add(new SchemaDataSet(entry.Key, fields));
                }
                
                Do(client => client.PostAsync("api/v1/rules/schema/sync", ToContent(schema)));
            }
            catch (Exception ex)
            {
                throw UnwrapException(ex);
            }
        }

        public QueryResults Execute()
        {
            if (_query.Select.Count < 1)
                throw new QueryException("No selection");
            if (_query.DataSets.Count < 1)
                throw new QueryException("No data-sets");
            try
            {
                return Do<QueryResults>(client => client.PostAsync("api/v1/rules/query", ToContent(_query))).Result;
            }
            catch (Exception ex)
            {
                throw UnwrapException(ex);
            }
        }

        private Exception UnwrapException(Exception exception)
        {
            while (exception is AggregateException)
                exception = exception.InnerException;
            return exception;
        }

        private static IEnumerable<SchemaColumnDef> DescribeFields(Type type)
        {
            foreach (var prop in type.GetProperties())
            {
                var field = prop.GetCustomAttribute<QueryFieldAttribute>();
                if (field != null && field.Ignore)
                    continue;
                yield return new SchemaColumnDef(prop);
            }
        }

        private static IDictionary<string, string> Describe(IEnumerable<SchemaColumnDef> fields, object obj)
        {
            IDictionary<string, string> map = new Dictionary<string, string>();
            foreach (var def in fields)
            {
                if (!map.ContainsKey(def.Name))
                {
                    var value = def.Invoke(obj);
                    if(value != null)
                        map.Add(def.Name, def.Format(def.Invoke(obj)));
                }
            }
            return map;
        }

        private void Do(Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            using (var clientHandler = GetClientHandler())
            using (var client = GetClient(clientHandler))
            {
                var result = func(client).Result;
                if (result.StatusCode == HttpStatusCode.NoContent)
                    return;
                if (result.IsSuccessStatusCode)
                    return;
                throw ProcessError(result).Result;
            }
        }

        private async Task<TResult> Do<TResult>(Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            using (var clientHandler = GetClientHandler())
            using (var client = GetClient(clientHandler))
            {
                var result = await func(client);
                if (result.StatusCode == HttpStatusCode.NoContent)
                    return default(TResult);
                if (result.IsSuccessStatusCode)
                {
                    var body = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(body))
                        return JsonConvert.DeserializeObject<TResult>(body);
                }

                throw await ProcessError(result);
            }
        }

        private async Task<Exception> ProcessError(HttpResponseMessage result)
        {
            var errorBody = await result.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(errorBody))
                return new QueryException((int) result.StatusCode, result.ReasonPhrase);
            var error = JsonConvert.DeserializeObject<Error>(errorBody, SerializationConfig);
            return new QueryException(error);
        }

        private HttpClientHandler GetClientHandler()
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return clientHandler;
        }

        private HttpClient GetClient(HttpClientHandler handler)
        {
            var userCredentials = Encoding.UTF8.GetBytes(_user + ":" + _password);
            var basicAuth = Convert.ToBase64String(userCredentials);

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", basicAuth);
            client.DefaultRequestHeaders.Add("X-TenantId", _tenantId.ToString());
            return client;
        }

        private HttpContent ToContent<TType>(TType content)
        {
            var body = JsonConvert.SerializeObject(content, SerializationConfig);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        public class DefaultJsonSerializerSettings : JsonSerializerSettings
        {
            public DefaultJsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver();
                MissingMemberHandling = MissingMemberHandling.Ignore;
                Converters.Add(new StringEnumConverter());
            }
        }
    }
}