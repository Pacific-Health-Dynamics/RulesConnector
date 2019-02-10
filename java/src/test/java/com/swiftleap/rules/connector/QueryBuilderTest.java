package com.swiftleap.rules.connector;


import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.Test;

import java.io.IOException;

public class QueryBuilderTest {

    /**
     * Example of an custom formatter.
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

        public String getMainProcedureICD10() {
            return mainProcedureICD10;
        }

        public void setMainProcedureICD10(String mainProcedureICD10) {
            this.mainProcedureICD10 = mainProcedureICD10;
        }

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

    @Test
    public void executeTest() throws IOException, QueryException {
        ObjectMapper mapper = new ObjectMapper();

        QueryResults results =
                QueryBuilder.create("https://www.swiftleap.com/rules/api/v1/rules/query",
                0,
                "example",
                "example")
                .withSelect("ClaimLine", "id", "ClaimLineId")
                .withSelect("ClaimLineHistory", "id", "ClaimLineHistoryId")
                .withDataSet("ClaimLine",
                        new ClaimLine(1, "0UT94ZZ", "female"),
                        new ClaimLine(2, "0UT94ZZ", "male"))
                .execute();
        System.out.println(mapper.writeValueAsString(results));
    }
}
