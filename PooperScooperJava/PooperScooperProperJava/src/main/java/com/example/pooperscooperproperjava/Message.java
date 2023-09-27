package com.example.pooperscooperproperjava;

import java.util.Date;

public class Message {
    private String text = "";
    private Date time = new Date();

    public String getText() {
        return text;
    }

    public void setText(String text) {
        this.text = text;
    }

    public Date getTime() {
        return time;
    }

    public void setTime(Date time) {
        this.time = time;
    }
}
