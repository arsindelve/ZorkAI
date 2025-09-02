import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import LoadSharedModal from '../modal/LoadSharedModal';
import { ISharedGame } from '../model/SharedGame';

// Mock SessionHandler
jest.mock('../SessionHandler', () => ({
    SessionHandler: jest.fn().mockImplementation(() => ({
        getClientId: () => 'TARGET123SESSION'
    }))
}));

// Mock Server
const mockGetSharedGames = jest.fn();
const mockCopySharedGame = jest.fn();

jest.mock('../Server', () => {
    return jest.fn().mockImplementation(() => ({
        getSharedGames: mockGetSharedGames,
        copySharedGame: mockCopySharedGame
    }));
});

describe('LoadSharedModal', () => {
    const mockSetOpen = jest.fn();
    const mockOnGameCopied = jest.fn();

    const mockSharedGames: ISharedGame[] = [
        {
            id: 'game1',
            name: 'My Adventure',
            savedOn: new Date('2024-01-15T10:30:00Z')
        },
        {
            id: 'game2', 
            name: 'Epic Quest (from share)',
            savedOn: new Date('2024-01-16T14:45:00Z')
        }
    ];

    beforeEach(() => {
        jest.clearAllMocks();
        mockGetSharedGames.mockClear();
        mockCopySharedGame.mockClear();
    });

    test('renders load shared modal correctly', () => {
        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        expect(screen.getByTestId('load-shared-game-modal')).toBeInTheDocument();
        expect(screen.getByText('Load Shared Game')).toBeInTheDocument();
        expect(screen.getByTestId('session-id-input')).toBeInTheDocument();
        expect(screen.getByText('Enter a Session ID to view and copy available saved games.')).toBeInTheDocument();
    });

    test('validates session ID input correctly', () => {
        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        const input = screen.getByTestId('session-id-input').querySelector('input');
        const loadButton = screen.getByTestId('load-shared-games-button');

        // Test empty input
        expect(loadButton).toBeDisabled();

        // Test invalid format (too short)
        fireEvent.change(input!, { target: { value: '123' } });
        expect(screen.getByText('Session ID must be exactly 15 alphanumeric characters')).toBeInTheDocument();
        expect(loadButton).toBeDisabled();

        // Test invalid format (special characters)
        fireEvent.change(input!, { target: { value: '123-456-789-012' } });
        expect(screen.getByText('Session ID must be exactly 15 alphanumeric characters')).toBeInTheDocument();

        // Test valid format
        fireEvent.change(input!, { target: { value: 'ABC123DEF456GHI' } });
        expect(screen.queryByText('Session ID must be exactly 15 alphanumeric characters')).not.toBeInTheDocument();
        expect(loadButton).toBeEnabled();
    });

    test('loads shared games successfully', async () => {
        mockGetSharedGames.mockResolvedValue(mockSharedGames);

        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        const input = screen.getByTestId('session-id-input').querySelector('input');
        const loadButton = screen.getByTestId('load-shared-games-button');

        fireEvent.change(input!, { target: { value: 'ABC123DEF456GHI' } });
        fireEvent.click(loadButton);

        expect(screen.getByText('Loading...')).toBeInTheDocument();

        await waitFor(() => {
            expect(mockGetSharedGames).toHaveBeenCalledWith('ABC123DEF456GHI');
        });

        expect(screen.getByText('Available Shared Games')).toBeInTheDocument();
        expect(screen.getByText('My Adventure')).toBeInTheDocument();
        expect(screen.getByText('Epic Quest (from share)')).toBeInTheDocument();
    });

    test('shows error when no games found', async () => {
        mockGetSharedGames.mockResolvedValue([]);

        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        const input = screen.getByTestId('session-id-input').querySelector('input');
        const loadButton = screen.getByTestId('load-shared-games-button');

        fireEvent.change(input!, { target: { value: 'ABC123DEF456GHI' } });
        fireEvent.click(loadButton);

        await waitFor(() => {
            expect(screen.getByText('No saved games found for this Session ID')).toBeInTheDocument();
        });
    });

    test('handles API error when loading games', async () => {
        mockGetSharedGames.mockRejectedValue(new Error('Network error'));

        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        const input = screen.getByTestId('session-id-input').querySelector('input');
        const loadButton = screen.getByTestId('load-shared-games-button');

        fireEvent.change(input!, { target: { value: 'ABC123DEF456GHI' } });
        fireEvent.click(loadButton);

        await waitFor(() => {
            expect(screen.getByText('Failed to load shared games. Please check the Session ID and try again.')).toBeInTheDocument();
        });
    });

    test('copies shared game successfully', async () => {
        mockGetSharedGames.mockResolvedValue(mockSharedGames);
        mockCopySharedGame.mockResolvedValue('new-game-id');

        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        // First load the games
        const input = screen.getByTestId('session-id-input').querySelector('input');
        const loadButton = screen.getByTestId('load-shared-games-button');

        fireEvent.change(input!, { target: { value: 'ABC123DEF456GHI' } });
        fireEvent.click(loadButton);

        await waitFor(() => {
            expect(screen.getByText('Available Shared Games')).toBeInTheDocument();
        });

        // Then copy a game
        const copyButton = screen.getByTestId('copy-game-game1');
        fireEvent.click(copyButton);

        expect(screen.getByText('Copying...')).toBeInTheDocument();

        await waitFor(() => {
            expect(mockCopySharedGame).toHaveBeenCalledWith({
                sourceSessionId: 'ABC123DEF456GHI',
                targetSessionId: 'TARGET123SESSION',
                sourceGameId: 'game1'
            });
            expect(mockOnGameCopied).toHaveBeenCalled();
            expect(mockSetOpen).toHaveBeenCalledWith(false);
        });
    });

    test('closes modal and resets state when cancelled', () => {
        render(
            <LoadSharedModal 
                open={true}
                setOpen={mockSetOpen}
                onGameCopied={mockOnGameCopied}
            />
        );

        const cancelButton = screen.getByText('Cancel');
        fireEvent.click(cancelButton);

        expect(mockSetOpen).toHaveBeenCalledWith(false);
    });
});