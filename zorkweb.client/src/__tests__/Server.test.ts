import axios from 'axios';
import {Mixpanel} from '@zork-ai/shared-types';
import Server from '../Server';

jest.mock('axios');
jest.mock('@zork-ai/shared-types', () => {
    const actual = jest.requireActual('@zork-ai/shared-types');
    return {
        ...actual,
        Mixpanel: {track: jest.fn()},
        SessionHandler: jest.fn().mockImplementation(() => ({getClientId: () => 'client-123'})),
    };
});

describe('Server', () => {
    const client = {
        get: jest.fn(),
        post: jest.fn(),
        delete: jest.fn(),
    };

    beforeEach(() => {
        jest.clearAllMocks();
        (axios.create as jest.Mock).mockReturnValue(client);
    });

    test('posts a turn and records the returned game state', async () => {
        const data = {response: 'Taken.', score: 5, moves: 2, locationName: 'Kitchen'};
        client.post.mockResolvedValue({data});

        const result = await new Server().gameInput({input: 'take lamp', sessionId: 'session-1'});

        expect(client.post).toHaveBeenCalledWith('', {input: 'take lamp', sessionId: 'session-1'}, {
            headers: {Accept: 'application/json'},
        });
        expect(Mixpanel.track).toHaveBeenCalledWith('Played a Turn', expect.objectContaining({
            input: 'take lamp', output: 'Taken.', clientId: 'client-123',
        }));
        expect(result).toBe(data);
    });

    test('loads the current session using the expected query parameter', async () => {
        const data = {response: 'West of House'};
        client.get.mockResolvedValue({data});

        await expect(new Server().gameInit('session-1')).resolves.toBe(data);
        expect(client.get).toHaveBeenCalledWith('', {
            params: {sessionId: 'session-1'},
            headers: {Accept: 'application/json'},
        });
    });

    test('lists saved games and records their count', async () => {
        const games = [{id: 'one'}, {id: 'two'}];
        client.get.mockResolvedValue({data: games});

        await expect(new Server().getSavedGames('client-1')).resolves.toBe(games);
        expect(client.get).toHaveBeenCalledWith('saveGame', {params: {sessionId: 'client-1'}});
        expect(Mixpanel.track).toHaveBeenCalledWith('Listed Saved Games', {
            clientId: 'client-1', gameCount: 2,
        });
    });

    test('rethrows saved-game listing failures', async () => {
        const error = new Error('offline');
        client.get.mockRejectedValue(error);
        jest.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(new Server().getSavedGames('client-1')).rejects.toBe(error);
    });

    test('saves, restores, and deletes using the API contract', async () => {
        client.post.mockResolvedValueOnce({data: 'Saved.'}).mockResolvedValueOnce({data: {response: 'Restored.'}});
        client.delete.mockResolvedValue({});
        const server = new Server();
        const saveRequest = {id: 'save-1', clientId: 'client-1', sessionId: 'session-1', name: 'My save'};

        await expect(server.saveGame(saveRequest)).resolves.toBe('Saved.');
        await expect(server.gameRestore('save-1', 'client-1', 'session-2')).resolves.toEqual({response: 'Restored.'});
        await server.deleteSavedGame('save-1', 'session-2');

        expect(client.post).toHaveBeenNthCalledWith(1, 'saveGame', saveRequest, {
            headers: {Accept: 'application/json'},
        });
        expect(client.post).toHaveBeenNthCalledWith(2, 'restoreGame', expect.objectContaining({
            id: 'save-1', clientId: 'client-1', sessionId: 'session-2',
        }), {headers: {Accept: 'application/json'}});
        expect(client.delete).toHaveBeenCalledWith('saveGame/save-1', {params: {sessionId: 'session-2'}});
        expect(Mixpanel.track).toHaveBeenCalledWith('Delete Saved Game', expect.objectContaining({gameId: 'save-1'}));
    });
});
