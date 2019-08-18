package com.swiftleap.rules.connector;


import java.util.HashMap;
import java.util.Map;


public class QueryResult {
    private Map<String, String> select = new HashMap<>(0);
    private String ruleCode;
    private String ruleId;
    private String name;
    private String message;
    private int severity;
    private int version;
    private String mappedCode;

    public QueryResult() {
    }

    public Map<String, String> getSelect() {
        return select;
    }

    public void setSelect(Map<String, String> select) {
        this.select = select;
    }

    public String getRuleCode() {
        return ruleCode;
    }

    public void setRuleCode(String ruleCode) {
        this.ruleCode = ruleCode;
    }

    public String getRuleId() {
        return ruleId;
    }

    public void setRuleId(String ruleId) {
        this.ruleId = ruleId;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public int getSeverity() {
        return severity;
    }

    public void setSeverity(int severity) {
        this.severity = severity;
    }

    public String getMappedCode() {
        return mappedCode;
    }

    public void setMappedCode(String mappedCode) {
        this.mappedCode = mappedCode;
    }

    public int getVersion() {
        return version;
    }

    public void setVersion(int version) {
        this.version = version;
    }
}
