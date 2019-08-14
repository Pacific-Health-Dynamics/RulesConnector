package com.swiftleap.rules.connector.exposed;


import com.fasterxml.jackson.databind.ObjectMapper;
import com.swiftleap.rules.connector.*;
import org.junit.Test;

import java.io.IOException;

public class QueryBuilderTest {

    private String endpoint = "https://www.swiftleap.com/rules";
    private String user = "example";
    private String password = "example";
    private int tenant = 0;

    @Test
    public void pingTest() {
        QueryBuilder.create(endpoint, tenant, user, password)
                .ping();
    }

    @Test
    public void syncSchemaTest() {
        QueryBuilder.create(endpoint, tenant, user, password)
                .withDataSet("ClaimLine", ClaimLine.class)
                .withDataSet("ClaimLineHistory", ClaimLine.class)
                .syncSchema("default");
    }

    @Test
    public void executeTest() throws IOException {

        ObjectMapper mapper = new ObjectMapper();

        QueryResults results =
                QueryBuilder.create(endpoint, tenant, user, password)
                        .withSelect("ClaimLine", "id", "ClaimLineId")
                        .withSelect("ClaimLineHistory", "id", "ClaimLineHistoryId")
                        .withDataSet("ClaimLine",
                                new ClaimLine(1, "0UT94ZZ", "female"),
                                new ClaimLine(2, "0UT94ZZ", "male"))
                        .execute();
        System.out.println(mapper.writeValueAsString(results));
    }

    /**
     * Example of a custom formatter.
     */
    public static class CustomFieldFormatter implements FieldFormatter {

        @Override
        public String format(Object value) {
            return value.toString();
        }
    }

    public class ClaimLine {
        int id;
        String mainProcedureICD10;
        String gender;

        public ClaimLine(int id, String mainProcedureICD10, String gender) {
            this.id = id;
            this.mainProcedureICD10 = mainProcedureICD10;
            this.gender = gender;
        }

        @QueryField(formatter = CustomFieldFormatter.class)
        public int getId() {
            return id;
        }

        public void setId(int id) {
            this.id = id;
        }

        @QueryField(description = "The procedure ICD10 code. Sample: '0UT94ZZ'")
        public String getMainProcedureICD10() {
            return mainProcedureICD10;
        }

        public void setMainProcedureICD10(String mainProcedureICD10) {
            this.mainProcedureICD10 = mainProcedureICD10;
        }

        @QueryField(description = "The gender. Sample: 'M' or 'F'")
        public String getGender() {
            return gender;
        }

        public void setGender(String gender) {
            this.gender = gender;
        }

        @QueryField(ignore = true)
        public String getIgnored() {
            //This property is ignored
            return null;
        }
    }
}
