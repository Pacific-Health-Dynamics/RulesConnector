package com.swiftleap.rules.connector;


import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Date;

class SchemaColumnDef implements FieldFormatter {
    public static FieldFormatter DEFAULT_FIELD_FORMATTER = new FieldFormatter.DefaultFieldFormatter();

    private String name;
    private String description;
    private Type type = Type.NULL;
    private FieldFormatter formatter;
    private Method method;

    public SchemaColumnDef() {
    }

    public SchemaColumnDef(Method method) {
        try {
            this.method = method;

            QueryField field = method.getAnnotation(QueryField.class);

            if (field != null) {
                formatter = field.formatter().newInstance();
                name = field.value();
                description = field.description();
                type = field.type();
            } else {
                formatter = DEFAULT_FIELD_FORMATTER;
            }

            if (name.isEmpty())
                name = toPropName(method.getName());

            if (type == Type.NULL) {
                type = typeFromJavaType(method.getReturnType());
            }
        } catch (InstantiationException | IllegalAccessException ex) {
            throw new QueryException(ex);
        }
    }

    private static String toPropName(String name) {
        if (!name.startsWith("get"))
            return name;
        return Character.toLowerCase(name.charAt(3)) + name.substring(4);
    }

    public String getName() {
        return name;
    }

    public String getDescription() {
        return description;
    }

    public Type getType() {
        return type;
    }

    public String invoke(Object o) {
        try {
            return format(method.invoke(o));
        } catch (InvocationTargetException | IllegalAccessException ex) {
            throw new QueryException(ex);
        }
    }

    private Type typeFromJavaType(Class<?> t) {
        if (Number.class.isAssignableFrom(t)
                || t == int.class
                || t == long.class
                || t == double.class
                || t == float.class
                || t == short.class)
            return Type.NUMBER;
        if (Date.class.isAssignableFrom(t))
            return Type.DATE;
        if (boolean.class.isAssignableFrom(t))
            return Type.BOOLEAN;
        return Type.WORD;
    }

    @Override
    public String format(Object value) {
        return formatter.format(value);
    }
}
