export interface IShareGameRequest {
    sourceSessionId: string;
    targetSessionId: string;
    sourceGameId: string;
}

export class ShareGameRequest implements IShareGameRequest {
    sourceSessionId: string;
    targetSessionId: string;
    sourceGameId: string;

    constructor(sourceSessionId: string, targetSessionId: string, sourceGameId: string) {
        this.sourceSessionId = sourceSessionId;
        this.targetSessionId = targetSessionId;
        this.sourceGameId = sourceGameId;
    }
}