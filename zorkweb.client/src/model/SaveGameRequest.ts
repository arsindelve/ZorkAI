export interface ISaveGameRequest {
    clientId: string | undefined;
    name: string;
    sessionId: string | undefined;
    id: string | undefined;
}

export class SaveGameRequest implements ISaveGameRequest {

    clientId: string | undefined;
    name: string;
    sessionId: string | undefined;
    id: string | undefined;

    constructor(name: string, sessionId: string | undefined) {
        this.name = name;
        this.sessionId = sessionId;
    }

}


