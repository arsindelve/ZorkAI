export interface ISharedGame {
    id: string;
    name: string;
    savedOn: Date;
}

export class SharedGame implements ISharedGame {
    id: string;
    name: string;
    savedOn: Date;

    constructor(id: string, name: string, savedOn: Date) {
        this.id = id;
        this.name = name;
        this.savedOn = savedOn;
    }
}