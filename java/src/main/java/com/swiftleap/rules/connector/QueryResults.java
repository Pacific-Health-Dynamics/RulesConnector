package com.swiftleap.rules.connector;


import java.util.ArrayList;
import java.util.List;


public class QueryResults {
    private List<QueryResult> results = new ArrayList<>(0);

    public List<QueryResult> getResults() {
        return results;
    }

    public void setResults(List<QueryResult> results) {
        this.results = results;
    }
}
