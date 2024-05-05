import axios, {AxiosResponse, RawAxiosRequestHeaders} from 'axios';
import {GameRequest} from './model/GameRequest.ts';
import {GameResponse} from "./model/GameResponse.ts";
import {ISaveGameRequest} from "./model/SaveGameRequest.ts";
import {ISavedGame} from "./model/SavedGame.ts";

export default class Server {

    baseUrl = "https://bxqzfka0hc.execute-api.us-east-1.amazonaws.com/Prod/ZorkOne"
    //baseUrl = "http://localhost:5000/ZorkOne";

    gameInput = async (input: GameRequest): Promise<GameResponse> => {

        const client = axios.create({
            baseURL: this.baseUrl
        });

        const response = await client.post<GameResponse, AxiosResponse>("", input, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        });

        return response.data;
    };

    async getSavedGames(sessionId: string): Promise<ISavedGame[]> {
        const client = axios.create({
            baseURL: this.baseUrl
        });

        const response = await client.get<ISavedGame[], AxiosResponse>("saveGame", {
            params: {
                sessionId: sessionId
            }
        });
        return response.data;
    }

    async gameInit(sessionId: string): Promise<GameResponse> {

        const client = axios.create({
            baseURL: this.baseUrl
        });

        const response = await client.get<GameResponse, AxiosResponse>("", {
            params: {
                sessionId: sessionId
            }, headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        });
        return response.data;
    }

    async saveGame(request: ISaveGameRequest): Promise<string> {

        const client = axios.create({
            baseURL: this.baseUrl
        });

        const response = await client.post<string, AxiosResponse>("saveGame", request, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        });

        return response.data;

    }
}