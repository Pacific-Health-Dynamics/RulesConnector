using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SwiftLeap.RulesConnector
{
    public class QueryBuilder
    {
        private readonly Query _query = new Query();
        private string _baseUrl;
        private string _password;
        private int _tenantId;
        private string _user;
        private static readonly JsonSerializerSettings SerializationConfig = new DefaultJsonSerializerSettings();

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

        private static IDictionary<string, string> Describe(object obj)
        {
            var defaultFieldFormatter = new DefaultFieldFormatter();
            IDictionary<string, string> map = new Dictionary<string, string>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var field = prop.GetCustomAttribute<QueryFieldAttribute>();
                if (field != null && field.Ignore)
                    continue;

                IFieldFormatter formatter;
                var propName = "";

                if (field != null)
                {
                    formatter = (IFieldFormatter) Activator.CreateInstance(field.Formatter);
                    propName = field.Alias;
                }
                else
                {
                    formatter = defaultFieldFormatter;
                }

                if (string.IsNullOrWhiteSpace(propName))
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
        
        public class DefaultJsonSerializerSettings : JsonSerializerSettings
        {
            public DefaultJsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver();
                MissingMemberHandling = MissingMemberHandling.Ignore;
            }
        }
        
        private HttpContent ToContent<TType>(TType content)
        {
            var body = JsonConvert.SerializeObject(content, SerializationConfig);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}