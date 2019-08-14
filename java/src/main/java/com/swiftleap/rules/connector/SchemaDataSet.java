package com.swiftleap.rules.connector;

import java.util.ArrayList;
import java.util.Collection;

class SchemaDataSet {
    String name;
    Collection<SchemaColumnDef> columns = new ArrayList<>(0);

    public SchemaDataSet() {
    }

    public SchemaDataSet(String name, Collection<SchemaColumnDef> columns) {
        this.name = name;
        this.columns = columns;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public Collection<SchemaColumnDef> getColumns() {
        return columns;
    }

    public void setColumns(Collection<SchemaColumnDef> columns) {
        this.columns = columns;
    }
}
