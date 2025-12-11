interface IGameRequest {
    input: string;
    sessionId: string;
}

export class GameRequest implements IGameRequest {

    input: string;
    sessionId: string;

    constructor(input: string, sessionId: string) {
        this.input = input;
        this.sessionId = sessionId;

    }

}
