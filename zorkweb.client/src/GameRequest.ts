interface IGameRequest {
    input: string;
}

export class GameRequest implements IGameRequest {

    input: string;

    constructor(input: string) {
        this.input = input;

    }

}
