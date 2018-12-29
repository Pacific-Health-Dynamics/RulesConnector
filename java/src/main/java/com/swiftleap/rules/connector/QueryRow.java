package com.swiftleap.rules.connector;


import java.util.HashMap;
import java.util.Map;


public class QueryRow {
    private Map<String, String> values = new HashMap<>(0);

    public Map<String, String> getValues() {
        return values;
    }

    public void setValues(Map<String, String> values) {
        this.values = values;
    }
}
