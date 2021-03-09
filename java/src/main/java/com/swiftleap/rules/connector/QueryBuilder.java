package com.swiftleap.rules.connector;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.lang.reflect.Method;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.*;
import java.util.function.Function;

public class QueryBuilder {
    private ObjectMapper mapper = new ObjectMapper();
    private String endpoint;
    private int tenantId;
    private CharSequence user;
    private CharSequence password;
    private Query query = new Query();
    private Map<String, Class<?>> dataSets = new HashMap<>();

    private QueryBuilder() {
        mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
        mapper.setSerializationInclusion(JsonInclude.Include.NON_NULL);
        mapper.configure(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false);
    }

    public static QueryBuilder create(String baseUrl, int tenantId, CharSequence user, CharSequence password) {
        QueryBuilder ret = new QueryBuilder();
        ret.endpoint = baseUrl;
        ret.password = password;
        ret.user = user;
        ret.tenantId = tenantId;
        return ret;
    }

    private HttpURLConnection getConnection(String method, String path) throws IOException {
        URL url = new URL(endpoint + "/" + path);
        HttpURLConnection connection = (HttpURLConnection) url.openConnection();
        String userCredentials = user + ":" + password;
        String basicAuth = "Basic " + new String(Base64.encode(userCredentials.getBytes()));

        connection.setRequestProperty("Authorization", basicAuth);
        connection.setRequestMethod(method);
        connection.setRequestProperty("X-TenantId", String.valueOf(tenantId));
        connection.setUseCaches(false);
        return connection;
    }

    private byte[] readResponse(HttpURLConnection connection) throws IOException {
        try (InputStream in = connection.getInputStream()) {
            ByteArrayOutputStream bout = new ByteArrayOutputStream();
            byte[] buff = new byte[2048];
            int read = 0;
            while ((read = in.read(buff)) > 0) {
                bout.write(buff, 0, read);
            }
            return bout.toByteArray();
        }
    }

    private <T> T httpGet(String path, Function<HttpURLConnection, T> con) {
        HttpURLConnection connection = null;
        try {
            connection = getConnection("GET", path);
            processResponseStatus(connection);
            return con.apply(connection);
        } catch (IOException ex) {
            throw new QueryException(ex);
        } finally {
            if (connection != null)
                connection.disconnect();
        }
    }

    private <T> T httpPost(String path, Object object, Function<HttpURLConnection, T> con) {
        HttpURLConnection connection = null;
        try {
            byte[] data = mapper.writeValueAsBytes(object);

            connection = getConnection("POST", path);

            connection.setRequestProperty("Content-Type", "application/json");
            connection.setRequestProperty("Content-Length", "" + data.length);
            connection.setDoInput(true);
            connection.setDoOutput(true);

            try (OutputStream out = connection.getOutputStream()) {
                out.write(data);
            }

            processResponseStatus(connection);
            return con.apply(connection);
        } catch (IOException ex) {
            throw new QueryException(ex);
        } finally {
            if (connection != null)
                connection.disconnect();
        }
    }

    public QueryBuilder withSelect(String dataSetName, String columnName, String alias) {
        QuerySelect select = new QuerySelect();
        select.setDataSetName(dataSetName);
        select.setColumnName(columnName);
        select.setAlias(alias);
        query.getSelect().add(select);
        return this;
    }

    public <T> QueryBuilder withDataSet(String dataSetName, Class<T> type) {
        dataSets.put(dataSetName, type);
        return this;
    }

    public <T> QueryBuilder withDataSet(String dataSetName, T... rows) {
        return withDataSet(dataSetName, Arrays.asList(rows));
    }

    public <T> QueryBuilder withDataSet(String dataSetName, Iterable<T> rows) {
        Collection<SchemaColumnDef> fields = null;
        List<QueryRow> queryRows = new ArrayList<>();

        Iterator<T> iter = rows.iterator();
        while (iter.hasNext()) {
            T next = iter.next();
            if (next == null)
                continue;
            if (fields == null) {
                dataSets.put(dataSetName, next.getClass());
                fields = describeFields(next.getClass());
            }

            QueryRow row = new QueryRow();
            row.setValues(describe(fields, next));
            queryRows.add(row);
        }

        QueryDataSet ds = new QueryDataSet();
        ds.setName(dataSetName);
        ds.setRows(queryRows);
        query.getDataSets().add(ds);
        return this;
    }

    private void processResponseStatus(HttpURLConnection connection) throws IOException {
        int responseCode = connection.getResponseCode();
        String responseMessage = connection.getResponseMessage();

        if (responseCode < 200 || responseCode > 299) {
            if (responseMessage == null)
                responseMessage = "";
            try {
                Error error = mapper.readValue(readResponse(connection), Error.class);
                throw new QueryException(String.format("Error: %d %s (%s)", error.getCode(), error.getMessage(), error.getReference()));
            } catch (Exception e) {
            }

            throw new QueryException(String.format("Error: %d %s", responseCode, responseMessage));
        }
    }

    public void ping() {
        httpGet("api/v1/system/ping", con -> true);
    }

    public void syncSchema(String name) {
        if (dataSets.isEmpty())
            throw new QueryException("No data-sets");

        Schema schema = new Schema();
        schema.setName(name);

        for (Map.Entry<String, Class<?>> e : dataSets.entrySet()) {
            Collection<SchemaColumnDef> fields = describeFields(e.getValue());
            schema.getDataSets().add(new SchemaDataSet(e.getKey(), fields));
        }

        httpPost("api/v1/rules/schema/sync", schema, connection -> {
            try {
                readResponse(connection);
                return true;
            } catch (IOException e) {
                throw new QueryException(e);
            }
        });
    }

    public QueryResults execute() {
        if (query.getSelect().isEmpty())
            throw new QueryException("No selection");
        if (query.getDataSets().isEmpty())
            throw new QueryException("No data-sets");

        return httpPost("api/v1/rules/query", query, connection -> {
            try {
                return mapper.readValue(readResponse(connection), QueryResults.class);
            } catch (IOException e) {
                throw new QueryException(e);
            }
        });
    }

    private Collection<SchemaColumnDef> describeFields(Class<?> clazz) {
        Collection<SchemaColumnDef> fields = new ArrayList<>();
        for (Method method : clazz.getMethods()) {
            if (method.getName().startsWith("get")) {
                if (method.getName().equals("getClass"))
                    continue;

                QueryField queryField = method.getAnnotation(QueryField.class);
                if (queryField != null && queryField.ignore())
                    continue;

                SchemaColumnDef field = new SchemaColumnDef(method);
                fields.add(field);
            }
        }
        return fields;
    }

    private Map<String, String> describe(Collection<SchemaColumnDef> fields, Object object) {

        Map<String, String> map = new HashMap<>();
        for (SchemaColumnDef field : fields) {
            String value = field.invoke(object);
            if(value != null) map.put(field.getName(), value);
        }
        return map;
    }
}
