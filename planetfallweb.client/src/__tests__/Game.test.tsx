import React from 'react';
import {act, fireEvent, render, screen, waitFor} from '@testing-library/react';
import {DialogType, SessionHandler, useGameContext} from '@zork-ai/shared-types';
import {useMutation} from '@tanstack/react-query';
import Game from '../Game';
import Server from '../Server';

jest.mock('@tanstack/react-query', () => ({useMutation: jest.fn()}));
jest.mock('@fontsource/roboto', () => ({}));
jest.mock('../Server');
jest.mock('../components/GameInput', () => (props: {
    playerInput: string;
    setInput: (value: string) => void;
    handleKeyDown: (event: React.KeyboardEvent<HTMLInputElement>) => void;
}) => <input data-testid="game-input" value={props.playerInput}
            onChange={(event) => props.setInput(event.target.value)} onKeyDown={props.handleKeyDown}/>);
jest.mock('@zork-ai/shared-types', () => {
    const actual = jest.requireActual('@zork-ai/shared-types');
    return {
        ...actual,
        useGameContext: jest.fn(),
        SessionHandler: jest.fn(),
        Mixpanel: {track: jest.fn()},
    };
});

describe('Game', () => {
    const mutate = jest.fn();
    const session = {
        getSessionId: jest.fn(() => ['session-1', false]),
        getClientId: jest.fn(() => 'client-1'),
        regenerate: jest.fn(),
    };
    const server = {
        gameInit: jest.fn(),
        gameInput: jest.fn(),
        saveGame: jest.fn(),
        gameRestore: jest.fn(),
        deleteSavedGame: jest.fn(),
    };
    const context = {
        setDialogToOpen: jest.fn(),
        restartGame: false,
        setRestartGame: jest.fn(),
        saveGameRequest: undefined,
        setSaveGameRequest: jest.fn(),
        restoreGameRequest: undefined,
        setRestoreGameRequest: jest.fn(),
        deleteGameRequest: undefined,
        setDeleteGameRequest: jest.fn(),
        setCopyGameTranscript: jest.fn(),
    };
    let mutationOptions: {onSuccess: (response: unknown) => void};

    const response = (text: string) => ({
        response: text,
        locationName: 'Escape Pod (A)',
        score: 7,
        moves: 3,
        time: 845,
        inventory: ['towel'],
        exits: ['0'],
        actionsAvailableFromInventory: {towel: ['take towel']},
        actionsAvailableFromLocation: {webbing: ['enter webbing']},
    });

    beforeEach(() => {
        jest.clearAllMocks();
        Object.assign(context, {
            restartGame: false,
            saveGameRequest: undefined,
            restoreGameRequest: undefined,
            deleteGameRequest: undefined,
        });
        (SessionHandler as jest.Mock).mockImplementation(() => session);
        (Server as jest.MockedClass<typeof Server>).mockImplementation(() => server as unknown as Server);
        (useGameContext as jest.Mock).mockReturnValue(context);
        (useMutation as jest.Mock).mockImplementation((options) => {
            mutationOptions = options;
            return {mutate, isPending: false, isError: false};
        });
        server.gameInit.mockResolvedValue(response('Escape Pod (A)\n  Safety webbing fills the pod.'));
    });

    test('loads and formats the initial game state', async () => {
        render(<Game/>);

        await waitFor(() => expect(screen.getByTestId('header-location')).toHaveTextContent('Escape Pod (A)'));
        const rendered = screen.getAllByTestId('game-response').at(-1)!;
        expect(rendered.innerHTML).toContain('<span class="room-header">Escape Pod (A)</span>Safety webbing fills the pod.');
        expect(screen.getByTestId('inventory-button')).toBeInTheDocument();
    });

    test('submits trimmed input with the active session and records successful output', async () => {
        render(<Game/>);
        await waitFor(() => expect(server.gameInit).toHaveBeenCalled());

        fireEvent.change(screen.getByTestId('game-input'), {target: {value: '  north  '}});
        fireEvent.click(screen.getByTestId('go-button'));
        expect(mutate).toHaveBeenCalledWith(expect.objectContaining({input: 'north', sessionId: 'session-1'}));

        act(() => mutationOptions.onSuccess(response('You go north.')));
        expect(screen.getAllByTestId('game-response').at(-1)!.innerHTML).toContain('&gt;   north  ');
    });

    test.each([
        ['<Save>', DialogType.Save],
        ['<Restore>', DialogType.Restore],
        ['<Restart>', DialogType.Restart],
    ])('routes the %s response to its dialog', async (marker, dialog) => {
        server.gameInit.mockResolvedValue(response(marker));
        render(<Game/>);

        await waitFor(() => expect(context.setDialogToOpen).toHaveBeenCalledWith(dialog));
    });

    test('shows request failures from the mutation', async () => {
        (useMutation as jest.Mock).mockReturnValue({mutate, isPending: false, isError: true});
        render(<Game/>);

        expect(await screen.findByText(/Something went wrong/)).toBeInTheDocument();
    });
});
