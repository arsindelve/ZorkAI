export interface IRestoreGameRequest {
    sessionId: string;
    id: string;
    clientId: string;
}

export class RestoreGameRequest implements IRestoreGameRequest {

    sessionId: string;
    id: string;
    clientId: string;

    constructor(id: string, sessionId: string, clientId: string) {
        this.id = id;
        this.sessionId = sessionId;
        this.clientId = clientId;

    }

}
