export class SessionHandler {

    getSessionId(): [string, boolean] {

        let firstTime: boolean = false;

        if (!localStorage.getItem("SessionId")) {
            firstTime = true;
            localStorage.setItem("SessionId", this.generateRandomString());
        }

        return [localStorage.getItem("SessionId")!, firstTime];
    }

    getClientId(): string {

        if (!localStorage.getItem("ClientId")) {
            localStorage.setItem("ClientId", this.generateRandomString());
        }

        return localStorage.getItem("ClientId")!;
    }

    regenerate(): void {
        localStorage.setItem("SessionId", this.generateRandomString());
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