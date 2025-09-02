import axios, {AxiosResponse, RawAxiosRequestHeaders} from 'axios';
import {GameRequest} from './model/GameRequest.ts';
import {GameResponse} from "./model/GameResponse.ts";
import {ISaveGameRequest} from "./model/SaveGameRequest.ts";
import {ISavedGame} from "./model/SavedGame.ts";
import config from '../config.json';
import {RestoreGameRequest} from "./model/RestoreGameRequest.ts";
import {ISharedGame} from "./model/SharedGame.ts";
import {IShareGameRequest} from "./model/ShareGameRequest.ts";
import {Mixpanel} from './Mixpanel';
import {SessionHandler} from "./SessionHandler.ts";

export default class Server {

    baseUrl = config.base_url;
    sessionId = new SessionHandler();

    gameInput = async (input: GameRequest): Promise<GameResponse> => {

        const client = axios.create({
            baseURL: this.baseUrl
        });

        const response = await client.post<GameResponse, AxiosResponse>("", input, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        });
        Mixpanel.track('Played a Turn', {
            "score": response.data.score,
            "moves": response.data.moves,
            "location": response.data.locationName,
            "input": input.input,
            "output": response.data.response,
            "sessionId": input.sessionId,
            "clientId": this.sessionId.getClientId()
        });
        return response.data;
    };

    async getSavedGames(clientId: string): Promise<ISavedGame[]> {
        const client = axios.create({
            baseURL: this.baseUrl
        });

        try {
            const response = await client.get<ISavedGame[], AxiosResponse>("saveGame", {
                params: {
                    sessionId: clientId
                }
            });

            Mixpanel.track('Listed Saved Games', {
                "clientId": clientId,
                "gameCount": response.data.length
            });

            return response.data;
        } catch (error) {
            console.error('Server.ts: Error in getSavedGames:', error);
            throw error;
        }
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

        Mixpanel.track('Save Game', {
            "clientId": this.sessionId.getClientId(),
            "gameName": request.name,
            "sessionId": request.sessionId,
        });
        return response.data;
    }

    async gameRestore(restoreGameId: string, clientId: string, sessionId: string) {

        const client = axios.create({
            baseURL: this.baseUrl
        });

        const request = new RestoreGameRequest(restoreGameId, sessionId, clientId);

        const response = await client.post<GameResponse, AxiosResponse>("restoreGame", request, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        });

        Mixpanel.track('Restore Game', {
            "clientId": this.sessionId.getClientId(),
            "gameId": restoreGameId,
            "sessionId": request.sessionId,
        });

        return response.data;
    }

    async deleteSavedGame(id: string, sessionId: string): Promise<void> {
        const client = axios.create({
            baseURL: this.baseUrl
        });

        await client.delete(`saveGame/${id}`, {
            params: {
                sessionId: sessionId
            }
        });

        Mixpanel.track('Delete Saved Game', {
            "clientId": this.sessionId.getClientId(),
            "gameId": id,
            "sessionId": sessionId,
        });
    }

    async getSharedGames(sourceSessionId: string): Promise<ISharedGame[]> {
        const client = axios.create({
            baseURL: this.baseUrl
        });

        try {
            const response = await client.get<ISharedGame[], AxiosResponse>(`shared-games/${sourceSessionId}`, {
                headers: {
                    'Accept': 'application/json',
                } as RawAxiosRequestHeaders,
            });

            Mixpanel.track('Listed Shared Games', {
                "clientId": this.sessionId.getClientId(),
                "sourceSessionId": sourceSessionId,
                "gameCount": response.data.length
            });

            return response.data;
        } catch (error) {
            console.error('Server.ts: Error in getSharedGames:', error);
            throw error;
        }
    }

    async copySharedGame(request: IShareGameRequest): Promise<string> {
        const client = axios.create({
            baseURL: this.baseUrl
        });

        const response = await client.post<string, AxiosResponse>("shared-games/copy", request, {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        });

        Mixpanel.track('Copy Shared Game', {
            "clientId": this.sessionId.getClientId(),
            "sourceSessionId": request.sourceSessionId,
            "targetSessionId": request.targetSessionId,
            "sourceGameId": request.sourceGameId,
        });

        return response.data;
    }
}
