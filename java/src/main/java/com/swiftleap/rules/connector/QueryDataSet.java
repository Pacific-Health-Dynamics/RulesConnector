package com.swiftleap.rules.connector;


import java.util.ArrayList;
import java.util.List;


public class QueryDataSet {
    private String name;
    private List<QueryRow> rows = new ArrayList<>(0);


    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public List<QueryRow> getRows() {
        return rows;
    }

    public void setRows(List<QueryRow> rows) {
        this.rows = rows;
    }
}
