package com.swiftleap.rules.connector;

import java.util.ArrayList;
import java.util.Collection;

class Schema {
    public String name;
    Collection<SchemaDataSet> dataSets = new ArrayList<>(0);

    public Schema() {
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public Collection<SchemaDataSet> getDataSets() {
        return dataSets;
    }

    public void setDataSets(Collection<SchemaDataSet> dataSets) {
        this.dataSets = dataSets;
    }
}
