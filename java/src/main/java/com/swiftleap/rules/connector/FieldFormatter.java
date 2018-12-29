package com.swiftleap.rules.connector;

import java.text.SimpleDateFormat;
import java.util.Date;

public interface FieldFormatter {
    String format(Object value);

    class DefaultFieldFormatter implements FieldFormatter {
        SimpleDateFormat df = new SimpleDateFormat("dd/MM/yyyy");

        @Override
        public String format(Object value) {
            if (value == null)
                return null;
            if (value instanceof Date)
                return df.format((Date) value);
            return value.toString();
        }
    }
}
