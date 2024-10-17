export interface IRestoreGameRequest {
    sessionId: string;
    id: string;
}

export class RestoreGameRequest implements IRestoreGameRequest {

    sessionId: string;
    id: string;

    constructor(id: string, sessionId: string) {
        this.id = id;
        this.sessionId = sessionId;

    }

}
