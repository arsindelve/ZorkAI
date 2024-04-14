import axios, {AxiosResponse, RawAxiosRequestHeaders} from 'axios';
import {GameRequest} from './GameRequest';
import {GameResponse} from "./GameResponse.ts";

export default class Server {

    async gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {

        let client = axios.create({
            baseURL: import.meta.env.VITE_BASE_URL
        });

        return client.post<GameResponse, AxiosResponse>("", input, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        })
    }

    async gameInit(sessionId: string): Promise<GameResponse> {

        let client = axios.create({
            baseURL: import.meta.env.VITE_BASE_URL
        });

        const response = await client.get<GameResponse, AxiosResponse>("", {
            params: {
                sessionId: sessionId
            }
        });
        return response.data;
    }
}