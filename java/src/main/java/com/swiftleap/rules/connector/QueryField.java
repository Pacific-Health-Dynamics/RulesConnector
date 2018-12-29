package com.swiftleap.rules.connector;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.METHOD)
public @interface QueryField {
    String value() default "";

    boolean ignore() default false;

    Class<? extends FieldFormatter> formatter() default FieldFormatter.DefaultFieldFormatter.class;
}
