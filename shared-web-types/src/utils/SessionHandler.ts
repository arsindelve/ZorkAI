export class SessionHandler {
    getSessionId(): [string, boolean] {
        let firstTime: boolean = false;

        const transferredSessionId = this.consumeTransferredSessionId();
        if (transferredSessionId) {
            localStorage.setItem('SessionId', transferredSessionId);
            return [transferredSessionId, false];
        }

        if (!localStorage.getItem('SessionId')) {
            firstTime = true;
            localStorage.setItem('SessionId', this.generateRandomString());
        }

        return [localStorage.getItem('SessionId')!, firstTime];
    }

    getClientId(): string {
        if (!localStorage.getItem('ClientId')) {
            localStorage.setItem('ClientId', this.generateRandomString());
        }

        return localStorage.getItem('ClientId')!;
    }

    regenerate(): void {
        localStorage.setItem('SessionId', this.generateRandomString());
    }

    generateRandomString(): string {
        let randomString = '';
        const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

        for (let i = 0; i < 15; i++) {
            randomString += characters.charAt(Math.floor(Math.random() * characters.length));
        }

        return randomString;
    }

    private consumeTransferredSessionId(): string | null {
        if (typeof window === 'undefined' || !window.location.hash.startsWith('#session='))
            return null;

        const transferredSessionId = new URLSearchParams(window.location.hash.slice(1)).get(
            'session',
        );
        if (!transferredSessionId || !/^[A-Za-z0-9]{15}$/.test(transferredSessionId)) return null;

        // The fragment is never sent to a web server. Remove it immediately after adoption so
        // copying the address bar cannot accidentally share control of a guest game session.
        window.history.replaceState(
            null,
            '',
            `${window.location.pathname}${window.location.search}`,
        );
        return transferredSessionId;
    }
}
