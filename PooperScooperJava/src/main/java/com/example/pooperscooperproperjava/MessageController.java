package com.example.pooperscooperproperjava;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class MessageController {
    @Value("${app.message:No message set}")
    private String text;

    @GetMapping("/message")
    public Message getMessage() {
        var message = new Message();

        message.setText(text);

        return message;
    }
}
