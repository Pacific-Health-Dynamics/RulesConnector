/*
 * Copyright Notice
 * ================
 * This file contains proprietary information of Pacific health Dynamics.
 * Copying or reproduction without prior written approval is prohibited.
 * Copyright (c) 2015
 *
 * Pacific health Dynamics
 * E-mail: info@pacificdynamics.com.au
 * www.pacificdynamics.com.au
 */

package com.swiftleap.rules.connector;

/**
 * Standard error returned to clients by the API services.
 * <p>
 * Created by ruans on 2015/08/18.
 */
public class Error {
    private String message;
    private int code;
    private String reference;

    public String getMessage() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public int getCode() {
        return code;
    }

    public void setCode(int code) {
        this.code = code;
    }

    public String getReference() {
        return reference;
    }

    public void setReference(String reference) {
        this.reference = reference;
    }
}
