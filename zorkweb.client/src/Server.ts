import axios, {AxiosResponse, RawAxiosRequestHeaders} from 'axios';
import {GameRequest} from './GameRequest';
import {GameResponse} from "./GameResponse.ts";

export default class Server {

    async gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {

        let here = location.href
            .replace("http:", "https:")
            .replace("127.0.0.1", "localhost")
            .replace("5173", "7007") + "ZorkOne";

        console.log(here);

        let client = axios.create({
            baseURL: here
        });

        return await client.post<GameResponse, AxiosResponse>("", input, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        })
    }

    async gameInit(sessionId: string): Promise<GameResponse> {

        let here = location.href
            .replace("http:", "https:")
            .replace("127.0.0.1", "localhost")
            .replace("5173", "7007") + "ZorkOne";

        console.log(here);

        let client = axios.create({
            baseURL: here
        });

        const response = await client.get<GameResponse, AxiosResponse>("", {
            params: {
                sessionId: sessionId
            }
        });
        return response.data;
    }
}