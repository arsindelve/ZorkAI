﻿export class SessionId {

    // Method
    getSessionId(): string {
        if (!localStorage.getItem("SessionId")) {
            localStorage.setItem("SessionId", this.generateRandomString());
        }

        return localStorage.getItem("SessionId")!
    }
    generateRandomString(): string {
        let randomString = '';
        const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

        for (let i = 0; i < 15; i++) {
            randomString += characters.charAt(Math.floor(Math.random() * characters.length));
        }

        return randomString;
    }
}