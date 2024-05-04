export interface ISaveGameRequest {
    name: string;
    sessionId: string;
}

export class SaveGameRequest implements ISaveGameRequest {

    name: string;
    sessionId: string;

    constructor(name: string, sessionId: string) {
        this.name = name;
        this.sessionId = sessionId;

    }

}
