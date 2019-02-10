package com.swiftleap.rules.connector;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationConfig;
import com.fasterxml.jackson.databind.SerializationFeature;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.*;

public class QueryBuilder {
    private ObjectMapper mapper = new ObjectMapper();
    private String endpoint;
    private int tenantId;
    private CharSequence user;
    private CharSequence password;
    private Query query = new Query();

    private QueryBuilder() {
        SerializationConfig sc = mapper.getSerializationConfig();
        mapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
        mapper.setSerializationInclusion(JsonInclude.Include.NON_NULL);
        mapper.configure(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false);
    }


    public static QueryBuilder create(String endpoint, int tenantId, CharSequence user, CharSequence password) {
        QueryBuilder ret = new QueryBuilder();
        ret.endpoint = endpoint;
        ret.password = password;
        ret.user = user;
        ret.tenantId = tenantId;
        return ret;
    }

    public QueryBuilder withSelect(String dataSetName, String columnName, String alias) {
        QuerySelect select = new QuerySelect();
        select.setDataSetName(dataSetName);
        select.setColumnName(columnName);
        select.setAlias(alias);
        query.getSelect().add(select);
        return this;
    }

    public <T> QueryBuilder withDataSet(String dataSetName, T... rows) throws QueryException {
        return withDataSet(dataSetName, Arrays.asList(rows));
    }

    public <T> QueryBuilder withDataSet(String dataSetName, Iterable<T> rows) throws QueryException {
        List<QueryRow> queryRows = new ArrayList<>();

        Iterator<T> iter = rows.iterator();
        while (iter.hasNext()) {
            T next = iter.next();
            if (next == null)
                continue;
            QueryRow row = new QueryRow();
            row.setValues(describe(next));
            queryRows.add(row);
        }

        QueryDataSet ds = new QueryDataSet();
        ds.setName(dataSetName);
        ds.setRows(queryRows);
        query.getDataSets().add(ds);
        return this;
    }

    public QueryResults execute() throws IOException, QueryException {
        if(query.getSelect().isEmpty())
            throw new QueryException("No selection");
        if(query.getDataSets().isEmpty())
            throw new QueryException("No data-sets");


        byte[] data = mapper.writeValueAsBytes(query);

        URL url = new URL(endpoint);
        HttpURLConnection connection = (HttpURLConnection) url.openConnection();
        String userCredentials = user + ":" + password;
        String basicAuth = "Basic " + new String(Base64.encode(userCredentials.getBytes()));

        connection.setRequestProperty("Authorization", basicAuth);
        connection.setRequestMethod("POST");
        connection.setRequestProperty("X-TenantId", String.valueOf(tenantId));
        connection.setRequestProperty("Content-Type", "application/json");
        connection.setRequestProperty("Content-Length", "" + data.length);
        connection.setUseCaches(false);
        connection.setDoInput(true);
        connection.setDoOutput(true);

        try (OutputStream out = connection.getOutputStream()) {
            out.write(data);
        }

        int responseCode = connection.getResponseCode();
        String responseMessage = connection.getResponseMessage();

        if (responseCode < 200 || responseCode > 299) {
            if(responseMessage == null)
                responseMessage = "";
            throw new QueryException(String.format("Error: %d %s", responseCode, responseMessage));
        }

        try (InputStream in = connection.getInputStream()) {
            ByteArrayOutputStream bout = new ByteArrayOutputStream();
            byte[] buff = new byte[2048];
            int read = 0;
            while ((read = in.read(buff)) > 0) {
                bout.write(buff, 0, read);
            }
            return mapper.readValue(bout.toByteArray(), QueryResults.class);
        }
    }

    private Map<String, String> describe(Object object) throws QueryException {
        try {
            Map<String, String> map = new HashMap<>();
            for (Method method : object.getClass().getMethods()) {
                if (method.getName().startsWith("get")) {
                    if (method.getName().equals("getClass"))
                        continue;

                    QueryField field = method.getAnnotation(QueryField.class);
                    if (field != null && field.ignore())
                        continue;

                    FieldFormatter formatter;
                    String propName = "";

                    if (field != null) {
                        formatter = field.formatter().newInstance();
                        propName = field.value();
                    } else {
                        formatter = new FieldFormatter.DefaultFieldFormatter();
                    }

                    if (propName.isEmpty())
                        propName = toPropName(method.getName());

                    map.put(propName, formatter.format(method.invoke(object)));
                }
            }
            return map;
        } catch (IllegalAccessException
                | IllegalArgumentException
                | InvocationTargetException
                | InstantiationException e) {
            throw new QueryException(e);
        }
    }

    private String toPropName(String name) {
        if (!name.startsWith("get"))
            return name;
        return Character.toLowerCase(name.charAt(3)) + name.substring(4);
    }
}
