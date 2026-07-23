import React from 'react';
import {fireEvent, render, screen, waitFor} from '@testing-library/react';
import {DialogType, ReleaseNotesServer, useGameContext} from '@zork-ai/shared-types';
import App from '../App';
import Server from '../Server';

jest.mock('../Game', () => () => <div data-testid="game"/>);
jest.mock('../Server');
jest.mock('../menu/GameMenu', () => (props: {latestVersion: string}) =>
    <div data-testid="game-menu">{props.latestVersion}</div>);
jest.mock('../modal/WelcomeModal', () => (props: {open: boolean}) =>
    <div data-testid="welcome-modal" data-open={props.open}/>);
jest.mock('@zork-ai/shared-types', () => {
    const actual = jest.requireActual('@zork-ai/shared-types');
    const modal = (testId: string) => (props: {open: boolean; games?: unknown[]; onConfirm?: () => void}) =>
        <div data-testid={testId} data-open={props.open} data-games={props.games?.length ?? 0}>
            {props.onConfirm && <button onClick={props.onConfirm}>confirm</button>}
        </div>;
    return {
        ...actual,
        useGameContext: jest.fn(),
        SessionHandler: jest.fn().mockImplementation(() => ({getClientId: () => 'client-1'})),
        ReleaseNotesServer: jest.fn(),
        Mixpanel: {track: jest.fn()},
        SaveModal: modal('save-modal'),
        RestoreModal: modal('restore-modal'),
        RestartConfirmDialog: modal('restart-modal'),
        VideoDialog: modal('video-modal'),
        ReleaseNotesModal: modal('release-notes-modal'),
    };
});

describe('App', () => {
    const server = {getSavedGames: jest.fn()};
    const context = {
        dialogToOpen: undefined as DialogType | undefined,
        setDialogToOpen: jest.fn(),
        setRestartGame: jest.fn(),
    };

    beforeEach(() => {
        jest.clearAllMocks();
        context.dialogToOpen = undefined;
        (useGameContext as jest.Mock).mockReturnValue(context);
        (Server as jest.MockedClass<typeof Server>).mockImplementation(() => server as unknown as Server);
        (ReleaseNotesServer as jest.Mock).mockResolvedValue([
            {date: '2026-07-22', name: 'v2.0', notes: 'More tests'}
        ]);
        server.getSavedGames.mockResolvedValue([{id: 'save-1'}]);
    });

    test('loads the latest release name on mount', async () => {
        render(<App/>);

        expect(await screen.findByTestId('game-menu')).toHaveTextContent('v2.0');
        expect(screen.getByTestId('game')).toBeInTheDocument();
    });

    test.each([
        [DialogType.Save, 'save-modal'],
        [DialogType.Restore, 'restore-modal'],
    ])('loads saved games before opening the %s dialog', async (dialog, testId) => {
        context.dialogToOpen = dialog;
        render(<App/>);

        await waitFor(() => expect(server.getSavedGames).toHaveBeenCalledWith('client-1'));
        expect(screen.getByTestId(testId)).toHaveAttribute('data-open', 'true');
        expect(screen.getByTestId(testId)).toHaveAttribute('data-games', '1');
        expect(context.setDialogToOpen).toHaveBeenCalledWith(undefined);
    });

    test.each([
        [DialogType.Video, 'video-modal'],
        [DialogType.Welcome, 'welcome-modal'],
        [DialogType.ReleaseNotes, 'release-notes-modal'],
    ])('opens the %s dialog without loading saves', async (dialog, testId) => {
        context.dialogToOpen = dialog;
        render(<App/>);

        await waitFor(() => expect(screen.getByTestId(testId)).toHaveAttribute('data-open', 'true'));
        expect(server.getSavedGames).not.toHaveBeenCalled();
    });

    test('confirms a restart through context', async () => {
        context.dialogToOpen = DialogType.Restart;
        render(<App/>);

        await waitFor(() => expect(screen.getByTestId('restart-modal')).toHaveAttribute('data-open', 'true'));
        fireEvent.click(screen.getByText('confirm'));
        expect(context.setRestartGame).toHaveBeenCalledWith(true);
    });
});
