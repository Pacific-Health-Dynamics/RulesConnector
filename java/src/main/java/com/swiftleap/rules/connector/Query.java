package com.swiftleap.rules.connector;


import java.util.ArrayList;
import java.util.List;


public class Query {
    private List<QuerySelect> select = new ArrayList<>(0);
    private List<QueryDataSet> dataSets = new ArrayList<>(0);

    public List<QuerySelect> getSelect() {
        return select;
    }

    public void setSelect(List<QuerySelect> select) {
        this.select = select;
    }

    public List<QueryDataSet> getDataSets() {
        return dataSets;
    }

    public void setDataSets(List<QueryDataSet> dataSets) {
        this.dataSets = dataSets;
    }
}
